// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.Entities;
using DSharpPlus.Core.Entities.Gateway.Payloads;
using DSharpPlus.Core.JsonConverters;
using NUnit.Framework;

namespace DSharpPlus.Test.Serialization.Core
{
    public class TestPayloadSerialization
    {
        private static readonly JsonSerializerOptions stjOptions = new()
        {
            TypeInfoResolver = DiscordJsonTypeInfoResolver.Default
        };

        private static readonly Regex NormalizeJsonWhitespace = new("(\"(?:[^\"\\\\]|\\\\.)*\")|\\s+", RegexOptions.Compiled);
        private static readonly Dictionary<string, Type?> payloadMap;

        static TestPayloadSerialization()
        {
            payloadMap = new()
            {
                { "HEARTBEAT", typeof(int?) }, // Doesn't include fields, only returns an int? which is null on first heartbeat.
                { "RECONNECT", null }, // Reconnect doesn't have a payload.
            };

            // Retrieve all gateway payloads. Some payloads are entities, some are custom classes. All payloads have the `DiscordGatewayPayloadAttribute`, so we can use reflection to grab them.
            foreach (Type type in typeof(DiscordReadyPayload).Assembly.GetExportedTypes())
            {
                // Skip non-payload types
                DiscordGatewayPayloadAttribute? eventName = type.GetCustomAttribute<DiscordGatewayPayloadAttribute>();
                if (eventName == null)
                {
                    continue;
                }

                // Some entities can represent multiple payloads, so we add the entity by payload name instead of by type.
                foreach (string name in eventName.Names)
                {
                    if (!payloadMap.TryAdd(name, type))
                    {
                        Debug.Fail($"Duplicate payload name \"{name}\". Types: {type.FullName}, {payloadMap[name]!.FullName}");
                    }
                }
            }
        }

        public static IEnumerable<TestCase> TestCases => Directory.GetFiles(Path.GetFullPath("../../../../discord-payloads", Environment.CurrentDirectory), "*.json", new EnumerationOptions() { RecurseSubdirectories = true }).Select(f => new TestCase(f));

        [TestCaseSource(nameof(TestCases))]
        public void TestPayloadSerializationAsync(TestCase test)
        {
            string file = test.File;
            using FileStream fileStream = File.OpenRead(file);
            JsonElement jsonElement = (JsonElement)JsonSerializer.Deserialize<object>(fileStream, stjOptions)!;

            // If the `t` (payload type) field is not present, it's invalid data. This should only happen when discord-payloads includes rest payloads, and tests should be modified to include them in a separate method.
            if (!jsonElement.TryGetProperty("t", out JsonElement typeElement))
            {
                throw new InvalidDataException($"Unable to find property 't' in file {file}");
            }

            // Attempt to see if the payload type is in the payload map. If it isn't, it's a new payload and we should include it within the next few commits.
            string eventName = typeElement.GetString()!;
            if (!payloadMap.TryGetValue(eventName, out Type? payloadType) || payloadType == null)
            {
                Assert.Fail($"Payload type {eventName} is not in the payload map. Please add it to the payload map.");
                return;
            }

            object serializedObject = JsonSerializer.Deserialize(jsonElement.GetProperty("d").GetRawText(), payloadType, stjOptions)!;
            VerifyObjectValues(serializedObject, jsonElement.GetProperty("d"));

            Console.WriteLine($"Serialized payload! Event Name: {eventName}");
        }

