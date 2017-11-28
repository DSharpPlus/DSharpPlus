using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.CommandsNext.Converters
{
    public class BoolConverter : IArgumentConverter<bool>
    {
        public Task<Optional<bool>> ConvertAsync(string value, CommandContext ctx)
        {
            if (bool.TryParse(value, out var result))
                return Task.FromResult(Optional<bool>.FromValue(result));

            return Task.FromResult(Optional<bool>.FromNoValue());
        }
    }

    public class Int8Converter : IArgumentConverter<sbyte>
    {
        public Task<Optional<sbyte>> ConvertAsync(string value, CommandContext ctx)
        {
            if (sbyte.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
                return Task.FromResult(Optional<sbyte>.FromValue(result));

            return Task.FromResult(Optional<sbyte>.FromNoValue());
        }
    }

    public class Uint8Converter : IArgumentConverter<byte>
    {
        public Task<Optional<byte>> ConvertAsync(string value, CommandContext ctx)
        {
            if (byte.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
                return Task.FromResult(Optional<byte>.FromValue(result));

            return Task.FromResult(Optional<byte>.FromNoValue());
        }
    }

    public class Int16Converter : IArgumentConverter<short>
    {
        public Task<Optional<short>> ConvertAsync(string value, CommandContext ctx)
        {
            if (short.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
                return Task.FromResult(Optional<short>.FromValue(result));

            return Task.FromResult(Optional<short>.FromNoValue());
        }
    }

    public class Uint16Converter : IArgumentConverter<ushort>
    {
        public Task<Optional<ushort>> ConvertAsync(string value, CommandContext ctx)
        {
            if (ushort.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
                return Task.FromResult(Optional<ushort>.FromValue(result));

            return Task.FromResult(Optional<ushort>.FromNoValue());
        }
    }

    public class Int32Converter : IArgumentConverter<int>
    {
        public Task<Optional<int>> ConvertAsync(string value, CommandContext ctx)
        {
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
                return Task.FromResult(Optional<int>.FromValue(result));

            return Task.FromResult(Optional<int>.FromNoValue());
        }
    }

    public class Uint32Converter : IArgumentConverter<uint>
    {
        public Task<Optional<uint>> ConvertAsync(string value, CommandContext ctx)
        {
            if (uint.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
                return Task.FromResult(Optional<uint>.FromValue(result));

            return Task.FromResult(Optional<uint>.FromNoValue());
        }
    }

    public class Int64Converter : IArgumentConverter<long>
    {
        public Task<Optional<long>> ConvertAsync(string value, CommandContext ctx)
        {
            if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
                return Task.FromResult(Optional<long>.FromValue(result));

            return Task.FromResult(Optional<long>.FromNoValue());
        }
    }

    public class Uint64Converter : IArgumentConverter<ulong>
    {
        public Task<Optional<ulong>> ConvertAsync(string value, CommandContext ctx)
        {
            if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
                return Task.FromResult(Optional<ulong>.FromValue(result));

            return Task.FromResult(Optional<ulong>.FromNoValue());
        }
    }

    public class Float32Converter : IArgumentConverter<float>
    {
        public Task<Optional<float>> ConvertAsync(string value, CommandContext ctx)
        {
            if (float.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
                return Task.FromResult(Optional<float>.FromValue(result));

            return Task.FromResult(Optional<float>.FromNoValue());
        }
    }

    public class Float64Converter : IArgumentConverter<double>
    {
        public Task<Optional<double>> ConvertAsync(string value, CommandContext ctx)
        {
            if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
                return Task.FromResult(Optional<double>.FromValue(result));

            return Task.FromResult(Optional<double>.FromNoValue());
        }
    }

    public class Float128Converter : IArgumentConverter<decimal>
    {
        public Task<Optional<decimal>> ConvertAsync(string value, CommandContext ctx)
        {
            if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
                return Task.FromResult(Optional<decimal>.FromValue(result));

            return Task.FromResult(Optional<decimal>.FromNoValue());
        }
    }
}
