namespace DSharpPlus
{
    internal enum OpusError : int
    {
        OPUS_OK = 0,
        OPUS_BAD_ARG,
        OPUS_BUFFER_TO_SMALL,
        OPUS_INTERNAL_ERROR,
        OPUS_INVALID_PACKET,
        OPUS_UNIMPLEMENTED,
        OPUS_INVALID_STATE,
        OPUS_ALLOC_FAIL
    }
}
