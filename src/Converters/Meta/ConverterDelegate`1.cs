using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.CommandAll.Converters.Meta
{
    public delegate Task<Optional<TOutput>> ConverterDelegate<TOutput>(ConverterContext context);
}
