
using System;

namespace DSharpPlus.VoiceNext;
/// <summary>
/// Represents a filter for PCM data. PCM data submitted through a <see cref="VoiceTransmitSink"/> will be sent through all installed instances of <see cref="IVoiceFilter"/> first.
/// </summary>
public interface IVoiceFilter
{
    /// <summary>
    /// Transforms the supplied PCM data using this filter.
    /// </summary>
    /// <param name="pcmData">PCM data to transform. The transformation happens in-place.</param>
    /// <param name="pcmFormat">Format of the supplied PCM data.</param>
    /// <param name="duration">Millisecond duration of the supplied PCM data.</param>
    void Transform(Span<short> pcmData, AudioFormat pcmFormat, int duration);
}
