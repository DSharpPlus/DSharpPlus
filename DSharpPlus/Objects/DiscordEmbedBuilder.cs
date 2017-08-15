using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus
{
    public class DiscordEmbedBuilder
    {
        DiscordEmbed embed;

        public DiscordEmbedBuilder()
        {
            embed = new DiscordEmbed();
        }

        public DiscordEmbedBuilder(DiscordEmbed original)
        {
            embed = original;
        }

        public string Title
        {
            get => embed.Title;
            set
            {
                if (value?.Length > 256) throw new ArgumentException("Embed title only allows a max of 256 characters!");
                embed.Title = value;
            }
        }

        public string Description
        {
            get => embed.Description;
            set
            {
                if (value?.Length > 2048) throw new ArgumentException("Embed description only allows a max of 2048 characters!");
                embed.Description = value;
            }
        }

        public string Url
        {
            get => embed.Url;
            set
            {
                embed.Url = value;
            }
        }

        public int Color
        {
            get => embed.Color;
            set
            {
                embed.Color = value;
            }
        }

        public List<DiscordEmbedField> Fields
        {
            get => embed.Fields;
            set
            {
                if (Fields.Count > 25)
                    throw new ArgumentException("Embed only allows a maximum of 25 fields!");
                embed.Fields = Fields;
            }
        }

        public DiscordEmbed GetEmbed()
        {
            return embed;
        }

        public DiscordEmbedBuilder SetTitle(string title)
        {
            if (title.Length > 256)
                throw new ArgumentException("Embed title only allows a max of 256 characters!");
            embed.Title = title ?? throw new ArgumentException("Null values are not allowed!");
            return this;
        }

        public DiscordEmbedBuilder SetDescription(string description)
        {
            if (description.Length > 2048)
                throw new ArgumentException("Embed description only allows a max of 2048 characters!");
            embed.Description = description ?? throw new ArgumentException("Null values are not allowed!");
            return this;
        }

        public DiscordEmbedBuilder SetAuthor(string name = "", string url = "", string iconurl = "")
        {
            embed.Author = new DiscordEmbedAuthor()
            {
                IconUrl = iconurl ?? throw new ArgumentException("Null values are not allowed!"),
                Name = name ?? throw new ArgumentException("Null values are not allowed!"),
                Url = url ?? throw new ArgumentException("Null values are not allowed!")
            };
            return this;
        }

        public DiscordEmbedBuilder SetColor(string color)
        {
            if (color == null)
                throw new ArgumentException("Null values are not allowed!");
            string TrimmedColor = color.Trim('#');
            embed.Color = int.Parse(TrimmedColor, System.Globalization.NumberStyles.HexNumber);
            return this;
        }

        public DiscordEmbedBuilder SetColor(int color)
        {
            embed.Color = color;
            return this;
        }

        public DiscordEmbedBuilder SetColor(int r, int g, int b)
        {
            if (r > 255 || g > 255 || b > 255 || r < 0 || g < 0 || b < 0)
                throw new ArgumentException("R, G and B should each be under 255 and above -1!");
            embed.Color = (r << 16) | (g << 8) | b;
            return this;
        }

        public DiscordEmbedBuilder AddField(string name, string value, bool inline = false)
        {
            if (name.Length > 256)
                throw new ArgumentException("Embed Field Name only allows a max of 256 characters!");
            if (value.Length > 1024)
                throw new ArgumentException("Embed Field Value only allows a max of 1024 characters!");
            if (embed.Fields == null)
                embed.Fields = new List<DiscordEmbedField>();
            else if (embed.Fields.Count == 25)
                throw new ArgumentException("Embed only allows a maximum of 25 fields!");

            embed.Fields.Add(new DiscordEmbedField()
            {
                Name = name ?? throw new ArgumentException("Null values are not allowed!"),
                Value = value ?? throw new ArgumentException("Null values are not allowed!"),
                Inline = inline
            });

            return this;
        }

        public DiscordEmbedBuilder SetFields(List<DiscordEmbedField> fields)
        {
            if (fields == new List<DiscordEmbedField>() || fields.Count < 1)
                throw new ArgumentException("Please use ClearFields() instead of an empty list!");
            if (fields.Count > 25)
                throw new ArgumentException("Embed only allows a maximum of 25 fields!");
            embed.Fields = fields ?? throw new ArgumentException("Null values are not allowed!");
            return this;
        }

        public DiscordEmbedBuilder ClearFields()
        {
            embed.Fields = new List<DiscordEmbedField>();
            return this;
        }

        public DiscordEmbedBuilder SetFooter(string text = "", string iconurl = "")
        {
            if (text.Length > 2048)
                throw new ArgumentException("Footer Text only allows a max of 2048 characters!");

            embed.Footer = new DiscordEmbedFooter()
            {
                IconUrl = iconurl ?? throw new ArgumentException("Null values are not allowed!"),
                Text = text ?? throw new ArgumentException("Null values are not allowed!")
            };
            return this;
        }

        public DiscordEmbedBuilder SetImage(string imageurl = "")
        {
            embed.Image = new DiscordEmbedImage()
            {
                Url = imageurl ?? throw new ArgumentException("Null values are not allowed!")
            };
            return this;
        }

        public DiscordEmbedBuilder SetThumbnail(string imageurl = "")
        {
            embed.Thumbnail = new DiscordEmbedThumbnail()
            {
                Url = imageurl ?? throw new ArgumentException("Null values are not allowed!")
            };
            return this;
        }

        public DiscordEmbedBuilder SetTimestamp(DateTimeOffset timestamp)
        {
            embed.Timestamp = timestamp;
            return this;
        }

        public DiscordEmbedBuilder SetTimestamp(DateTime timestamp)
        {
            embed.Timestamp = (DateTimeOffset)timestamp;
            return this;
        }

        public DiscordEmbedBuilder SetTimestamp(ulong discordsnowflake)
        {
            embed.Timestamp = new DateTimeOffset(2015, 1, 1, 0, 0, 0, TimeSpan.Zero).AddMilliseconds(discordsnowflake >> 22);
            return this;
        }

        public DiscordEmbedBuilder SetTimestamp(TimeSpan timespan, bool increase)
        {
            var dt = DateTimeOffset.Now;
            if (increase)
                dt = dt.Add(timespan);
            else
                dt = dt.Subtract(timespan);
            embed.Timestamp = dt;
            return this;
        }

        public DiscordEmbedBuilder SetUrl(string url = "")
        {
            embed.Url = url ?? throw new ArgumentException("Null values are not allowed!");
            return this;
        }

    }
}
