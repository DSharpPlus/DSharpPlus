using DSharpPlus.Core.Caching.Abstractions;

using Microsoft.Extensions.Options;

namespace DSharpPlus.Core.Caching.Memory
{
    /// <summary>
    /// Represents options for <see cref="MemoryCacheService"/>.
    /// </summary>
    public class MemoryCacheOptions : AbstractCacheOptions, IOptions<MemoryCacheOptions>
    {
        MemoryCacheOptions IOptions<MemoryCacheOptions>.Value => this;
    }
}
