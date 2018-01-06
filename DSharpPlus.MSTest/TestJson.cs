using DSharpPlus.Entities;
using DSharpPlus.Net.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace DSharpPlus.Test
{
    [TestClass]
    public class TestJson
    {
        [TestMethod]
        public void TestOptionalSerialization()
        {
            // ulong
            Assert.AreEqual("[0]", DiscordJson.SerializeObject(new[] { new Optional<ulong?>(0UL) }));
            Assert.AreEqual("[]" , DiscordJson.SerializeObject(new[] { new Optional<ulong?>() }));
            // bool
            Assert.AreEqual("[true]" , DiscordJson.SerializeObject(new[] { new Optional<bool?>(true) }));
            Assert.AreEqual("[false]", DiscordJson.SerializeObject(new[] { new Optional<bool?>(false) }));
            Assert.AreEqual("[]"     , DiscordJson.SerializeObject(new[] { new Optional<bool?>() }));
            
            Assert.AreEqual(@"[{""HasValue"":true,""Value"":0}]", DiscordJson.SerializeObject(new[] { new Optional<ulong>(0UL) }));
            // `System.InvalidOperationException: Value is not set.` wrapped in JsonSerializationException 
            Assert.ThrowsException<JsonSerializationException>(() =>
            {
                DiscordJson.SerializeObject(new[] {new Optional<ulong>()});
            });
        }
    }
}