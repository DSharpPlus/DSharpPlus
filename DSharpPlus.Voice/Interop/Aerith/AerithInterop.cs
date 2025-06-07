using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace DSharpPlus.Voice.Interop.Aerith;

internal static unsafe partial class AerithInterop
{
    [UnmanagedCallersOnly]
    public static void HandleNativeMlsError(byte* site, byte* reason)
    {

    }

    /// <summary>
    /// Creates a basic user credential from the user's snowflake ID.
    /// </summary>
    public static void* CreateUserCredential(ulong userId)
    {
        Span<byte> bigEndian = stackalloc byte[8];
        BinaryPrimitives.WriteUInt64BigEndian(bigEndian, userId);

        fixed (byte* pId = bigEndian)
        {
            return Bindings.aerith_create_user_credential(pId);
        }
    }

    /// <summary>
    /// Extracts the user ID from a basic credential.
    /// </summary>
    public static ulong DeserializeUserCredential(void* credential)
    {
        Span<byte> buffer = stackalloc byte[8];
        
        fixed (byte* pBuffer = buffer)
        {
            Bindings.aerith_deserialize_credential(credential, pBuffer);
        }

        return BinaryPrimitives.ReadUInt64BigEndian(buffer);
    }

    /// <summary>
    /// Destroys a credential. Using the credential in any way is illegal after this operation.
    /// </summary>
    public static void FreeUserCredential(void* credential) 
        => Bindings.aerith_free_credential(credential);
}
