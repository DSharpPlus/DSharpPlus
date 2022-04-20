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
using System.IO;
using System.Reflection;
using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.Gateway.Payloads;
using DSharpPlus.Core.JsonConverters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Test.Serialization.Core
{
    [TestClass]
    public class TestPayloadSerialization
    {
        [TestMethod]
        public void TestPayloadSerializationAsync()
        {
            Dictionary<string, string> serialized = new();
            Dictionary<string, Type?> payloadMap = new() {
                { "HEARTBEAT", typeof(int?) },
                { "RECONNECT", null },
            };

            foreach (Type type in typeof(DiscordReadyPayload).Assembly.GetExportedTypes())
            {
                DiscordGatewayEventNameAttribute? eventName = type.GetCustomAttribute<DiscordGatewayEventNameAttribute>();
                if (eventName == null)
                {
                    continue;
                }

                foreach (string name in eventName.Names)
                {
                    payloadMap.Add(name, type);
                }
            }

            JsonSerializer jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings() { Converters = new List<JsonConverter>() { new DiscordSnowflakeConverter(), new OptionalConverter() } });
            string filePath = "../../../../discord-payloads"; // This should be fine as long as we use the dotnet command instead of moving the executable and executing it.
            foreach (string file in Directory.GetFiles(Path.Join(Environment.CurrentDirectory, filePath), "*.json", new EnumerationOptions() { RecurseSubdirectories = true }))
            {
                string json = File.ReadAllText(file);
                JObject jObject = JObject.Parse(json);
                if (!jObject.TryGetValue("t", out JToken? t) || t == null)
                {
                    throw new InvalidDataException($"Unable to find property 't' in file {file}");
                }

                string eventName = t.Value<string>()!;
                if (!payloadMap.TryGetValue(eventName, out Type? payloadType) || payloadType == null)
                {
                    throw new InvalidOperationException($"No payload type found for event {eventName}!");
                }

                try
                {
                    using TextReader reader = File.OpenText(file);
                    object serializedObject = jsonSerializer.Deserialize(reader, payloadType)!;
                    serialized.Add(file, payloadType.Name);
                }
                catch (Exception error)
                {
                    Console.WriteLine($"Error on {file} ({payloadType.FullName}): {error.Message}");
                }
            }

            Console.WriteLine($"Serialized {serialized.Keys.Count}/{payloadMap.Keys.Count} payloads!");
            Assert.IsTrue(serialized.Keys.Count == Directory.GetFiles(Path.Join(Environment.CurrentDirectory, filePath), "*.json", new EnumerationOptions() { RecurseSubdirectories = true }).Length);
        }
    }
}
