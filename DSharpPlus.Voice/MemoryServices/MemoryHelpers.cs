namespace DSharpPlus.Voice.MemoryServices;

internal static partial class MemoryHelpers
{
    // we add 1 so as to never round down and not have enough space. sometimes we might not need that last element, but it's fine.
    public static int CalculateNeededSamplesFor48KHz(int inputSampleRate, int inputSampleCount) 
        => (int)((inputSampleRate / 48000.0 * inputSampleCount) + 1);
}
