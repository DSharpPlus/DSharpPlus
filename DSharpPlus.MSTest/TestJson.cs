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
            // bool?
            Assert.AreEqual("[true]" , DiscordJson.SerializeObject(new[] { new Optional<bool?>(true) }));
            Assert.AreEqual("[false]", DiscordJson.SerializeObject(new[] { new Optional<bool?>(false) }));
            Assert.AreEqual("[]"     , DiscordJson.SerializeObject(new[] { new Optional<bool?>() }));
            // bool
            Assert.AreEqual("[true]" , DiscordJson.SerializeObject(new[] { new Optional<bool>(true) }));
            Assert.AreEqual("[false]", DiscordJson.SerializeObject(new[] { new Optional<bool>(false) }));
            Assert.AreEqual("[]"     , DiscordJson.SerializeObject(new[] { new Optional<bool>() }));
        }
    }
}