using System;
using DSharpPlus.CommandAll.Checks;

namespace DSharpPlus.CommandAll.Exceptions
{
    public sealed class CheckFailedException(ContextCheckAttribute Check, Exception? innerException = null, string? message = null) : Exception(message ?? $"Check {Check.GetType().Name} failed{(innerException is null ? "." : " with an error.")}", innerException);
}
