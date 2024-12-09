namespace DSharpPlus.Commands.Processors.TextCommands;

/// <summary>
/// The requirements for a converter to require a text argument.
/// </summary>
public enum ConverterInputType
{
    /// <summary>
    /// The converter does not require a text argument.
    /// </summary>
    Never = 0,

    /// <summary>
    /// The converter will always require a text argument.
    /// </summary>
    Always,

    /// <summary>
    /// The converter will require a text argument when a reply is missing.
    /// </summary>
    IfReplyMissing,

    /// <summary>
    /// The converter will require a text argument when a reply is present.
    /// </summary>
    IfReplyPresent
}
