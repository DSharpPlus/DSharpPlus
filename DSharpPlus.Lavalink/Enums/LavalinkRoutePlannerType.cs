namespace DSharpPlus.Lavalink;

public enum LavalinkRoutePlannerType
{
    /// <summary>
    /// Route planner that switches the IP on ban.
    /// </summary>
    RotatingIpRoutePlanner = 1,

    /// <summary>
    /// Route planner that selects random IP addresses from the given block.
    /// </summary>
    BalancingIpRoutePlanner = 2,

    /// <summary>
    /// Route planner that switches the IP on every clock update.
    /// </summary>
    NanoIpRoutePlanner = 3,

    /// <summary>
    /// Route planner that switches the IP on every clock update and rotates to next IP block on a ban as a fallback.
    /// </summary>
    RotatingNanoIpRoutePlanner = 4
}
