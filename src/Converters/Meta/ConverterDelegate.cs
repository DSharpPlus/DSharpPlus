using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.CommandAll.Converters.Meta
{
    public delegate Task<IOptional> ConverterDelegate(ConverterContext context);
}
