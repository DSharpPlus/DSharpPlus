using DSharpPlus.Entities;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace DSharpPlus.Test
{
    public class TestFixture1
    {
        public Optional<ulong?> Entry { get; set; }
    }
    public class TestFixture2
    {
        public Optional<bool?> Entry { get; set; }
    }
    public class TestFixture3
    {
        public Optional<bool> Entry { get; set; }
    }
    
    [TestClass]
    public class TestJson
    {
        [TestMethod]
        public void TestOptionalSerialization()
        {
            // ulong
            Assert.AreEqual(@"{""Entry"":0}", DiscordJson.SerializeObject(new TestFixture1 { Entry = new Optional<ulong?>(0UL) }));
            Assert.AreEqual(@"{}" , DiscordJson.SerializeObject(new TestFixture1 { Entry = new Optional<ulong?>() }));
            // bool?
            Assert.AreEqual(@"{""Entry"":true}" , DiscordJson.SerializeObject(new TestFixture2 { Entry = new Optional<bool?>(true) }));
            Assert.AreEqual(@"{""Entry"":false}", DiscordJson.SerializeObject(new TestFixture2 { Entry = new Optional<bool?>(false) }));
            Assert.AreEqual(@"{}"     , DiscordJson.SerializeObject(new TestFixture2 { Entry = new Optional<bool?>() }));
            // bool
            Assert.AreEqual(@"{""Entry"":true}" , DiscordJson.SerializeObject(new TestFixture3 { Entry = new Optional<bool>(true) }));
            Assert.AreEqual(@"{""Entry"":false}", DiscordJson.SerializeObject(new TestFixture3 { Entry = new Optional<bool>(false) }));
            Assert.AreEqual(@"{}"     , DiscordJson.SerializeObject(new TestFixture3 { Entry = new Optional<bool>() }));
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