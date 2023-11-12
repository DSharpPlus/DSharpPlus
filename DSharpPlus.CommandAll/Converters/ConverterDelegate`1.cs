namespace DSharpPlus.CommandAll.Converters;
using System.Threading.Tasks;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Entities;

public delegate Task<IOptional> ConverterDelegate<T>(ConverterContext context, T eventArgs) where T : AsyncEventArgs;
