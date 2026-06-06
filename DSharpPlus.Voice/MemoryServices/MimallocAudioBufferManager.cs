using TerraFX.Interop.Mimalloc;

namespace DSharpPlus.Voice.MemoryServices;

internal sealed unsafe class MimallocAudioBufferManager : AudioBufferManager
{
    public override AudioBufferLease Rent(int count)
    {
        void* ptr = Mimalloc.mi_malloc((nuint)count);
        return new(this, ptr, count);
    }

    protected internal override void Return(void* buffer)
        => Mimalloc.mi_free(buffer);
}
