using System.Runtime.InteropServices;

namespace DSharpPlus.Voice.Interop.Aerith;

#pragma warning disable IDE0040
partial class AerithInterop
#pragma warning restore IDE0040
{
    private static unsafe partial class Bindings
    {
        [LibraryImport("aerith")]
        public static partial AerithSession* AerithCreateSession
        (
            byte* authSessionId,
            nuint authSessionLength,
            delegate* unmanaged<byte*, byte*> errorHandler
        );

        [LibraryImport("aerith")]
        public static partial AerithSignaturePrivateKey* AerithGetSignaturePrivateKey
        (
            byte* sessionId,
            nuint sessionLength
        );

        [LibraryImport("aerith")]
        public static partial void AerithInitSession
        (
            AerithSession* session,
            ulong groupId,
            byte* currentUserId,
            nuint currentUserLength,
            AerithSignaturePrivateKey* privateKey
        );

        [LibraryImport("aerith")]
        public static partial int AerithGetLastEpochAuthenticatorSize(AerithSession* session);

        [LibraryImport("aerith")]
        public static partial void AerithGetLastEpochAuthenticator
        (
            AerithSession* session,
            byte* buffer
        );

        [LibraryImport("aerith")]
        public static partial void AerithSetExternalSender
        (
            AerithSession* session,
            byte* externalSenderPackage,
            nuint externalSenderLength
        );

        [LibraryImport("aerith")]
        public static partial void AerithResetSession(AerithSession* session);

        [LibraryImport("aerith")]
        public static partial void* AerithDestroySession(AerithSession* session);
    }
}
