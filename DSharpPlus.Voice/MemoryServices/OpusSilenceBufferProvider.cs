using System.Runtime.InteropServices;

namespace DSharpPlus.Voice.MemoryServices;

internal sealed unsafe class OpusSilenceBufferProvider : AudioBufferManager
{
    private static readonly byte[] silence = [0xF8, 0xFF, 0xFE];
    private static readonly void* silencePtr;
    private static readonly GCHandle handle;

    static OpusSilenceBufferProvider()
    {
        handle = GCHandle.Alloc(silence, GCHandleType.Pinned);
        silencePtr = (void*)handle.AddrOfPinnedObject();
    }

    public override AudioBufferLease Rent(int count) 
        => new(this, silencePtr, 3);

    protected internal override void Return(void* buffer)
    {
        // we do nothing here
    }
}
