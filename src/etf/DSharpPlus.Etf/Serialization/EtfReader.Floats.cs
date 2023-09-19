// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Buffers.Binary;
using System.Globalization;

namespace DSharpPlus.Etf.Serialization;

partial struct EtfReader
{
    /// <summary>
    /// Reads the current term as a double.
    /// </summary>
    /// <returns>True if successful, false if unsuccessful.</returns>
    public readonly bool TryReadDouble
    (
        out double value
    )
    {
        switch (this.TermType)
        {
            case TermType.Float:
                return double.TryParse(this.CurrentTermContents, CultureInfo.InvariantCulture, out value);

            case TermType.NewFloat:
                value = BinaryPrimitives.ReadDoubleBigEndian(this.CurrentTermContents);
                return true;

            default:
                value = default;
                return false;
        }
    }

    /// <summary>
    /// Reads the current term as a float.
    /// </summary>
    /// <returns>True if successful, false if unsuccessful.</returns>
    public readonly bool TryReadSingle
    (
        out float value
    )
    {
        bool success = this.TryReadDouble(out double result);

        value = (float)result;
        return success;
    }

    /// <summary>
    /// Reads the current term as a half.
    /// </summary>
    /// <returns>True if successful, false if unsuccessful.</returns>
    public readonly bool TryReadHalf
    (
        out Half value
    )
    {
        bool success = this.TryReadDouble(out double result);

        value = (Half)result;
        return success;
    }

    /// <summary>
    /// Reads the current term as a double.
    /// </summary>
    public readonly double ReadDouble()
    {
        if (this.TryReadDouble(out double value))
        {
            return value;
        }

        ThrowHelper.ThrowInvalidDecode(typeof(double));
        return default;
    }

    /// <summary>
    /// Reads the current term as a float.
    /// </summary>
    public readonly float ReadSingle()
    {
        if (this.TryReadSingle(out float value))
        {
            return value;
        }

        ThrowHelper.ThrowInvalidDecode(typeof(float));
        return default;
    }

    /// <summary>
    /// Reads the current term as a half.
    /// </summary>
    public readonly Half ReadHalf()
    {
        if (this.TryReadHalf(out Half value))
        {
            return value;
        }

        ThrowHelper.ThrowInvalidDecode(typeof(Half));
        return default;
    }
}
