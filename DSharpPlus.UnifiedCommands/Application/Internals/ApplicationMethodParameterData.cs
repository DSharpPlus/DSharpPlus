namespace DSharpPlus.UnifiedCommands.Application.Internals;

internal class ApplicationMethodParameterData
{
    public ApplicationCommandOptionType Type { get; set; }
    public string Name { get; }
    public bool IsNullable { get; set; } = false;

    public ApplicationMethodParameterData(string name)
        => Name = name;
}
