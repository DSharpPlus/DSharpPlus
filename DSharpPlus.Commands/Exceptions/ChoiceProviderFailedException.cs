using System;

namespace DSharpPlus.Commands.Exceptions;

/// <summary>
/// Thrown if a choice provider failed to execute.
/// </summary>
public class ChoiceProviderFailedException : Exception
{
    public Type ProviderType { get; private set; }

    public ChoiceProviderFailedException(Type providerType, Exception? innerException) 
        : base($"Choice provider {providerType} failed.", innerException)
        => this.ProviderType = providerType;

    public override string ToString() 
        => $"Choice provider {this.ProviderType} failed with exception {this.InnerException}";
}
