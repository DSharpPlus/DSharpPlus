#pragma warning disable IDE0040

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Text;

using CommunityToolkit.HighPerformance.Helpers;

using DSharpPlus.Net.Serialization;

using Newtonsoft.Json;

using HashCode = CommunityToolkit.HighPerformance.Helpers.HashCode<uint>;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a set of Discord permissions.
/// </summary>
[JsonConverter(typeof(DiscordPermissionsAsStringJsonConverter))]
[DebuggerDisplay("{ToString(\"name\")}")]
public partial struct DiscordPermissions
    : IEquatable<DiscordPermissions>, IEnumerable<DiscordPermission>
{
    // only change ContainerWidth here, the other two constants are automatically updated for internal uses
    // for ContainerWidth, 1 width == 128 bits.
    private const int ContainerWidth = 1;
    private const int ContainerElementCount = ContainerWidth * 4;
    private const int ContainerByteCount = ContainerWidth * 16;

    private static readonly string[] permissionNames = CreatePermissionNameArray();
    private static readonly int highestDefinedValue = (int)DiscordPermissionExtensions.GetValues()[^1];

    private DiscordPermissionContainer data;

    /// <summary>
    /// Creates a new instance of this type from exactly the specified permission.
    /// </summary>
    public DiscordPermissions(DiscordPermission permission)
        => this.data.SetFlag((int)permission, true);

    /// <summary>
    /// Creates a new instance of this type from the specified permissions.
    /// </summary>
    [OverloadResolutionPriority(1)]
    public DiscordPermissions(params ReadOnlySpan<DiscordPermission> permissions)
    {
        foreach (DiscordPermission permission in permissions)
        {
            this.data.SetFlag((int)permission, true);
        }
    }

    /// <summary>
    /// Creates a new instance of this type from the specified permissions.
    /// </summary>
    [OverloadResolutionPriority(0)]
    public DiscordPermissions(IReadOnlyList<DiscordPermission> permissions)
    {
        foreach (DiscordPermission permission in permissions)
        {
            this.data.SetFlag((int)permission, true);
        }
    }

    /// <summary>
    /// Creates a new instance of this type from the specified big integer. This assumes that the data is unsigned.
    /// </summary>
    public DiscordPermissions(BigInteger permissionSet)
    {
        Span<byte> buffer = MemoryMarshal.Cast<uint, byte>(MemoryMarshal.CreateSpan(ref this.data[0], ContainerElementCount));

        if (!permissionSet.TryWriteBytes(buffer, out _, isUnsigned: true))
        {
            // we don't want to fail in release mode, which would break perfectly working code because the library
            // hasn't been updated to support a new permission or because Discord is testing in prod again.
            // seeing this assertion in dev should be an indication to expand this type.
            Debug.Assert(false, "The amount of permissions DSharpPlus can represent has been exceeded.");
        }
    }

    /// <summary>
    /// Creates a new instance of this type from the specified raw data. This assumes that the data is unsigned.
    /// </summary>
    public DiscordPermissions(scoped ReadOnlySpan<byte> raw)
    {
        Span<byte> buffer = MemoryMarshal.Cast<uint, byte>(MemoryMarshal.CreateSpan(ref this.data[0], ContainerElementCount * 4));

        if (!raw.TryCopyTo(buffer))
        {
            // we don't want to fail in release mode, which would break perfectly working code because the library
            // hasn't been updated to support a new permission or because Discord is testing in prod again.
            // seeing this assertion in dev should be an indication to expand this type.
            Debug.Assert(false, "The amount of permissions DSharpPlus can represent has been exceeded.");
        }
    }

    /// <summary>
    /// A copy constructor that sets one specific flag to the specified value.
    /// </summary>
    private DiscordPermissions(DiscordPermissions original, int index, bool flag)
    {
        this.data = original.data;
        this.data.SetFlag(index, flag);
    }

    public static implicit operator DiscordPermissions(DiscordPermission initial) => new(initial);

    /// <summary>
    /// Returns an empty Discord permission set.
    /// </summary>
    public static DiscordPermissions None => default;

    /// <summary>
    /// Returns a full Discord permission set with all flags set to true.
    /// </summary>
    public static DiscordPermissions AllBitsSet
    {
        get
        {
            Span<byte> result = stackalloc byte[ContainerByteCount];

            for (int i = 0; i < ContainerElementCount; i += 16)
            {
                Vector128.StoreUnsafe(Vector128<byte>.AllBitsSet, ref result[i]);
            }

            return new(result);
        }
    }

    /// <summary>
    /// Returns a Discord permission set with all documented permissions set to true.
    /// </summary>
    public static DiscordPermissions All { get; } = new(DiscordPermissionExtensions.GetValues());

    [UnscopedRef]
    private readonly ReadOnlySpan<byte> AsSpan
        => MemoryMarshal.Cast<uint, byte>(this.data);

    private readonly bool GetFlag(int index)
        => this.data.HasFlag(index);

    /// <summary>
    /// Determines whether this Discord permission set is equal to the provided object.
    /// </summary>
    public override readonly bool Equals([NotNullWhen(true)] object? obj)
        => obj is DiscordPermissions permissions && Equals(permissions);

    /// <summary>
    /// Determines whether this Discord permission set is equal to the provided Discord permission set.
    /// </summary>
    public readonly bool Equals(DiscordPermissions other)
        => ((ReadOnlySpan<uint>)this.data).SequenceEqual(other.data);

    /// <summary>
    /// Returns a string representation of this permission set.
    /// </summary>
    public override readonly string ToString() => ToString("a placeholder format string that doesn't do anything");

    /// <summary>
    /// Returns a string representation of this permission set, according to the provided format string.
    /// </summary>
    /// <param name="format">
    /// Specifies the format in which the string should be created. Currently supported formats are: <br/>
    /// - <c>raw</c>: This prints the raw, byte-wise backing data of this instance. <br/>
    /// - <c>name</c>: This prints each flag by name, separated by commas. <br/>
    /// - <c>name:format</c>: This prints each flag by name according to the specified format. The string <c>{permission}</c> must be contained to mark the position of the flag. <br/>
    /// - anything else will print the integer value contained in this <see cref="DiscordPermissions"/> instance.
    /// </param>
    public readonly string ToString(string format)
    {
        if (format == "raw")
        {
            StringBuilder builder = new("DiscordPermissions - raw value:");

            foreach (byte b in this.AsSpan)
            {
                _ = builder.Append(' ');
                _ = builder.Append(b.ToString("x2", CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }
        else if (format == "name")
        {
            int pop = 0;

            for (int i = 0; i < ContainerElementCount; i += 4)
            {
                pop += BitOperations.PopCount(this.data[i]);
                pop += BitOperations.PopCount(this.data[i + 1]);
                pop += BitOperations.PopCount(this.data[i + 2]);
                pop += BitOperations.PopCount(this.data[i + 3]);
            }

            if (pop == 0)
            {
                return "None";
            }

            Span<string> names = new string[pop];
            DiscordPermissionEnumerator enumerator = new(this.data);

            for (int i = 0; i < pop; i++)
            {
                _ = enumerator.MoveNext();
                int flag = (int)enumerator.Current;
                names[i] = flag <= highestDefinedValue ? permissionNames[flag] : flag.ToString(CultureInfo.InvariantCulture);
            }

            return string.Join(", ", names);
        }
        else if (format.StartsWith("name:"))
        {
            string trimmedFormat = format[5..];

            if (string.IsNullOrWhiteSpace(trimmedFormat) || !trimmedFormat.Contains("{permission}"))
            {
                ThrowFormatException(format);
            }

            StringBuilder builder = new();

            foreach (DiscordPermission permission in this)
            {
                int flag = (int)permission;
                string permissionName = flag <= highestDefinedValue ? permissionNames[flag] : flag.ToString(CultureInfo.InvariantCulture);

                _ = builder.Append(trimmedFormat.Replace("{permission}", permissionName));
            }

            return builder.ToString();
        }
        else
        {
            Span<byte> buffer = stackalloc byte[ContainerElementCount * 4];
            this.AsSpan.CopyTo(buffer);

            if (!BitConverter.IsLittleEndian)
            {
                Span<uint> bigEndianWorkaround = MemoryMarshal.Cast<byte, uint>(buffer);
                BinaryPrimitives.ReverseEndianness(bigEndianWorkaround, bigEndianWorkaround);
            }

            return new BigInteger(buffer, true, false).ToString(CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// Calculates a hash code for this Discord permission set. The hash code is only guaranteed to be consistent
    /// within a process, and sharing this data across process boundaries is dangerous.
    /// </summary>
    public override readonly int GetHashCode()
        => HashCode.Combine(this.data);

    public static bool operator ==(DiscordPermissions left, DiscordPermissions right) => left.Equals(right);
    public static bool operator !=(DiscordPermissions left, DiscordPermissions right) => !(left == right);

    private static string[] CreatePermissionNameArray()
    {
        int highest = (int)DiscordPermissionExtensions.GetValues()[^1];
        string[] names = new string[highest + 1];

        for (int i = 0; i <= highest; i++)
        {
            names[i] = ((DiscordPermission)i).ToStringFast(true);
        }

        return names;
    }

    [DoesNotReturn]
    [DebuggerHidden]
    [StackTraceHidden]
    private static void ThrowFormatException(string format)
        => throw new FormatException($"The format string \"{format}\" was empty or malformed: it must contain an instruction to print a permission.");

    // we will be using an inline array from the start here so that further increases in the bit width
    // only require increasing this number instead of switching to a new backing implementation strategy.
    // if Discord changes the way permissions are represented in the API, this will obviously have to change.
    //
    // this should always be backed by a 32-bit integer, to make our life easier around popcnt and BitHelper.
    //
    /// <summary>
    /// Represents a container for the backing storage of Discord permissions.
    /// </summary>
    [InlineArray(ContainerElementCount)]
    internal struct DiscordPermissionContainer
    {
        public uint value;

        /// <summary>
        /// Sets a specified flag to the specific value. This function fails in debug mode if the flag was out of range.
        /// </summary>
        public void SetFlag(int index, bool value)
        {
            int fieldIndex = index >> 5;

            Debug.Assert(fieldIndex < ContainerElementCount);

            int bitIndex = index & 0x1F;
            ref uint segment = ref this[fieldIndex];
            BitHelper.SetFlag(ref segment, bitIndex, value);
        }

        /// <summary>
        /// Returns the value of a specified flag. This function fails in debug mode if the flag was out of range.
        /// </summary>
        public readonly bool HasFlag(int index)
        {
            int fieldIndex = index >> 5;

            Debug.Assert(fieldIndex < ContainerElementCount);

            int bitIndex = index & 0x1F;
            uint segment = this[fieldIndex];
            return BitHelper.HasFlag(segment, bitIndex);
        }
    }
}
