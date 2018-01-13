using DSharpPlus.Entities;
using DSharpPlus.Net.Abstractions;
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

        [TestMethod]
        public void TestOptionalSerialization2()
        {
            var p = new RestGuildModifyPayload();
            Assert.AreEqual("{}", DiscordJson.SerializeObject(p));
            Assert.AreEqual("{}", DiscordJson.SerializeObject(new RestGuildModifyPayload()
            {
                SystemChannelId = default,
            }));
            Assert.AreEqual(@"{""name"":""doobilip"",""system_channel_id"":null}", DiscordJson.SerializeObject(new RestGuildModifyPayload()
            {
                Name = "doobilip",
                SystemChannelId = null,
            }));
            Assert.AreEqual(@"{""name"":""doobilip2"",""system_channel_id"":420}", DiscordJson.SerializeObject(new RestGuildModifyPayload()
            {
                Name = "doobilip2",
                SystemChannelId = 420,
            }));
        }
    }
}