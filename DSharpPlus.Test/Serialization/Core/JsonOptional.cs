using System.Text.Json;
using DSharpPlus.Core;
using DSharpPlus.Core.JsonConverters;
using NUnit.Framework;

namespace DSharpPlus.Test.Serialization.Core
{
    public class JsonOptional
    {
        private static readonly JsonSerializerOptions stjOptions = new()
        {
            TypeInfoResolver = DiscordJsonTypeInfoResolver.Default
        };

        [Test]
        public void Deserialize()
        {
            RecordWithOptional? empty = JsonSerializer.Deserialize<RecordWithOptional>("{}", stjOptions);
            Assert.That(empty, Is.EqualTo(new RecordWithOptional(Optional<int>.Empty)));

            RecordWithOptional? withFive = JsonSerializer.Deserialize<RecordWithOptional>(@"{""Value"": 5}", stjOptions);
            Assert.That(withFive, Is.EqualTo(new RecordWithOptional(5)));
        }

        [Test]
        public void Serialize()
        {
            string empty = JsonSerializer.Serialize(new RecordWithOptional(Optional<int>.Empty), stjOptions);
            Assert.That(empty, Is.EqualTo("{}"));

            string withFive = JsonSerializer.Serialize(new RecordWithOptional(5), stjOptions);
            Assert.That(withFive, Is.EqualTo(@"{""Value"":5}"));
        }

        public record RecordWithOptional(Optional<int> Value);
    }
}
