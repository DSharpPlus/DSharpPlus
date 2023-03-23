// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023 DSharpPlus Contributors
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
using DSharpPlus.EventArgs;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{

    /// <summary>
    /// Represents a button that can be pressed. Fires <see cref="ComponentInteractionCreateEventArgs"/> when pressed.
    /// </summary>
    public sealed class DiscordButtonComponent : DiscordComponent
    {
        /// <summary>
        /// The style of the button.
        /// </summary>
        [JsonProperty("style", NullValueHandling = NullValueHandling.Ignore)]
        public ButtonStyle Style { get; internal set; }

        /// <summary>
        /// The text to apply to the button. If this is not specified <see cref="Emoji"/> becomes required.
        /// </summary>
        [JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
        public string Label { get; internal set; }

        /// <summary>
        /// Whether this button can be pressed.
        /// </summary>
        [JsonProperty("disabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool Disabled { get; internal set; }

        /// <summary>
        /// The emoji to add to the button. Can be used in conjunction with a label, or as standalone. Must be added if label is not specified.
        /// </summary>
        [JsonProperty("emoji", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordComponentEmoji Emoji { get; internal set; }

        /// <summary>
        /// Enables this component if it was disabled before.
        /// </summary>
        /// <returns>The current component.</returns>
        public DiscordButtonComponent Enable()
        {
            this.Disabled = false;
            return this;
        }

        /// <summary>
        /// Disables this component.
        /// </summary>
        /// <returns>The current component.</returns>
        public DiscordButtonComponent Disable()
        {
            this.Disabled = true;
            return this;
        }

        /// <summary>
        /// Constructs a new <see cref="DiscordButtonComponent"/>.
        /// </summary>
        internal DiscordButtonComponent()
        {
            this.Type = ComponentType.Button;
        }


        /// <summary>
        /// Constucts a new button based on another button.
        /// </summary>
        /// <param name="other">The button to copy.</param>
        public DiscordButtonComponent(DiscordButtonComponent other) : this()
        {
            this.CustomId = other.CustomId;
            this.Style = other.Style;
            this.Label = other.Label;
            this.Disabled = other.Disabled;
            this.Emoji = other.Emoji;
        }

        /// <summary>
        /// Constructs a new button with the specified options.
        /// </summary>
        /// <param name="style">The style/color of the button.</param>
        /// <param name="customId">The Id to assign to the button. This is sent back when a user presses it.</param>
        /// <param name="label">The text to display on the button, up to 80 characters. Can be left blank if <paramref name="emoji"/>is set.</param>
        /// <param name="disabled">Whether this button should be initialized as being disabled. User sees a greyed out button that cannot be interacted with.</param>
        /// <param name="emoji">The emoji to add to the button. This is required if <paramref name="label"/> is empty or null.</param>
        public DiscordButtonComponent(ButtonStyle style, string customId, string label, bool disabled = false, DiscordComponentEmoji emoji = null)
        {
            this.Style = style;
            this.Label = label;
            this.CustomId = customId;
            this.Disabled = disabled;
            this.Emoji = emoji;
            this.Type = ComponentType.Button;
        }
    }
}
