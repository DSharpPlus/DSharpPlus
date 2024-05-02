using System.Threading.Tasks;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Converters;

public delegate Task<IOptional> ConverterDelegate<T>(ConverterContext context, T eventArgs) where T : AsyncEventArgs;
