using System.Threading.Tasks;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Entities;

namespace DSharpPlus.CommandAll.Converters
{
    public delegate Task<IOptional> ConverterDelegate<T>(ConverterContext context, T eventArgs) where T : AsyncEventArgs;
}
