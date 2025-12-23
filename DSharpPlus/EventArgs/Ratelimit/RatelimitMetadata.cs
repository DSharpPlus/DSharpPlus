namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents metadata sent with <see cref="RatelimitedEventArgs"/>. The exact type depends on <see cref="RatelimitedEventArgs.Opcode"/>.
/// </summary>
public abstract class RatelimitMetadata;
