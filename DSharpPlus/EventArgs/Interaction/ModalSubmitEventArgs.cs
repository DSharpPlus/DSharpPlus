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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace DSharpPlus.EventArgs
{
    /// <summary>
    /// Fired when a modal is submitted. Note that this event is fired only if the modal is submitted by the user, and not if the modal is closed.
    /// </summary>
    public class ModalSubmitEventArgs : InteractionCreateEventArgs
    {
        /// <summary>
        /// A dictionary of submitted fields, keyed on the custom id of the input component.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyDictionary<string, string> Values { get; }

        internal ModalSubmitEventArgs(DiscordInteraction interaction)
        {
            this.Interaction = interaction;

            var dict = new Dictionary<string, string>();

            foreach (var component in interaction.Data._components)
                if (component.Components.First() is TextInputComponent input)
                    dict.Add(input.CustomId, input.Value);

            this.Values = dict;
        }
    }
}
