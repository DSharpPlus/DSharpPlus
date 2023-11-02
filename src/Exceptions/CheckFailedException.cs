using System;
using DSharpPlus.CommandAll.ContextChecks;

namespace DSharpPlus.CommandAll.Exceptions
{
    public sealed class CheckFailedException(ContextCheckAttribute Check, Exception? innerException = null, string? message = null) : Exception(message ?? $"Check {Check.GetType().Name} failed{(innerException is null ? "." : " with an error.")}", innerException);
}
