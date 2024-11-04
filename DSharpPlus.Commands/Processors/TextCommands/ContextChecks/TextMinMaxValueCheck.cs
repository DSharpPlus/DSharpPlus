using System.Threading.Tasks;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.ContextChecks.ParameterChecks;

namespace DSharpPlus.Commands.Processors.TextCommands.ContextChecks;

/// <summary>
/// Implements MinMaxValueAttribute on text commands.
/// </summary>
internal sealed class TextMinMaxValueCheck : IParameterCheck<MinMaxValueAttribute>
{
    public ValueTask<string?> ExecuteCheckAsync(MinMaxValueAttribute attribute, ParameterCheckInfo info, CommandContext context)
    {
        if (info.Value is null)
        {
            // this implies a NVT
            return ValueTask.FromResult<string?>(null);
        }

        if (attribute.MinValue is not null)
        {
            bool correctlyOrdered = info.Value switch
            {
                byte => (byte)attribute.MinValue <= (byte)info.Value,
                sbyte => (sbyte)attribute.MinValue <= (sbyte)info.Value,
                short => (short)attribute.MinValue <= (short)info.Value,
                ushort => (ushort)attribute.MinValue <= (ushort)info.Value,
                int => (int)attribute.MinValue <= (int)info.Value,
                uint => (uint)attribute.MinValue <= (uint)info.Value,
                long => (long)attribute.MinValue <= (long)info.Value,
                ulong => (ulong)attribute.MinValue <= (ulong)info.Value,
                float => (float)attribute.MinValue <= (float)info.Value,
                double => (double)attribute.MinValue <= (double)info.Value,
                _ => true,
            };

            if (!correctlyOrdered)
            {
                return ValueTask.FromResult<string?>($"The provided value (`{info.Value}`) was less than the minimum value (`{attribute.MinValue}`).");
            }
        }

        if (attribute.MaxValue is not null)
        {
            bool correctlyOrdered = info.Value switch
            {
                byte => (byte)attribute.MaxValue >= (byte)info.Value,
                sbyte => (sbyte)attribute.MaxValue >= (sbyte)info.Value,
                short => (short)attribute.MaxValue >= (short)info.Value,
                ushort => (ushort)attribute.MaxValue >= (ushort)info.Value,
                int => (int)attribute.MaxValue >= (int)info.Value,
                uint => (uint)attribute.MaxValue >= (uint)info.Value,
                long => (long)attribute.MaxValue >= (long)info.Value,
                ulong => (ulong)attribute.MaxValue >= (ulong)info.Value,
                float => (float)attribute.MaxValue >= (float)info.Value,
                double => (double)attribute.MaxValue >= (double)info.Value,
                _ => true,
            };

            if (!correctlyOrdered)
            {
                return ValueTask.FromResult<string?>($"The provided value (`{info.Value}`) was greater than the maximum value (`{attribute.MaxValue}`).");
            }
        }

        return ValueTask.FromResult<string?>(null);
    }
}
