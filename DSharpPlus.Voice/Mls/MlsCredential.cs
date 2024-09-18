using System;

using DSharpPlus.Voice.Interop.Aerith;

namespace DSharpPlus.Voice.Mls;

/// <summary>
/// Manages a MLS credential for a user.
/// </summary>
internal unsafe struct MlsCredential : IDisposable
{
    private void* nativeCredential;

    public bool IsInvalid { get; private set; }

    public static MlsCredential Create(ulong userId)
    {

    }

    public ulong GetTargetUserId()
        => AerithInterop.DeserializeUserCredential(this.nativeCredential);

    public void Dispose()
    {
        this.IsInvalid = true;
        AerithInterop.FreeUserCredential(this);
    }
}
