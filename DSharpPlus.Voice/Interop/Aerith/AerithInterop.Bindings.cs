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
            AerithSignaturePrivateKey* privateKey
        );

        [LibraryImport("aerith")]
        public static partial VectorWrapper* AerithGetLastEpochAuthenticator(AerithSession* session);

        [LibraryImport("aerith")]
        public static partial void AerithSetExternalSender
        (
            AerithSession* session,
            byte* externalSenderPackage,
            nuint externalSenderLength
        );

        [LibraryImport("aerith")]
        public static partial VectorWrapper* AerithProcessProposals
        (
            AerithSession* session,
            byte* proposalsData,
            nuint proposalsLength,
            byte** recognizedUserIds,
            int recognizedUserIdCount
        );

        [LibraryImport("aerith")]
        public static partial RosterWrapper* AerithProcessCommit
        (
            AerithSession* session,
            byte* commitData,
            nuint commitLength,
            int* failureCode
        );

        [LibraryImport("aerith")]
        public static partial RosterWrapper* AerithProcessWelcome
        (
            AerithSession* session,
            byte* welcomeData,
            nuint welcomeLength,
            byte** recognizedUserIds,
            int recognizedUserIdCount
        );

        [LibraryImport("aerith")]
        public static partial VectorWrapper* AerithGetMarshalledKeyPackage(AerithSession* session);

        [LibraryImport("aerith")]
        public static partial void AerithResetSession(AerithSession* session);

        [LibraryImport("aerith")]
        public static partial void* AerithDestroySession(AerithSession* session);

        // vector_wrapper.h

        [LibraryImport("aerith")]
        public static partial int AerithGetWrappedVectorSize(VectorWrapper* wrapper);

        [LibraryImport("aerith")]
        public static partial void AerithGetWrappedVectorData(VectorWrapper* wrapper, byte* buffer);

        [LibraryImport("aerith")]
        public static partial void AerithDestroyVectorWrapper(VectorWrapper* wrapper);

        // roster_wrapper.h

        [LibraryImport("aerith")]
        public static partial int AerithGetRosterCount(RosterWrapper* wrapper);

        [LibraryImport("aerith")]
        public static partial ulong AerithGetRosterKeyAtIndex(RosterWrapper* wrapper, int index);

        [LibraryImport("aerith")]
        public static partial ulong AerithGetRosterValue(RosterWrapper* wrapper, ulong key);

        [LibraryImport("aerith")]
        public static partial void AerithDestroyRoster(RosterWrapper* wrapper);
    }
}
