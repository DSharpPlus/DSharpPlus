using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Converters;

public delegate ValueTask<IOptional> ConverterDelegate(ConverterContext context);
