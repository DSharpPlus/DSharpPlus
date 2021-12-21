// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2021 DSharpPlus Contributors
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

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Constructs embed fields.
    /// </summary>
    public sealed class DiscordEmbedFieldBuilder
    {
        /// <summary>
        /// Gets or sets the field's name.
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    if (_name == null)
                        throw new ArgumentNullException(nameof(value));
                    throw new ArgumentException("Name cannot be empty or whitespace.", nameof(value));
                }

                if (value.Length > 256)
                    throw new ArgumentException("Name length cannot exceed 256 characters.", nameof(value));

                _name = value;
            }
        }
        private string _name;

        /// <summary>
        /// Gets or sets the field's value.
        /// </summary>
        public string Value
        {
            get => _value;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    if (value == null)
                        throw new ArgumentNullException(nameof(value));
                    throw new ArgumentException("Value cannot be empty or whitespace.", nameof(value));
                }

                if (value.Length > 1024)
                    throw new ArgumentException("Value length cannot exceed 1024 characters.", nameof(value));

                _value = value;
            }
        }
        private string _value;

        /// <summary>
        /// Gets or sets whether or not this field should display inline.
        /// </summary>
        public bool Inline { get; set; }

        /// <summary>
        /// Constructs a new embed field.
        /// </summary>
        public DiscordEmbedFieldBuilder()
        {
            this.Inline = false;
        }

        /// <summary>
        /// Sets the embed field's name.
        /// </summary>
        /// <param name="name">Name to set.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedFieldBuilder WithName(string name)
        {
            this.Name = name;
            return this;
        }

        /// <summary>
        /// Sets the embed field's value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedFieldBuilder WithValue(string value)
        {
            this.Value = value;
            return this;
        }

        /// <summary>
        /// Constructs a new embed field from data supplied to this builder.
        /// </summary>
        /// <returns>New discord embed field.</returns>
        public DiscordEmbedField Build()
        {
            var field = new DiscordEmbedField()
            {
                Name = this._name,
                Value = this._value,
                Inline = this.Inline
            };

            return field;
        }

        /// <summary>
        /// Implicitly converts this builder to an embed field.
        /// </summary>
        /// <param name="builder">Builder to convert.</param>
        public static implicit operator DiscordEmbedField(DiscordEmbedFieldBuilder builder)
            => builder?.Build();
    }
}
