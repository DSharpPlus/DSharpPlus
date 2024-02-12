namespace DSharpPlus.Commands.ContextChecks;

using System;

[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
public abstract class ContextCheckAttribute : Attribute;
