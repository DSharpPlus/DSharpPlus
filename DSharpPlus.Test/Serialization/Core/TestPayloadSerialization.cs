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
using DSharpPlus.Core.GatewayEntities.Payloads;
using DSharpPlus.Core.RestEntities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DSharpPlus.Test.Serialization.Core
{
    [TestClass]
    public class TestPayloadSerialization
    {
        private static readonly Regex NormalizeJsonWhitespace = new("(\"(?:[^\"\\\\]|\\\\.)*\")|\\s+", RegexOptions.Compiled);

        [TestMethod]
        public void TestPayloadSerializationAsync()
        {
            Dictionary<string, Type?> payloadMap = new()
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

            Dictionary<string, string> serializedPayloads = new();

            // Using Path.GetFullPath should be fine as long as we use the `dotnet test` command instead of moving the executable and executing it.
            string[] files = Directory.GetFiles(Path.GetFullPath("../../../../discord-payloads", Environment.CurrentDirectory), "*.json", new EnumerationOptions() { RecurseSubdirectories = true });
            foreach (string file in files)
            {
                using FileStream fileStream = File.OpenRead(file);
                JsonElement jsonElement = (JsonElement)JsonSerializer.Deserialize<object>(fileStream)!;

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

                try
                {
                    object serializedObject = JsonSerializer.Deserialize(jsonElement.GetProperty("d").GetRawText(), payloadType)!;
                    VerifyObjectValues(serializedObject, jsonElement.GetProperty("d"));

                    // If the above method doesn't throw, then the payload has serialized correctly.
                    serializedPayloads.Add(file, payloadType.Name);
                }
                catch (Exception error)
                {
                    Console.WriteLine($"Error on {payloadType.FullName} ({file}): {error.Message}");
                }
            }

            Console.WriteLine($"Serialized {serializedPayloads.Keys.Count}/{files.Length} payloads!");
            Assert.IsTrue(serializedPayloads.Keys.Count == files.Length);
        }

        private static void VerifyObjectValues(object obj, JsonElement jsonElement, string? accessedProperty = null)
        {
            JsonElement.ObjectEnumerator objEnumerator = jsonElement.EnumerateObject(); // Iterate through the json, not the class

            Type objType = obj.GetType();
            accessedProperty ??= objType.Name; // Start the accessed property with the type name, then add onto which properties down the road.
            PropertyInfo[] properties = objType.GetProperties(); // Match the properties to the json, removing them when they're successfully matched.
            List<PropertyInfo> trueProperties = properties.ToList(); // We need to keep a copy of the properties list, as we'll be removing items from it.
            while (objEnumerator.MoveNext())
            {
                string normalizedJson = NormalizeJsonWhitespace.Replace(objEnumerator.Current.Value.GetRawText(), "$1"); // Normalize whitespace in the json.
                foreach (PropertyInfo property in properties)
                {
                    JsonPropertyNameAttribute? jsonPropertyName = property.GetCustomAttribute<JsonPropertyNameAttribute>();
                    if (jsonPropertyName == null) // Make sure the property is intended to be (de)serialized. If it's not the correct property, temporarily skip it.
                    {
                        trueProperties.Remove(property);
                        continue;
                    }
                    else if (jsonPropertyName.Name != objEnumerator.Current.Name)
                    {
                        continue;
                    }

                    object? propertyValue = property.GetValue(obj);
                    JsonElement currentElement = jsonElement.GetProperty(objEnumerator.Current.Name);
                    if (propertyValue != null)
                    {
                        if (currentElement.ValueKind == JsonValueKind.Object) // If the property is an object, recurse into it.
                        {
                            VerifyObjectValues(propertyValue, currentElement, $"{accessedProperty}.{property.Name}");
                            continue;
                        }
                        else if (currentElement.ValueKind == JsonValueKind.Array) // If the property is an array, check to see if it holds any objects.
                        {
                            JsonElement.ArrayEnumerator arrayEnumerator = currentElement.EnumerateArray();
                            if (arrayEnumerator.MoveNext() && arrayEnumerator.Current.ValueKind == JsonValueKind.Object) // If the array is empty or doesn't contain objects, skip it.
                            {
                                VerifyObjectValues(propertyValue, arrayEnumerator.Current, $"{accessedProperty}.{property.Name}");
                            }
                            continue;
                        }
                    }

                    // Optional verification
                    if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Optional<>))
                    {
                        if (propertyValue is Optional<object> optionalObject)
                        {
                            if (!optionalObject.HasValue)
                            {
                                if (currentElement.GetProperty(jsonPropertyName.Name).ValueKind != JsonValueKind.Undefined)
                                {
                                    throw new InvalidDataException($"{accessedProperty}.{property.Name} does not hold a value, but the json is does.\nProperty Value Read: {optionalObject}\nJson Value Read: {currentElement.GetProperty(jsonPropertyName.Name).GetString()}");
                                }
                                // We don't want to write any empty optionals to the json.
                                continue;
                            }
                            else if (optionalObject.HasValue && currentElement.GetProperty(jsonPropertyName.Name).ValueKind == JsonValueKind.Undefined)
                            {
                                throw new InvalidDataException($"{accessedProperty}.{property.Name} holds a value, but the json does not.\nProperty Value Read: {optionalObject}\nJson Value Read: {currentElement.GetProperty(jsonPropertyName.Name).GetString()}");
                            }
                        }
                        else if (propertyValue is Optional<ValueType> optionalValueType)
                        {
                            if (!optionalValueType.HasValue)
                            {
                                if (currentElement.GetProperty(jsonPropertyName.Name).ValueKind != JsonValueKind.Undefined)
                                {
                                    throw new InvalidDataException($"{accessedProperty}.{property.Name} does not hold a value, but the json is does.\nProperty Value Read: {optionalValueType}\nJson Value Read: {currentElement.GetProperty(jsonPropertyName.Name).GetString()}");
                                }
                                // We don't want to write any empty optionals to the json.
                                continue;
                            }
                            else if (optionalValueType.HasValue && currentElement.GetProperty(jsonPropertyName.Name).ValueKind == JsonValueKind.Undefined)
                            {
                                throw new InvalidDataException($"{accessedProperty}.{property.Name} holds a value, but the json does not.\nProperty Value Read: {optionalValueType}\nJson Value Read: {currentElement.GetProperty(jsonPropertyName.Name).GetString()}");
                            }
                        }
                    }

                    string jsonValue = JsonSerializer.Serialize(propertyValue, property.PropertyType);
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
                                    jsonValue = JsonSerializer.Serialize(obj);
                                    normalizedJson = JsonSerializer.Serialize(JsonSerializer.Deserialize(jsonValue, obj.GetType())); // I'm relatively sure this is incorrect.
                                    goto default;
                                }
                                break;
                            default:
                                throw new InvalidDataException($"Property {accessedProperty}.{property.Name} does not have a matching value:\n\tProperty Value Written: {jsonValue}\n\tJson Value Read: {normalizedJson}");
                        }
                    }

                    if (!trueProperties.Remove(property))
                    {
                        throw new InvalidDataException($"Property {accessedProperty}.{property.Name} was not found in the list of properties. Does this mean they're duplicate properties?");
                    }
                }
            }

            if (trueProperties.Count != 0)
            {
                foreach (PropertyInfo property in trueProperties)
                {
                    if ((property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Optional<>))
                        || (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        || (property.DeclaringType != null && property.DeclaringType.IsGenericType && property.DeclaringType.GetGenericTypeDefinition() == typeof(Optional<>))
                    )
                    {
                        continue;
                    }

                    throw new InvalidDataException($"Property {accessedProperty}.{property.Name} was not found in the json and is not optional. Do Discord docs need to be updated, or do we have incorrect code?");
                }
            }
        }

        private static bool RoundtripVerify(object obj) => obj.Equals(JsonSerializer.Deserialize(JsonSerializer.Serialize(obj), obj.GetType()));
    }
}
