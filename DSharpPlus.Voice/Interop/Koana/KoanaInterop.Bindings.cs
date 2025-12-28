using System.Runtime.InteropServices;

namespace DSharpPlus.Voice.Interop.Koana;

#pragma warning disable IDE0040

unsafe partial struct KoanaInterop
{
    /// <summary>
    /// <code>
    /// <![CDATA[void koana_set_mls_error_callback(void (*error_handler)(const char*, const char*));]]>
    /// </code>
    /// </summary>
    [LibraryImport("koana")]
    private static partial void koana_set_mls_error_callback(delegate* unmanaged<byte*, byte*, void> errorHandler);

    /// <summary>
    /// <code>
    /// <![CDATA[koana_context* koana_create_context(void (*log)(int32_t, const char*));]]>
    /// </code>
    /// </summary>
    [LibraryImport("koana")]
    private static partial KoanaContext* koana_create_context(delegate* unmanaged<KoanaLogLevel, byte*, void> log);

    /// <summary>
    /// <code>
    /// <![CDATA[void koana_reinit_context
    /// (
    ///     koana_context* context,
    ///     uint16_t protocol_version,
    ///     uint64_t channel_id,
    ///     uint64_t bot_uesr_id
    /// );]]>
    /// </code>
    /// </summary>
    [LibraryImport("koana")]
    private static partial void koana_reinit_context(KoanaContext* context, ushort protocolVersion, ulong channelId, ulong botId);

    /// <summary>
    /// <code>
    /// <![CDATA[void koana_reset_context(koana_context* context);]]>
    /// </code>
    /// </summary>
    [LibraryImport("koana")]
    private static partial void koana_reset_context(KoanaContext* context);

    /// <summary>
    /// <code>
    /// <![CDATA[void koana_destroy_context(koana_context* context);]]>
    /// </code>
    /// </summary>
    [LibraryImport("koana")]
    private static partial void koana_destroy_context(KoanaContext* context);

    /// <summary>
    /// <code>
    /// <![CDATA[void koana_set_external_sender(koana_context* context, const uint8_t* data, int32_t length);]]>
    /// </code>
    /// </summary>
    [LibraryImport("koana")]
    private static partial void koana_set_external_sender(KoanaContext* context, byte* data, int length);

    /// <summary>
    /// <code>
    /// <![CDATA[native_vector* koana_process_proposals
    /// (
    ///     koana_context* context,
    ///     const uint8_t* proposals,
    ///     int32_t proposals_length,
    ///     const uint64_t* known_users,
    ///     int32_t known_users_length
    /// );]]>
    /// </code>
    /// </summary>
    [LibraryImport("koana")]
    private static partial NativeVector* koana_process_proposals
    (
        KoanaContext* context,
        byte* proposals,
        int proposalsLength,
        ulong* knownUsers,
        int knownUsersLength
    );

    /// <summary>
    /// <code>
    /// <![CDATA[koana_error koana_process_commit(koana_context* context, const uint8_t* commit, int32_t commit_length);]]>
    /// </code>
    /// </summary>
    [LibraryImport("koana")]
    private static partial KoanaError koana_process_commit(KoanaContext* context, byte* commit, int commitLength);

    /// <summary>
    /// <code>
    /// <![CDATA[void koana_process_welcome
    /// (
    ///     koana_context* context,
    ///     const uint8_t* welcome,
    ///     int32_t welcome_length,
    ///     const uint64_t* known_users,
    ///     int32_t known_users_length
    /// );]]>
    /// </code>
    /// </summary>
    [LibraryImport("koana")]
    private static partial void koana_process_welcome
    (
        KoanaContext* context,
        byte* welcome,
        int welcomeLength,
        ulong* knownUsers,
        int knownUsersLength
    );

    /// <summary>
    /// <code>
    /// <![CDATA[native_roster* koana_get_cached_roster(koana_context* context);]]>
    /// </code>
    /// </summary>
    [LibraryImport("koana")]
    private static partial NativeRoster* koana_get_cached_roster(KoanaContext* context);

    /// <summary>
    /// <code>
    /// <![CDATA[uint16_t koana_get_current_protocol_version(koana_context* context);]]>
    /// </code>
    /// </summary>
    [LibraryImport("koana")]
    private static partial ushort koana_get_current_protocol_version(KoanaContext* context);

    /// <summary>
    /// <code>
    /// <![CDATA[native_vector* koana_get_marshalled_key_package(koana_context* context);]]>
    /// </code>
    /// </summary>
    [LibraryImport("koana")]
    private static partial NativeVector* koana_get_marshalled_key_package(KoanaContext* context);

    /// <summary>
    /// <code>
    /// <![CDATA[int32_t koana_decrypt_frame
    /// (
    ///     koana_context* context,
    ///     uint64_t user_id,
    ///     const uint8_t* encrypted_frame,
    ///     int32_t encrypted_length,
    ///     uint8_t* decrypted_frame,
    ///     int32_t decrypted_length
    /// );]]>
    /// </code>
    /// </summary>
    [LibraryImport("koana")]
    private static partial int koana_decrypt_frame
    (
        KoanaContext* context,
        ulong userId,
        byte* encryptedFrame,
        int encryptedLength,
        byte* decryptedFrame,
        int decryptedBufferLength
    );

    /// <summary>
    /// <code>
    /// <![CDATA[koana_error koana_encrypt_frame
    /// (
    ///     koana_context* context,
    ///     uint32_t ssrc,
    ///     const uint8_t* unencrypted_frame,
    ///     int32_t unencrypted_length,
    ///     uint8_t* encrypted_frame,
    ///     int32_t encrypted_length,
    ///     int32_t* encrypted_size
    /// );]]>
    /// </code>
    /// </summary>
    [LibraryImport("koana")]
    private static partial KoanaError koana_encrypt_frame
    (
        KoanaContext* context,
        uint ssrc,
        byte* unencryptedFrame,
        int unencryptedFrameLength,
        byte* encryptedFrame,
        int encryptedBufferLength,
        int* encryptedLength
    );

    /// <summary>
    /// <code>
    /// <![CDATA[void koana_destroy_roster(native_roster* roster);]]>
    /// </code>
    /// </summary>
    [LibraryImport("koana")]
    private static partial void koana_destroy_roster(NativeRoster* roster);

    /// <summary>
    /// <code>
    /// <![CDATA[void koana_destroy_vector(native_vector* vector);]]>
    /// </code>
    /// </summary>
    [LibraryImport("koana")]
    private static partial void koana_destroy_vector(NativeVector* vector);
}
