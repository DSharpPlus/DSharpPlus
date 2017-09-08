using System.Globalization;

namespace DSharpPlus.CommandsNext.Converters
{
    public class BoolConverter : IArgumentConverter<bool>
    {
        public bool TryConvert(string value, CommandContext ctx, out bool result) =>
            bool.TryParse(value, out result);
    }

    public class Int8Converter : IArgumentConverter<sbyte>
    {
        public bool TryConvert(string value, CommandContext ctx, out sbyte result) =>
            sbyte.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
    }

    public class Uint8Converter : IArgumentConverter<byte>
    {
        public bool TryConvert(string value, CommandContext ctx, out byte result) =>
            byte.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
    }

    public class Int16Converter : IArgumentConverter<short>
    {
        public bool TryConvert(string value, CommandContext ctx, out short result) =>
            short.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
    }

    public class Uint16Converter : IArgumentConverter<ushort>
    {
        public bool TryConvert(string value, CommandContext ctx, out ushort result) =>
            ushort.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
    }

    public class Int32Converter : IArgumentConverter<int>
    {
        public bool TryConvert(string value, CommandContext ctx, out int result) =>
            int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
    }

    public class Uint32Converter : IArgumentConverter<uint>
    {
        public bool TryConvert(string value, CommandContext ctx, out uint result) =>
            uint.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
    }

    public class Int64Converter : IArgumentConverter<long>
    {
        public bool TryConvert(string value, CommandContext ctx, out long result) =>
            long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
    }

    public class Uint64Converter : IArgumentConverter<ulong>
    {
        public bool TryConvert(string value, CommandContext ctx, out ulong result) =>
            ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
    }

    public class Float32Converter : IArgumentConverter<float>
    {
        public bool TryConvert(string value, CommandContext ctx, out float result) =>
            float.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
    }

    public class Float64Converter : IArgumentConverter<double>
    {
        public bool TryConvert(string value, CommandContext ctx, out double result) =>
            double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
    }

    public class Float128Converter : IArgumentConverter<decimal>
    {
        public bool TryConvert(string value, CommandContext ctx, out decimal result) =>
            decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
    }
}
