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

namespace DSharpPlus.Enums
{

    [Flags]
    public enum UserFlags
    {
        /// <summary>
        /// The user has no flags.
        /// </summary>
        None = 0,

        /// <summary>
        /// The user is a Discord Employee.
        /// </summary>
        Staff = 1 << 0,

        /// <summary>
        /// The user is a Partnered Server Owner.
        /// </summary>
        Partner = 1 << 1,

        /// <summary>
        /// The user is a HypeSquad Events Coordinator.
        /// </summary>
        Hypesquad = 1 << 2,

        /// <summary>
        /// The user is a Level 1 Bug Hunter.
        /// </summary>
        BugHunterLevel1 = 1 << 3,

        /// <summary>
        /// The user is part of the Hypesquad House of Bravery.
        /// </summary>
        HypesquadBravery = 1 << 6,

        /// <summary>
        /// The user is part of the Hypesquad House of Brilliance.
        /// </summary>
        HypesquadBrilliance = 1 << 7,

        /// <summary>
        /// The user is part of the Hypesquad House of Balance.
        /// </summary>
        HypesquadBalance = 1 << 8,

        /// <summary>
        /// The user is Early Nitro Supporter.
        /// </summary>
        NitroEarlySupporter = 1 << 9,

        /// <summary>
        /// The user is a team.
        /// </summary>
        TeamPseudoUser = 1 << 10,

        /// <summary>
        /// The user is a Level 2 Bug Hunter.
        /// </summary>
        BugHunterLevel2 = 1 << 14,

        /// <summary>
        /// The user is a Verified Bot.
        /// </summary>
        VerifiedBot = 1 << 16,

        /// <summary>
        /// The user is a Verified Bot Developer.
        /// </summary>
        VerifiedDeveloper = 1 << 17,

        /// <summary>
        /// The user is a Discord Certified Moderator.
        /// </summary>
        CertifiedModerator = 1 << 18,

        /// <summary>
        /// The user is a bot that uses only HTTP interactions and is shown in the online member list.
        /// </summary>
        BotHttpInteractions = 1 << 19,
    }
}