        private static void VerifyObjectValues(object obj, JsonElement jsonElement, string? accessedProperty = null)
        {
            Type objType = obj.GetType();
            accessedProperty ??= objType.Name; // Start the accessed property with the type name, then add onto which properties down the road.
            PropertyInfo[] properties = objType.GetProperties(); // Match the properties to the json, removing them when they're successfully matched.
            List<PropertyInfo> trueProperties = properties.ToList(); // We need to keep a copy of the properties list, as we'll be removing items from it.

            foreach (JsonProperty currentProperty in jsonElement.EnumerateObject())
            {
                string normalizedJson = NormalizeJsonWhitespace.Replace(currentProperty.Value.GetRawText(), "$1"); // Normalize whitespace in the json.
                foreach (PropertyInfo property in properties)
                {
                    JsonPropertyNameAttribute? jsonPropertyName = property.GetCustomAttribute<JsonPropertyNameAttribute>();
                    if (jsonPropertyName == null) // Make sure the property is intended to be (de)serialized. If it's not the correct property, temporarily skip it.
                    {
                        trueProperties.Remove(property);
                        continue;
                    }
                    else if (jsonPropertyName.Name != currentProperty.Name)
                    {
                        continue;
                    }

                    Assert.That(jsonPropertyName.Name, Is.EqualTo(currentProperty.Name));

                    object? propertyValue = property.GetValue(obj);
                    JsonElement currentElement = jsonElement.GetProperty(currentProperty.Name);
                    if (propertyValue != null)
                    {
                        if (currentElement.ValueKind == JsonValueKind.Object) // If the property is an object, recurse into it.
                        {
                            VerifyObjectValues(propertyValue, currentElement, $"{accessedProperty}.{property.Name}");
                        }
                        else if (currentElement.ValueKind == JsonValueKind.Array) // If the property is an array, check to see if it holds any objects.
                        {
                            JsonElement.ArrayEnumerator arrayEnumerator = currentElement.EnumerateArray();
                            if (arrayEnumerator.MoveNext() && arrayEnumerator.Current.ValueKind == JsonValueKind.Object) // If the array is empty or doesn't contain objects, skip it.
                            {
                                VerifyObjectValues(propertyValue, arrayEnumerator.Current, $"{accessedProperty}.{property.Name}");
                            }
                        }

                        // Optional verification
                        if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Optional<>))
                        {
                            if (HasOptionalValue(propertyValue))
                            {
                                Assert.That(
                                    currentElement.ValueKind != JsonValueKind.Undefined,
                                    "{0}.{1} holds a value, but the json does not.\nProperty Value Read: {2}\nJson Value Read: {3}",
                                    accessedProperty, property.Name, propertyValue, currentElement);
                            }
                            else
                            {
                                Assert.That(
                                    currentElement.ValueKind == JsonValueKind.Undefined,
                                    "{0}.{1} does not hold a value, but the json is does.\nProperty Value Read: {2}\nJson Value Read: {3}",
                                    accessedProperty, property.Name, propertyValue, currentElement);

                                // We don't want to write any empty optionals to the json.
                                RemoveFromTrueProperties();
                                continue;
                            }
                        }
                    }

                    string jsonValue = JsonSerializer.Serialize(propertyValue, property.PropertyType, stjOptions);
                    if (jsonValue != normalizedJson)
                    {
                        switch (propertyValue)
                        {
                            // We're doing this because the json serializer isn't precise enough or ToString("o") is too precise.
                            // STJ (not precise enough):
                            //  Error on DSharpPlus.Core.Gateway.Payloads.DiscordTypingStartPayload: Property DiscordTypingStartPayload.Member.JoinedAt does not have a matching value:
                            //    Property Value Written: "2022-01-16T01:06:06.869+00:00"
                            //    Json Value Read:        "2022-01-16T01:06:06.869000+00:00"
                            // ToString("o") (too precise):
                            //  Error on DSharpPlus.Core.Gateway.Payloads.DiscordTypingStartPayload: Property DiscordTypingStartPayload.Member.JoinedAt does not have a matching value:
                            //    Property Value Written: "2022-01-16T01:06:06.8690000+00:00"
                            //    Json Value Read:        "2022-01-16T01:06:06.869000+00:00"
                            // This is why we use roundtrip verification.
                            case DateTimeOffset:
                            case Optional<DateTimeOffset>:
                            // Test Unicode emojis.
                            case string when $"{accessedProperty}.{property.Name}".Contains("Emoji.Name") && !propertyValue!.Equals(normalizedJson) && normalizedJson.StartsWith("\"\\u", true, CultureInfo.InvariantCulture):
                            case Optional<string> when $"{accessedProperty}.{property.Name}".Contains("Emoji.Name") && !propertyValue!.Equals(normalizedJson) && normalizedJson.StartsWith("\"\\u", true, CultureInfo.InvariantCulture):
                                if (!RoundtripVerify(propertyValue))
                                {
                                    jsonValue = JsonSerializer.Serialize(obj, stjOptions);
                                    normalizedJson = JsonSerializer.Serialize(JsonSerializer.Deserialize(jsonValue, obj.GetType(), stjOptions)); // I'm relatively sure this is incorrect.
                                    goto default;
                                }
                                break;
                            default:
                                Assert.Fail($"Property {accessedProperty}.{property.Name} does not have a matching value:\n\tProperty Value Written: {jsonValue}\n\tJson Value Read: {normalizedJson}");
                                break;
                        }
                    }

                    RemoveFromTrueProperties();

                    void RemoveFromTrueProperties()
                    {
                        Assert.That(
                            trueProperties.Remove(property),
                            "Property {0}.{1} was not found in the list of properties. Does this mean they're duplicate properties?",
                            accessedProperty, property.Name);
                    }
                }
            }

            if (trueProperties.Count != 0)
            {
                foreach (PropertyInfo property in trueProperties)
                {
                    Assert.That((property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Optional<>))
                        || (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        || (property.DeclaringType != null && property.DeclaringType.IsGenericType && property.DeclaringType.GetGenericTypeDefinition() == typeof(Optional<>)),
                        "Property {0}.{1} was not found in the json and is not optional. Do Discord docs need to be updated, or do we have incorrect code?",
                        accessedProperty, property.Name);
                }
            }
        }

        public static bool HasOptionalValue(object value) => (bool)value.GetType().GetProperty("HasValue")!.GetValue(value)!;

        private static bool RoundtripVerify(object obj) => obj.Equals(JsonSerializer.Deserialize(JsonSerializer.Serialize(obj, stjOptions), obj.GetType(), stjOptions));

        public sealed record TestCase(string File)
        {
            public override string ToString() => Path.GetFileName(File);
        }
    }
}
