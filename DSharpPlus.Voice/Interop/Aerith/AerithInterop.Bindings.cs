using System.Runtime.InteropServices;

namespace DSharpPlus.Voice.Interop.Aerith;

#pragma warning disable IDE0040
partial class AerithInterop
#pragma warning restore IDE0040
{
    private static unsafe partial class Bindings
    {
        /// <summary>
        /// <code>
        /// <![CDATA[discord::dave::mls::Session* AerithCreateSession
        /// (
        ///     const char* authSessionId,
        ///     size_t authSessionLength,
        ///     void (*errorHandler)(const char*, const char*)
        /// );]]>
        /// </code>
        /// </summary>
        [LibraryImport("aerith")]
        public static partial AerithSession* AerithCreateSession
        (
            byte* authSessionId,
            nuint authSessionLength,
            delegate* unmanaged<byte*, byte*, void> errorHandler
        );

        /// <summary>
        /// <code>
        /// <![CDATA[std::shared_ptr<mlspp::SignaturePrivateKey>* AerithGetSignaturePrivateKey
        /// (
        ///     const char* sessionId,
        ///     size_t sessionLength
        /// );]]>
        /// </code>
        /// </summary>
        [LibraryImport("aerith")]
        public static partial AerithSignaturePrivateKey* AerithGetSignaturePrivateKey
        (
            byte* sessionId,
            nuint sessionLength
        );

        /// <summary>
        /// <code>
        /// <![CDATA[void AerithInitSession
        /// (
        ///     discord::dave::mls::Session* session,
        ///     uint64_t groupId,
        ///     uint64_t currentUserId,
        ///     std::shared_ptr<mlspp::SignaturePrivateKey>* privateTransientKey
        /// );]]>
        /// </code>
        /// </summary>
        [LibraryImport("aerith")]
        public static partial void AerithInitSession
        (
            AerithSession* session,
            ulong groupId,
            ulong currentUserId,
            AerithSignaturePrivateKey* privateKey
        );

        /// <summary>
        /// <code>
        /// <![CDATA[VectorWrapper* AerithGetLastEpochAuthenticator(discord::dave::mls::Session* session);]]>
        /// </code>
        /// </summary>
        [LibraryImport("aerith")]
        public static partial VectorWrapper* AerithGetLastEpochAuthenticator(AerithSession* session);

        /// <summary>
        /// <code>
        /// <![CDATA[void AerithSetExternalSender
        /// (
        ///     discord::dave::mls::Session* session,
        ///     const uint8_t* externalSenderPackage,
        ///     size_t externalSenderLength
        /// );]]>
        /// </code>
        /// </summary>
        [LibraryImport("aerith")]
        public static partial void AerithSetExternalSender
        (
            AerithSession* session,
            byte* externalSenderPackage,
            nuint externalSenderLength
        );

        /// <summary>
        /// <code>
        /// <![CDATA[VectorWrapper* AerithProcessProposals
        /// (
        ///     discord::dave::mls::Session* session,
        ///     const uint8_t* proposalsData,
        ///     size_t proposalsLength,
        ///     uint64_t* recognizedUserIds,
        ///     int32_t recognizedUserCount
        /// );]]>
        /// </code>
        /// </summary>
        [LibraryImport("aerith")]
        public static partial VectorWrapper* AerithProcessProposals
        (
            AerithSession* session,
            byte* proposalsData,
            nuint proposalsLength,
            ulong* recognizedUserIds,
            int recognizedUserIdCount
        );

        /// <summary>
        /// <code>
        /// <![CDATA[RosterWrapper* AerithProcessCommit
        /// (
        ///     discord::dave::mls::Session* session,
        ///     const uint8_t* commitData,
        ///     size_t commitLength,
        ///     int32_t* failureCode
        /// );]]>
        /// </code>
        /// </summary>
        [LibraryImport("aerith")]
        public static partial RosterWrapper* AerithProcessCommit
        (
            AerithSession* session,
            byte* commitData,
            nuint commitLength,
            int* failureCode
        );

        /// <summary>
        /// <code>
        /// <![CDATA[RosterWrapper* AerithProcessWelcome
        /// (
        ///     discord::dave::mls::Session* session,
        ///     const uint8_t* welcomeData,
        ///     size_t welcomeLength,
        ///     uint64_t* recognizedUserIds,
        ///     int32_t recognizedUserCount
        /// );]]>
        /// </code>
        /// </summary>
        [LibraryImport("aerith")]
        public static partial RosterWrapper* AerithProcessWelcome
        (
            AerithSession* session,
            byte* welcomeData,
            nuint welcomeLength,
            ulong* recognizedUserIds,
            int recognizedUserIdCount
        );

        /// <summary>
        /// <code>
        /// <![CDATA[VectorWrapper* AerithGetMarshalledKeyPackage(discord::dave::mls::Session* session);]]>
        /// </code>
        /// </summary>
        [LibraryImport("aerith")]
        public static partial VectorWrapper* AerithGetMarshalledKeyPackage(AerithSession* session);

        /// <summary>
        /// <code>
        /// <![CDATA[void AerithResetSession(discord::dave::mls::Session* session);]]>
        /// </code>
        /// </summary>
        [LibraryImport("aerith")]
        public static partial void AerithResetSession(AerithSession* session);

        /// <summary>
        /// <code>
        /// <![CDATA[void AerithDestroySession(discord::dave::mls::Session* session);]]>
        /// </code>
        /// </summary>
        [LibraryImport("aerith")]
        public static partial void AerithDestroySession(AerithSession* session);

        // vector_wrapper.h

        /// <summary>
        /// <code>
        /// <![CDATA[void AerithDestroyVectorWrapper(VectorWrapper* wrapper);]]>
        /// </code>
        /// </summary>
        [LibraryImport("aerith")]
        public static partial void AerithDestroyVectorWrapper(VectorWrapper* wrapper);

        // roster_wrapper.h

        /// <summary>
        /// <code>
        /// <![CDATA[void AerithDestroyRoster(RosterWrapper* wrapper);]]>
        /// </code>
        /// </summary>
        [LibraryImport("aerith")]
        public static partial void AerithDestroyRoster(RosterWrapper* wrapper);
    }
}
