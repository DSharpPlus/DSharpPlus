// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma warning disable CA1031

using System;
using System.Numerics;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Buffers.Binary;

namespace DSharpPlus;

public readonly partial record struct Snowflake :
    IBinaryInteger<Snowflake>,
    IMinMaxValue<Snowflake>,
    IParsable<Snowflake>,
    ISpanFormattable,
    ISpanParsable<Snowflake>,
    IIncrementOperators<Snowflake>,
    IDecrementOperators<Snowflake>
{
    /// <inheritdoc cref="INumberBase{TSelf}.One"/>
    public static Snowflake One => 1;

    /// <inheritdoc cref="INumberBase{TSelf}.Zero"/>
    public static Snowflake Zero => 0;

    /// <inheritdoc cref="IMinMaxValue{TSelf}.MaxValue"/>
    public static Snowflake MaxValue => long.MaxValue;

    /// <inheritdoc cref="IMinMaxValue{TSelf}.MaxValue"/>
    public static Snowflake MinValue => new
    (
        DiscordEpoch,
        0,
        0,
        0
    );

    /// <inheritdoc/>
    static int INumberBase<Snowflake>.Radix => 2;

    /// <inheritdoc/>
    static Snowflake IAdditiveIdentity<Snowflake, Snowflake>.AdditiveIdentity { get; } = 0;

    /// <inheritdoc/>
    static Snowflake IMultiplicativeIdentity<Snowflake, Snowflake>.MultiplicativeIdentity { get; } = 1;

    /// <inheritdoc cref="INumberBase{TSelf}.Parse(ReadOnlySpan{char}, NumberStyles, IFormatProvider?)"/>
    public static Snowflake Parse
    (
        ReadOnlySpan<char> s,
        NumberStyles style = NumberStyles.Integer | NumberStyles.AllowLeadingWhite,
        IFormatProvider? provider = null
    )
    {
        return long.Parse
        (
            s,
            style,
            provider
        );
    }

    /// <inheritdoc cref="INumberBase{TSelf}.Parse(string, NumberStyles, IFormatProvider?)"/>
    public static Snowflake Parse
    (
        string s,
        NumberStyles style = NumberStyles.Integer | NumberStyles.AllowLeadingWhite,
        IFormatProvider? provider = null
    )
    {
        return long.Parse
        (
            s,
            style,
            provider
        );
    }

    /// <inheritdoc cref="ISpanParsable{TSelf}.Parse(ReadOnlySpan{char}, IFormatProvider?)"/>
    public static Snowflake Parse
    (
        ReadOnlySpan<char> s,
        IFormatProvider? provider = null
    )
    {
        return long.Parse
        (
            s,
            provider
        );
    }

    /// <inheritdoc cref="IParsable{TSelf}.Parse(string, IFormatProvider?)"/>
    public static Snowflake Parse
    (
        string s,
        IFormatProvider? provider = null
    )
    {
        return long.Parse
        (
            s,
            provider
        );
    }

    /// <inheritdoc cref="INumberBase{TSelf}.TryParse(ReadOnlySpan{char}, NumberStyles, IFormatProvider?, out TSelf)"/>
    public static bool TryParse
    (
        ReadOnlySpan<char> s,
        NumberStyles style,
        IFormatProvider? provider,

        [MaybeNullWhen(false)]
        out Snowflake result
    )
    {
        bool success = long.TryParse
        (
            s,
            style,
            provider,
            out long value
        );

        result = success ? value : default;

        return success;
    }

    /// <inheritdoc cref="INumberBase{TSelf}.TryParse(string, NumberStyles, IFormatProvider?, out TSelf)"/>
    public static bool TryParse
    (
        [NotNullWhen(true)]
        string? s,

        NumberStyles style,
        IFormatProvider? provider,

        [MaybeNullWhen(false)]
        out Snowflake result
    )
    {
        bool success = long.TryParse
        (
            s,
            style,
            provider,
            out long value
        );

        result = success ? value : default;

        return success;
    }

    /// <inheritdoc cref="ISpanParsable{TSelf}.TryParse(ReadOnlySpan{char}, IFormatProvider?, out TSelf)"/>
    public static bool TryParse
    (
        ReadOnlySpan<char> s,
        IFormatProvider? provider,

        [MaybeNullWhen(false)]
        out Snowflake result
    )
    {
        bool success = long.TryParse
        (
            s,
            provider,
            out long value
        );

        result = success ? value : default;

        return success;
    }

    /// <inheritdoc cref="IParsable{TSelf}.TryParse(string?, IFormatProvider?, out TSelf)"/>
    public static bool TryParse
    (
        [NotNullWhen(true)]
        string? s,

        IFormatProvider? provider,

        [MaybeNullWhen(false)]
        out Snowflake result
    )
    {
        bool success = long.TryParse
        (
            s,
            provider,
            out long value
        );

        result = success ? value : default;

        return success;
    }

    /// <inheritdoc/>
    static Snowflake INumberBase<Snowflake>.Abs(Snowflake value)
        => long.Abs(value);

    /// <inheritdoc/>
    static bool INumberBase<Snowflake>.IsCanonical(Snowflake value)
        => true;

    /// <inheritdoc/>
    static bool INumberBase<Snowflake>.IsComplexNumber(Snowflake value)
        => false;

    /// <inheritdoc/>
    static bool INumberBase<Snowflake>.IsEvenInteger(Snowflake value)
        => long.IsEvenInteger(value);

    /// <inheritdoc/>
    static bool INumberBase<Snowflake>.IsFinite(Snowflake value)
        => true;

    /// <inheritdoc/>
    static bool INumberBase<Snowflake>.IsImaginaryNumber(Snowflake value)
        => false;

    /// <inheritdoc/>
    static bool INumberBase<Snowflake>.IsInfinity(Snowflake value)
        => false;

    /// <inheritdoc/>
    static bool INumberBase<Snowflake>.IsInteger(Snowflake value)
        => true;

    /// <inheritdoc/>
    static bool INumberBase<Snowflake>.IsNaN(Snowflake value)
        => false;

    /// <inheritdoc/>
    static bool INumberBase<Snowflake>.IsNegative(Snowflake value)
        => long.IsNegative(value);

    /// <inheritdoc/>
    static bool INumberBase<Snowflake>.IsNegativeInfinity(Snowflake value)
        => false;

    /// <inheritdoc/>
    static bool INumberBase<Snowflake>.IsNormal(Snowflake value)
        => false;

    /// <inheritdoc/>
    static bool INumberBase<Snowflake>.IsOddInteger(Snowflake value)
        => long.IsOddInteger(value);

    /// <inheritdoc/>
    static bool INumberBase<Snowflake>.IsPositive(Snowflake value)
        => long.IsPositive(value);

    /// <inheritdoc/>
    static bool INumberBase<Snowflake>.IsPositiveInfinity(Snowflake value)
        => false;

    /// <inheritdoc/>
    static bool IBinaryNumber<Snowflake>.IsPow2(Snowflake value)
        => long.IsPow2(value);

    /// <inheritdoc/>
    static bool INumberBase<Snowflake>.IsRealNumber(Snowflake value)
        => true;

    /// <inheritdoc/>
    static bool INumberBase<Snowflake>.IsSubnormal(Snowflake value)
        => false;

    /// <inheritdoc/>
    static bool INumberBase<Snowflake>.IsZero(Snowflake value)
        => value == 0;

    /// <inheritdoc/>
    static Snowflake IBinaryNumber<Snowflake>.Log2(Snowflake value)
        => long.Log2(value);

    /// <inheritdoc/>
    static Snowflake INumberBase<Snowflake>.MaxMagnitude(Snowflake x, Snowflake y)
        => long.MaxMagnitude(x, y);

    /// <inheritdoc/>
    static Snowflake INumberBase<Snowflake>.MaxMagnitudeNumber(Snowflake x, Snowflake y)
        => long.MaxMagnitude(x, y);

    /// <inheritdoc/>
    static Snowflake INumberBase<Snowflake>.MinMagnitude(Snowflake x, Snowflake y)
        => long.MinMagnitude(x, y);

    /// <inheritdoc/>
    static Snowflake INumberBase<Snowflake>.MinMagnitudeNumber(Snowflake x, Snowflake y)
        => long.MinMagnitude(x, y);

    /// <inheritdoc/>
    static Snowflake INumberBase<Snowflake>.Parse
    (
        ReadOnlySpan<char> s,
        NumberStyles style,
        IFormatProvider? provider
    )
    {
        return long.Parse
        (
            s,
            style,
            provider
        );
    }

    /// <inheritdoc/>
    static Snowflake INumberBase<Snowflake>.Parse
    (
        string s,
        NumberStyles style,
        IFormatProvider? provider
    )
    {
        return long.Parse
        (
            s,
            style,
            provider
        );
    }

    /// <inheritdoc/>
    static Snowflake ISpanParsable<Snowflake>.Parse
    (
        ReadOnlySpan<char> s,
        IFormatProvider? provider
    )
    {
        return long.Parse
        (
            s,
            provider
        );
    }

    /// <inheritdoc/>
    static Snowflake IParsable<Snowflake>.Parse
    (
        string s,
        IFormatProvider? provider
    )
    {
        return long.Parse
        (
            s,
            provider
        );
    }

    /// <inheritdoc/>
    static Snowflake IBinaryInteger<Snowflake>.PopCount(Snowflake value)
        => long.PopCount(value);

    /// <inheritdoc/>
    static Snowflake IBinaryInteger<Snowflake>.TrailingZeroCount(Snowflake value)
        => long.TrailingZeroCount(value);

    /// <inheritdoc/>
    static bool INumberBase<Snowflake>.TryConvertFromChecked<TOther>
    (
        TOther value,
        out Snowflake result
    )
    {
        try
        {
            result = long.CreateChecked
            (
                value
            );

            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    /// <inheritdoc/>
    static bool INumberBase<Snowflake>.TryConvertFromSaturating<TOther>
    (
        TOther value,
        out Snowflake result
    )
    {
        try
        {
            result = long.CreateSaturating
            (
                value
            );

            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    /// <inheritdoc/>
    static bool INumberBase<Snowflake>.TryConvertFromTruncating<TOther>
    (
        TOther value,
        out Snowflake result
    )
    {
        try
        {
            result = long.CreateTruncating
            (
                value
            );

            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

#pragma warning disable CS8500 // we statically prove this is fine
    /// <inheritdoc/>
    static unsafe bool INumberBase<Snowflake>.TryConvertToChecked<TOther>
    (
        Snowflake value,

        [NotNullWhen(true)]
        out TOther result
    )
    {
        if (typeof(TOther) == typeof(byte))
        {
            byte actualResult = checked((byte)value);
            result = *(TOther*)&actualResult;
            return true;
        }
        else if (typeof(TOther) == typeof(char))
        {
            char actualResult = checked((char)value);
            result = *(TOther*)&actualResult;
            return true;
        }
        else if (typeof(TOther) == typeof(decimal))
        {
            decimal actualResult = value;
            result = *(TOther*)&actualResult;
            return true;
        }
        else if (typeof(TOther) == typeof(ushort))
        {
            ulong actualResult = checked((ushort)value);
            result = *(TOther*)&actualResult;
            return true;
        }
        else if (typeof(TOther) == typeof(uint))
        {
            ulong actualResult = checked((uint)value);
            result = *(TOther*)&actualResult;
            return true;
        }
        else if (typeof(TOther) == typeof(ulong))
        {
            ulong actualResult = checked((ulong)value.Value);
            result = *(TOther*)&actualResult;
            return true;
        }
        else if (typeof(TOther) == typeof(UInt128))
        {
            UInt128 actualResult = checked((UInt128)value.Value);
            result = *(TOther*)&actualResult;
            return true;
        }
        else if (typeof(TOther) == typeof(UIntPtr))
        {
            UIntPtr actualResult = checked((UIntPtr)value.Value);
            result = *(TOther*)&actualResult;
            return true;
        }
        else
        {
            result = default!;
            return false;
        }
    }

    /// <inheritdoc/>
    static unsafe bool INumberBase<Snowflake>.TryConvertToSaturating<TOther>
    (
        Snowflake value,

        [MaybeNullWhen(false)]
        out TOther result
    )
    {
        if (typeof(TOther) == typeof(byte))
        {
            byte actualResult = value >= byte.MaxValue
                ? byte.MaxValue
                : value <= byte.MinValue
                    ? byte.MinValue
                    : (byte)value;

            result = *(TOther*)&actualResult;
            return true;
        }
        else if (typeof(TOther) == typeof(char))
        {
            char actualResult = value >= char.MaxValue
                ? char.MaxValue
                : value <= char.MinValue
                    ? char.MinValue
                    : (char)value;

            result = *(TOther*)&actualResult;
            return true;
        }
        else if (typeof(TOther) == typeof(decimal))
        {
            decimal actualResult = value;
            result = *(TOther*)&actualResult;
            return true;
        }
        else if (typeof(TOther) == typeof(ushort))
        {
            ushort actualResult = value >= ushort.MaxValue
                ? ushort.MaxValue
                : value <= ushort.MinValue
                    ? ushort.MinValue
                    : (ushort)value;

            result = *(TOther*)&actualResult;
            return true;
        }
        else if (typeof(TOther) == typeof(uint))
        {
            uint actualResult = value >= uint.MaxValue
                ? uint.MaxValue
                : value <= uint.MinValue
                    ? uint.MinValue
                    : (uint)value;

            result = *(TOther*)&actualResult;
            return true;
        }
        else if (typeof(TOther) == typeof(ulong))
        {
            ulong actualResult = value <= 0 ? ulong.MinValue : (ulong)value.Value;
            result = *(TOther*)&actualResult;
            return true;
        }
        else if (typeof(TOther) == typeof(UInt128))
        {
            UInt128 actualResult = (value <= 0) ? UInt128.MinValue : (UInt128)value.Value;
            result = *(TOther*)&actualResult;
            return true;
        }
        else if (typeof(TOther) == typeof(UIntPtr))
        {
            UIntPtr actualResult = (value <= 0) ? 0 : (UIntPtr)value.Value;
            result = *(TOther*)&actualResult;
            return true;
        }
        else
        {
            result = default!;
            return false;
        }
    }

    /// <inheritdoc/>
    static unsafe bool INumberBase<Snowflake>.TryConvertToTruncating<TOther>
    (
        Snowflake value,

        [MaybeNullWhen(false)]
        out TOther result
    )
    {
        if (typeof(TOther) == typeof(byte))
        {
            byte actualResult = (byte)value;
            result = *(TOther*)&actualResult;
            return true;
        }
        else if (typeof(TOther) == typeof(char))
        {
            char actualResult = (char)value;
            result = *(TOther*)&actualResult;
            return true;
        }
        else if (typeof(TOther) == typeof(decimal))
        {
            decimal actualResult = value;
            result = *(TOther*)&actualResult;
            return true;
        }
        else if (typeof(TOther) == typeof(ushort))
        {
            ushort actualResult = (ushort)value;
            result = *(TOther*)&actualResult;
            return true;
        }
        else if (typeof(TOther) == typeof(uint))
        {
            uint actualResult = (uint)value;
            result = *(TOther*)&actualResult;
            return true;
        }
        else if (typeof(TOther) == typeof(ulong))
        {
            ulong actualResult = (ulong)value.Value;
            result = *(TOther*)&actualResult;
            return true;
        }
        else if (typeof(TOther) == typeof(UInt128))
        {
            UInt128 actualResult = (UInt128)value.Value;
            result = *(TOther*)&actualResult;
            return true;
        }
        else if (typeof(TOther) == typeof(UIntPtr))
        {
            UIntPtr actualResult = (UIntPtr)value.Value;
            result = *(TOther*)&actualResult;
            return true;
        }
        else
        {
            result = default;
            return false;
        }
    }
#pragma warning restore CS8500

    /// <inheritdoc/>
    static bool INumberBase<Snowflake>.TryParse
    (
        ReadOnlySpan<char> s,
        NumberStyles style,
        IFormatProvider? provider,
        out Snowflake result
    )
    {
        bool success = long.TryParse
        (
            s,
            style,
            provider,
            out long value
        );

        result = success ? value : default;

        return success;
    }

    /// <inheritdoc/>
    static bool INumberBase<Snowflake>.TryParse
    (
        [NotNullWhen(true)]
        string? s,

        NumberStyles style,
        IFormatProvider? provider,
        out Snowflake result
    )
    {
        bool success = long.TryParse
        (
            s,
            style,
            provider,
            out long value
        );

        result = success ? value : default;

        return success;
    }

    /// <inheritdoc/>
    static bool ISpanParsable<Snowflake>.TryParse
    (
        ReadOnlySpan<char> s,
        IFormatProvider? provider,
        out Snowflake result
    )
    {
        bool success = long.TryParse
        (
            s,
            provider,
            out long value
        );

        result = success ? value : default;

        return success;
    }

    /// <inheritdoc/>
    static bool IParsable<Snowflake>.TryParse
    (
        string? s,
        IFormatProvider? provider,
        out Snowflake result
    )
    {
        bool success = long.TryParse
        (
            s,
            provider,
            out long value
        );

        result = success ? value : default;

        return success;
    }

    /// <inheritdoc/>
    static bool IBinaryInteger<Snowflake>.TryReadBigEndian
    (
        ReadOnlySpan<byte> source,
        bool isUnsigned,
        out Snowflake value
    )
    {
        if (source.Length < 8)
        {
            value = default;
            return false;
        }

        long result = Unsafe.ReadUnaligned<long>
        (
            ref MemoryMarshal.GetReference(source)
        );

        value = BitConverter.IsLittleEndian
            ? BinaryPrimitives.ReverseEndianness(result)
            : result;

        return true;
    }

    /// <inheritdoc/>
    static bool IBinaryInteger<Snowflake>.TryReadLittleEndian
    (
        ReadOnlySpan<byte> source,
        bool isUnsigned,
        out Snowflake value
    )
    {
        if (source.Length < 8)
        {
            value = default;
            return false;
        }

        long result = Unsafe.ReadUnaligned<long>
        (
            ref MemoryMarshal.GetReference(source)
        );

        value = BitConverter.IsLittleEndian
            ? result
            : BinaryPrimitives.ReverseEndianness(result);

        return true;
    }

    /// <inheritdoc cref="IComparable.CompareTo(object?)"/>
    public int CompareTo(object? obj)
        => this.Value.CompareTo(obj);

    /// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
    public string ToString(string? format, IFormatProvider? formatProvider = null)
        => this.Value.ToString(format, formatProvider);

    /// <inheritdoc cref="ISpanFormattable.TryFormat(Span{char}, out int, ReadOnlySpan{char}, IFormatProvider?)"/>
    public bool TryFormat
    (
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? provider = null
    )
    {
        return this.Value.TryFormat
        (
            destination,
            out charsWritten,
            format,
            provider
        );
    }

    /// <inheritdoc/>
    int IComparable.CompareTo(object? obj)
        => this.Value.CompareTo(obj);

    /// <inheritdoc/>
    int IComparable<Snowflake>.CompareTo(Snowflake other)
        => this.Value.CompareTo(other);

    /// <inheritdoc/>
    bool IEquatable<Snowflake>.Equals(Snowflake other)
        => this.Value.Equals(other);

    /// <inheritdoc/>
    int IBinaryInteger<Snowflake>.GetByteCount()
        => 8;

    /// <inheritdoc/>
    int IBinaryInteger<Snowflake>.GetShortestBitLength()
        => 64 - BitOperations.LeadingZeroCount((ulong)this.Value);

    /// <inheritdoc/>
    string IFormattable.ToString
    (
        string? format,
        IFormatProvider? formatProvider
    )
    {
        return this.ToString
        (
            format,
            formatProvider
        );
    }

    /// <inheritdoc/>
    bool ISpanFormattable.TryFormat
    (
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? provider
    )
    {
        return this.TryFormat
        (
            destination,
            out charsWritten,
            format,
            provider
        );
    }

    /// <inheritdoc/>
    bool IBinaryInteger<Snowflake>.TryWriteBigEndian
    (
        Span<byte> destination,
        out int bytesWritten
    )
    {
        if (destination.Length < 8)
        {
            bytesWritten = 0;
            return false;
        }

        long value = BitConverter.IsLittleEndian
            ? BinaryPrimitives.ReverseEndianness(this.Value)
            : this.Value;

        Unsafe.WriteUnaligned
        (
            ref MemoryMarshal.GetReference(destination),
            value
        );

        bytesWritten = 8;
        return true;
    }

    /// <inheritdoc/>
    bool IBinaryInteger<Snowflake>.TryWriteLittleEndian
    (
        Span<byte> destination,
        out int bytesWritten
    )
    {
        if (destination.Length < 8)
        {
            bytesWritten = 0;
            return false;
        }

        long value = !BitConverter.IsLittleEndian
            ? BinaryPrimitives.ReverseEndianness(this.Value)
            : this.Value;

        Unsafe.WriteUnaligned
        (
            ref MemoryMarshal.GetReference(destination),
            value
        );

        bytesWritten = 8;
        return true;
    }

    /// <inheritdoc/>
    static Snowflake IUnaryPlusOperators<Snowflake, Snowflake>.operator +(Snowflake value)
        => +value.Value;

    /// <inheritdoc/>
    public static Snowflake operator +(Snowflake left, Snowflake right)
        => left.Value + right.Value;

    /// <inheritdoc/>
    static Snowflake IAdditionOperators<Snowflake, Snowflake, Snowflake>.operator +(Snowflake left, Snowflake right)
        => left.Value + right.Value;

    static Snowflake IUnaryNegationOperators<Snowflake, Snowflake>.operator -(Snowflake value)
        => -value.Value;

    /// <inheritdoc/>
    public static Snowflake operator -(Snowflake left,Snowflake right)
        => left.Value - right.Value;

    /// <inheritdoc/>
    static Snowflake ISubtractionOperators<Snowflake, Snowflake, Snowflake>.operator -(Snowflake left, Snowflake right)
        => left.Value - right.Value;

    /// <inheritdoc/>
    static Snowflake IBitwiseOperators<Snowflake, Snowflake, Snowflake>.operator ~(Snowflake value)
        => ~value.Value;

    /// <inheritdoc/>
    public static Snowflake operator ++(Snowflake value)
        => value.Value + 1;

    /// <inheritdoc/>
    static Snowflake IIncrementOperators<Snowflake>.operator ++(Snowflake value)
        => value.Value + 1;

    /// <inheritdoc/>
    public static Snowflake operator --(Snowflake value)
        => value.Value - 1;

    /// <inheritdoc/>
    static Snowflake IDecrementOperators<Snowflake>.operator --(Snowflake value)
        => value.Value - 1;

    /// <inheritdoc/>
    static Snowflake IMultiplyOperators<Snowflake, Snowflake, Snowflake>.operator *(Snowflake left, Snowflake right)
        => left.Value * right.Value;

    /// <inheritdoc/>
    static Snowflake IDivisionOperators<Snowflake, Snowflake, Snowflake>.operator /(Snowflake left, Snowflake right)
        => left.Value / right.Value;

    /// <inheritdoc/>
    static Snowflake IModulusOperators<Snowflake, Snowflake, Snowflake>.operator %(Snowflake left, Snowflake right)
        => left.Value % right.Value;

    /// <inheritdoc/>
    static Snowflake IBitwiseOperators<Snowflake, Snowflake, Snowflake>.operator &(Snowflake left, Snowflake right)
        => left.Value & right.Value;

    /// <inheritdoc/>
    static Snowflake IBitwiseOperators<Snowflake, Snowflake, Snowflake>.operator |(Snowflake left, Snowflake right)
        => left.Value | right.Value;

    /// <inheritdoc/>
    static Snowflake IBitwiseOperators<Snowflake, Snowflake, Snowflake>.operator ^(Snowflake left, Snowflake right)
        => left.Value ^ right.Value;

    /// <inheritdoc/>
    static Snowflake IShiftOperators<Snowflake, int, Snowflake>.operator <<(Snowflake value, int shiftAmount)
        => value.Value << shiftAmount;

    /// <inheritdoc/>
    static Snowflake IShiftOperators<Snowflake, int, Snowflake>.operator >>(Snowflake value, int shiftAmount)
        => value.Value >> shiftAmount;

    /// <inheritdoc/>
    static bool IEqualityOperators<Snowflake, Snowflake, bool>.operator ==(Snowflake left, Snowflake right)
        => left.Value == right.Value;

    /// <inheritdoc/>
    static bool IEqualityOperators<Snowflake, Snowflake, bool>.operator !=(Snowflake left, Snowflake right)
        => left.Value != right.Value;

    /// <inheritdoc/>
    static bool IComparisonOperators<Snowflake, Snowflake, bool>.operator <(Snowflake left, Snowflake right)
        => left.Value < right.Value;

    /// <inheritdoc/>
    static bool IComparisonOperators<Snowflake, Snowflake, bool>.operator >(Snowflake left, Snowflake right)
        => left.Value > right.Value;

    /// <inheritdoc/>
    static bool IComparisonOperators<Snowflake, Snowflake, bool>.operator <=(Snowflake left, Snowflake right)
        => left.Value <= right.Value;

    /// <inheritdoc/>
    static bool IComparisonOperators<Snowflake, Snowflake, bool>.operator >=(Snowflake left, Snowflake right)
        => left.Value >= right.Value;

    /// <inheritdoc/>
    static Snowflake IShiftOperators<Snowflake, int, Snowflake>.operator >>>(Snowflake value, int shiftAmount)
        => value.Value >>> shiftAmount;
}
