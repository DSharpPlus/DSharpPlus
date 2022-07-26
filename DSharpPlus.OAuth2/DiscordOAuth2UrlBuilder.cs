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
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using DSharpPlus.Net;

namespace DSharpPlus.OAuth2
{
    public class DiscordOAuth2UrlBuilder
    {
        private readonly ulong _clientId;
        private DiscordScopes? _scopes = null;
        private string _redirectUri = "";
        private string _state = "";

        public DiscordOAuth2UrlBuilder(ulong clientId)
        {
            this._clientId = clientId;
        }

        public DiscordOAuth2UrlBuilder AddScope(DiscordScopes scope)
        {
            if (_scopes == null)
                _scopes = scope;
            else if(!_scopes.Value.HasFlag(scope))
                _scopes |= scope;

            return this;
        }

        public DiscordOAuth2UrlBuilder WithRedirectUri(string redirectUri)
        {
            this._redirectUri = redirectUri;
            return this;
        }

        public DiscordOAuth2UrlBuilder WithState(string state)
        {
            this._state = state;
            return this;
        }

        
        public string Build()
        {
            var url = Utilities.GetApiUriFor($"{Endpoints.OAUTH2}{Endpoints.AUTHORIZE}");

            var query = new StringBuilder();
            query.Append("?client_id=" + _clientId.ToString(CultureInfo.InvariantCulture));

            if (!string.IsNullOrEmpty(this._redirectUri))
                query.Append("&redirect_uri=" + HttpUtility.UrlEncode(this._redirectUri));

            if (!string.IsNullOrEmpty(this._state))
                query.Append("&state=" + HttpUtility.UrlEncode(this._state));

            // This translates the enum's name from CamelCase to whatever.this.is
            var scopes = _scopes.HasValue? _scopes.Value.GetScopeString() : "";

            if (!string.IsNullOrEmpty(scopes))
                query.Append("&scopes=" + HttpUtility.UrlEncode(scopes));

            return new UriBuilder(url)
            {
                Query = query.ToString()
            }.ToString();
        }
    }
}
