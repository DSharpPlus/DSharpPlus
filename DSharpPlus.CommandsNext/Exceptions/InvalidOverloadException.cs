namespace DSharpPlus.CommandsNext.Exceptions;

using System;
using System.Reflection;

/// <summary>
/// Thrown when the command service fails to build a command due to a problem with its overload.
/// </summary>
public sealed class InvalidOverloadException : Exception
{
    /// <summary>
    /// Gets the method that caused this exception.
    /// </summary>
    public MethodInfo Method { get; }

    /// <summary>
    /// Gets or sets the argument that caused the problem. This can be null.
    /// </summary>
    public ParameterInfo? Parameter { get; }

    /// <summary>
    /// Creates a new <see cref="InvalidOverloadException"/>.
    /// </summary>
    /// <param name="message">Exception message.</param>
    /// <param name="method">Method that caused the problem.</param>
    /// <param name="parameter">Method argument that caused the problem.</param>
    public InvalidOverloadException(string message, MethodInfo method, ParameterInfo? parameter)
        : base(message)
    {
        Method = method;
        Parameter = parameter;
    }

    /// <summary>
    /// Creates a new <see cref="InvalidOverloadException"/>.
    /// </summary>
    /// <param name="message">Exception message.</param>
    /// <param name="method">Method that caused the problem.</param>
    public InvalidOverloadException(string message, MethodInfo method)
        : this(message, method, null)
    { }

    /// <summary>
    /// Returns a string representation of this <see cref="InvalidOverloadException"/>.
    /// </summary>
    /// <returns>A string representation.</returns>
    public override string ToString() =>
        // much like System.ArgumentNullException works
        Parameter == null
            ? $"{GetType()}: {Message}\nMethod: {Method} (declared in {Method.DeclaringType})"
            : $"{GetType()}: {Message}\nMethod: {Method} (declared in {Method.DeclaringType})\nArgument: {Parameter.ParameterType} {Parameter.Name}";
}
