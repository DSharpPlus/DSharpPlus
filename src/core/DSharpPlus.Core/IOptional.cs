namespace DSharpPlus.Core;

/// <summary>
/// Serialization utility interface for optional values.
/// </summary>
public interface IOptional
{
    public bool HasValue { get; set; }
}
