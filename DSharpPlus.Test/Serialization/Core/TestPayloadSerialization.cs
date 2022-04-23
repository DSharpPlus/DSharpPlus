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
            Dictionary<string, Type?> payloadMap = new() {
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
                    payloadMap.Add(name, type);
                }
            }

            Dictionary<string, string> serializedPayloads = new();
            JsonSerializer jsonSerializer = JsonSerializer.Create();

            // Using Path.GetFullPath should be fine as long as we use the `dotnet test` command instead of moving the executable and executing it.
            string[] files = Directory.GetFiles(Path.GetFullPath("../../../../discord-payloads", Environment.CurrentDirectory), "*.json", new EnumerationOptions() { RecurseSubdirectories = true });
            foreach (string file in files)
            {
                string json = File.ReadAllText(file);
                JObject jObject = JObject.Parse(json);

                // If the `t` (payload type) field is not present, it's invalid data. This should only happen when discord-payloads includes rest payloads, and tests should be modified to include them in a separate method.
                if (!jObject.TryGetValue("t", out JToken? t) || t == null)
                {
                    throw new InvalidDataException($"Unable to find property 't' in file {file}");
                }

                // Attempt to see if the payload type is in the payload map. If it isn't, it's a new payload and we should include it within the next few commits.
                string eventName = t.Value<string>()!;
                if (!payloadMap.TryGetValue(eventName, out Type? payloadType) || payloadType == null)
                {
                    throw new InvalidOperationException($"No payload type found for event {eventName}!");
                }

                try
                {
                    using TextReader stringReader = new StringReader(json);
                    object serializedObject = jsonSerializer.Deserialize(stringReader, payloadType)!;

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
    }
}
