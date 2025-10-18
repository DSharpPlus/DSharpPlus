using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

namespace DSharpPlus.Voice.RuntimeServices.Pipes;

internal static partial class MemoryHelpers
{
    // we add 1 so as to never round down and not have enough space. sometimes we might not need that last element, but it's fine.
    public static int CalculateNeededSamplesFor48KHz(int inputSampleRate, int inputSampleCount) 
        => (int)((inputSampleRate / 48000.0 * inputSampleCount) + 1);

    // [TODO] remove once we're on .NET 10 and replace with Vector128.ShuffleNative/reinspect codegen for Shuffle. it's known to be problematic on
    // .NET 9, but works fine on .NET 10
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<byte> Shuffle(Vector128<byte> values, Vector128<byte> indices)
    {
        if (Ssse3.IsSupported)
        {
            return Ssse3.Shuffle(values, indices);
        }
        else if (AdvSimd.Arm64.IsSupported)
        {
            return AdvSimd.Arm64.VectorTableLookup(values, indices);
        }
        else
        {
            return Vector128.Shuffle(values, indices);
        }
    }
}
