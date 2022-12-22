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

using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Lavalink.Entities;

public class LavalinkRouteStatus
{
    /// <summary>
    /// Gets the route planner type.
    /// </summary>
    [JsonIgnore]
    public LavalinkRoutePlannerType? Class
        => GetLavalinkRoutePlannerType(ClassInternal);

    /// <summary>
    /// Gets the details of the route planner.
    /// </summary>
    [JsonProperty("details", NullValueHandling = NullValueHandling.Ignore)]
    public LavalinkRouteStatusDetails Details { get; internal set; }

    [JsonProperty("class", NullValueHandling = NullValueHandling.Ignore)]
    internal string ClassInternal { get; set; }

    private LavalinkRoutePlannerType? GetLavalinkRoutePlannerType(string type) => type switch
    {
        "RotatingIpRoutePlanner" => LavalinkRoutePlannerType.RotatingIpRoutePlanner,
        "BalancingIpRoutePlanner" => LavalinkRoutePlannerType.BalancingIpRoutePlanner,
        "NanoIpRoutePlanner" => LavalinkRoutePlannerType.NanoIpRoutePlanner,
        "RotatingNanoIpRoutePlanner" => LavalinkRoutePlannerType.RotatingNanoIpRoutePlanner,
        _ => null,
    };
}

public class LavalinkRouteStatusDetails
{
    /// <summary>
    /// Gets the details for the current IP block.
    /// </summary>
    [JsonProperty("ipBlock", NullValueHandling = NullValueHandling.Ignore)]
    public LavalinkIpBlock IpBlock { get; internal set; }

    /// <summary>
    /// Gets the collection of failed addresses.
    /// </summary>
    [JsonProperty("failingAddresses", NullValueHandling = NullValueHandling.Ignore)]
    public IEnumerable<LavalinkFailedAddress> FailedAddresses { get; internal set; }

    /// <summary>
    /// Gets the number of rotations since the restart of Lavalink.
    /// <para>Only present in the <see cref="LavalinkRoutePlannerType.RotatingIpRoutePlanner"/>.</para>
    /// </summary>
    [JsonProperty("rotateIndex", NullValueHandling = NullValueHandling.Ignore)]
    public string RotateIndex { get; internal set; }

    /// <summary>
    /// Gets the current offset of the IP block.
    /// <para>Only present in the <see cref="LavalinkRoutePlannerType.RotatingIpRoutePlanner"/>.</para>
    /// </summary>
    [JsonProperty("ipIndex", NullValueHandling = NullValueHandling.Ignore)]
    public string IpIndex { get; internal set; }

    /// <summary>
    /// Gets the current IP Address used by the planner.
    /// <para>Only present in the <see cref="LavalinkRoutePlannerType.RotatingIpRoutePlanner"/>.</para>
    /// </summary>
    [JsonProperty("currentAddress", NullValueHandling = NullValueHandling.Ignore)]
    public string CurrentAddress { get; internal set; }

    /// <summary>
    /// Gets the current offset of the IP block.
    /// <para>Only present in the <see cref="LavalinkRoutePlannerType.NanoIpRoutePlanner"/> and the <see cref="LavalinkRoutePlannerType.RotatingNanoIpRoutePlanner"/>.</para>
    /// </summary>
    [JsonProperty("currentAddressIndex", NullValueHandling = NullValueHandling.Ignore)]
    public long CurrentAddressIndex { get; internal set; }

    /// <summary>
    /// Gets the information in which /64 block ips are chosen. This number increases on each ban.
    /// <para>Only present in the <see cref="LavalinkRoutePlannerType.RotatingNanoIpRoutePlanner"/>.</para>
    /// </summary>
    [JsonProperty("blockIndex", NullValueHandling = NullValueHandling.Ignore)]
    public string BlockIndex { get; internal set; }
}

public struct LavalinkIpBlock
{
    /// <summary>
    /// Gets the type of the IP block.
    /// </summary>
    [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
    public string Type { get; internal set; }

    /// <summary>
    /// Gets the size of the IP block.
    /// </summary>
    [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
    public string Size { get; internal set; }
}

public struct LavalinkFailedAddress
{
    /// <summary>
    /// Gets the failed address IP.
    /// </summary>
    [JsonProperty("address", NullValueHandling = NullValueHandling.Ignore)]
    public string Address { get; internal set; }

    /// <summary>
    /// Gets the failing timestamp in miliseconds.
    /// </summary>
    [JsonProperty("failingTimestamp", NullValueHandling = NullValueHandling.Ignore)]
    public ulong FailingTimestamp { get; internal set; }

    /// <summary>
    /// Gets the DateTime format of the failing address.
    /// </summary>
    [JsonProperty("failingTime", NullValueHandling = NullValueHandling.Ignore)]
    public string FailingTime { get; internal set; }
}
