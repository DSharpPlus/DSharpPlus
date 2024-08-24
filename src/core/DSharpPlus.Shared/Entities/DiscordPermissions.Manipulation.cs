// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Runtime.Intrinsics;

namespace DSharpPlus.Entities;

// contains the code for manipulating DiscordPermissions, via operators and method calls. all of these are copying operations.

partial struct DiscordPermissions
{
    // ADD-REMOVE
    // add is equivalent to or, remove is an and-not operation

    /// <summary>
    /// Adds a permission to this permission set.
    /// </summary>
    /// <param name="permission">The permission field to add.</param>
    /// <returns>A new permission set containing all previous permissions along with the new permission.</returns>
    public DiscordPermissions Add(DiscordPermission permission)
        => new(this.AsSpan, [permission], []);

    /// <summary>
    /// Adds multiple permissions to this permission set.
    /// </summary>
    /// <param name="permissions">The permission fields to add.</param>
    /// <returns>A new permission set containing all previous permissions along with the new permissions.</returns>
    public DiscordPermissions Add(params ReadOnlySpan<DiscordPermission> permissions)
        => new(this.AsSpan, permissions, []);

    /// <summary>
    /// Adds the specified permission to the permission set.
    /// </summary>
    public static DiscordPermissions operator +(DiscordPermissions value, DiscordPermission add) => value.Add(add);

    /// <summary>
    /// Removes a permission from this permission set.
    /// </summary>
    /// <param name="permission">The permission field to remove.</param>
    /// <returns>A new permission set containing all previous permissions except the specified permission.</returns>
    public DiscordPermissions Remove(DiscordPermission permission)
        => new(this.AsSpan, [], [permission]);

    /// <summary>
    /// Removes multiple permission from this permission set.
    /// </summary>
    /// <param name="permissions">The permission fields to remove.</param>
    /// <returns>A new permission set containing all previous permissions except the specified permissions.</returns>
    public DiscordPermissions Remove(params ReadOnlySpan<DiscordPermission> permissions)
        => new(this.AsSpan, [], permissions);

    /// <summary>
    /// Removes the specified permission from a permission set.
    /// </summary>
    public static DiscordPermissions operator -(DiscordPermissions value, DiscordPermission remove) => value.Remove(remove);

    /// <summary>
    /// Removes all permissions specified in the right-hand set from the left-hand set and returns the new permission set.
    /// </summary>
    public static DiscordPermissions operator -(DiscordPermissions left, DiscordPermissions right)
    {
        ReadOnlySpan<byte> current = left.AsSpan;
        ReadOnlySpan<byte> other = right.AsSpan;
        Span<byte> result = stackalloc byte[ContainerByteCount];

        for (int i = 0; i < ContainerByteCount; i += 16)
        {
            Vector128<byte> v1 = Vector128.LoadUnsafe(in current[i]);
            Vector128<byte> v2 = Vector128.LoadUnsafe(in other[i]);

            Vector128<byte> removed = Vector128.AndNot(v1, v2);
            removed.StoreUnsafe(ref result[i]);
        }

        return new(result);
    }

    // OR
    // we can short-circuit single-flag OR into Add

    /// <summary>
    /// Adds the specified permission to the permission set.
    /// </summary>
    public static DiscordPermissions operator |(DiscordPermissions value, DiscordPermission flag) => value.Add(flag);

    /// <summary>
    /// Merges both permission sets into one, taking all set permissions from both.
    /// </summary>
    public static DiscordPermissions operator |(DiscordPermissions left, DiscordPermissions right)
    {
        ReadOnlySpan<byte> current = left.AsSpan;
        ReadOnlySpan<byte> other = right.AsSpan;
        Span<byte> result = stackalloc byte[ContainerByteCount];

        for (int i = 0; i < ContainerByteCount; i += 16)
        {
            Vector128<byte> v1 = Vector128.LoadUnsafe(in current[i]);
            Vector128<byte> v2 = Vector128.LoadUnsafe(in other[i]);

            Vector128<byte> or = Vector128.BitwiseOr(v1, v2);
            or.StoreUnsafe(ref result[i]);
        }

        return new(result);
    }

    // AND

    /// <summary>
    /// Returns a permission set containing either no value if the flag was not found, or exactly the specified flag if it was.
    /// </summary>
    public static DiscordPermissions operator &(DiscordPermissions value, DiscordPermission flag)
        => value.HasFlag(flag) ? new(flag) : None;

    /// <summary>
    /// Performs a bitwise AND between the two permission sets.
    /// </summary>
    public static DiscordPermissions operator &(DiscordPermissions left, DiscordPermissions right)
    {
        ReadOnlySpan<byte> current = left.AsSpan;
        ReadOnlySpan<byte> other = right.AsSpan;
        Span<byte> result = stackalloc byte[ContainerByteCount];

        for (int i = 0; i < ContainerByteCount; i += 16)
        {
            Vector128<byte> v1 = Vector128.LoadUnsafe(in current[i]);
            Vector128<byte> v2 = Vector128.LoadUnsafe(in other[i]);

            Vector128<byte> and = Vector128.BitwiseAnd(v1, v2);
            and.StoreUnsafe(ref result[i]);
        }

        return new(result);
    }

    // XOR

    /// <summary>
    /// Toggles the specified flag in the permission set.
    /// </summary>
    public static DiscordPermissions operator ^(DiscordPermissions value, DiscordPermission flag)
    {
        bool currentFlagValue = value.GetFlag((int)flag);
        bool newFlagValue = currentFlagValue ^ true;

        return new(value, (int)flag, newFlagValue);
    }

    /// <summary>
    /// Performs a bitwise XOR operation between the two permission sets.
    /// </summary>
    public static DiscordPermissions operator ^(DiscordPermissions left, DiscordPermissions right)
    {
        ReadOnlySpan<byte> current = left.AsSpan;
        ReadOnlySpan<byte> other = right.AsSpan;
        Span<byte> result = stackalloc byte[ContainerByteCount];

        for (int i = 0; i < ContainerByteCount; i += 16)
        {
            Vector128<byte> v1 = Vector128.LoadUnsafe(in current[i]);
            Vector128<byte> v2 = Vector128.LoadUnsafe(in other[i]);

            Vector128<byte> xor = Vector128.Xor(v1, v2);
            xor.StoreUnsafe(ref result[i]);
        }

        return new(result);
    }

    // NOT

    /// <summary>
    /// Negates the specified permission set.
    /// </summary>
    /// <remarks>
    /// Since <see cref="DiscordPermission.Administrator"/> is a normal flag for the intents and purposes of this method,
    /// it is recommended to be careful uploading return values to the Discord API.
    /// </remarks>
    public static DiscordPermissions operator ~(DiscordPermissions value)
    {
        ReadOnlySpan<byte> current = value.AsSpan;
        Span<byte> result = stackalloc byte[ContainerByteCount];

        for (int i = 0; i < ContainerByteCount; i += 16)
        {
            Vector128<byte> vec = Vector128.LoadUnsafe(in current[i]);

            Vector128<byte> not = ~vec;
            not.StoreUnsafe(ref result[i]);
        }

        return new(result);
    }
}
