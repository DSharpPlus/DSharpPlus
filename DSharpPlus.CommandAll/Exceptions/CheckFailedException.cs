namespace DSharpPlus.CommandAll.Exceptions;
using System;
using DSharpPlus.CommandAll.ContextChecks;

public sealed class CheckFailedException : Exception
{
    public ContextCheckAttribute Check { get; init; }

    public CheckFailedException(ContextCheckAttribute check, Exception? innerException = null, string? message = null) : base(message ?? $"Check {check.GetType().Name} failed{(innerException is null ? "." : " with an error.")}", innerException)
    {
        ArgumentNullException.ThrowIfNull(check, nameof(check));
        this.Check = check;
    }
}
