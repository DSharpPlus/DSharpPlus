using Remora.Results;

namespace DSharpPlus.UnifiedCommands.Message.Errors;

public record class FailedConversionError : ResultError
{
    public string Name { get; init; }
    public string Value { get; init; }
    public IResultError Inner { get; init; }

    public FailedConversionError(string message, string name, string value, IResultError inner) : base(message)
    {
        Name = name;
        Value = value;
        Inner = inner;
    }
}