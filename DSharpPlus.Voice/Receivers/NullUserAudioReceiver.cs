using System.IO;
using System.IO.Pipelines;

namespace DSharpPlus.Voice.Receivers;

/// <summary>
/// Provides an audio receiver that discards all received audio.
/// </summary>
internal sealed class NullUserAudioReceiver : IUserAudioReceiver
{
    private static readonly PipeReader reader = NullReaderSetup();

    /// <inheritdoc />
    public PipeReader Reader => reader;

    /// <inheritdoc />
    public bool IsSpeaking => false;

    /// <inheritdoc />
    public void Ingest(ushort sequence, uint timestamp, byte[] audio)
    {
        // empty - we discard the audio
    }

    /// <inheritdoc />
    public void Close()
    {
        // dont do anything
    }

    private static PipeReader NullReaderSetup()
    {
        // it'll be disposed once the scope ends, which is fine because the reader can't be read from anyway.
        using MemoryStream ms = new();
        
        PipeReader fakeReader = PipeReader.Create(ms);
        fakeReader.Complete();

        return fakeReader;
    }
}
