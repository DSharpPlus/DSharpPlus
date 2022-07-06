using DSharpPlus.Caching.Abstractions;

using Microsoft.Extensions.Options;

namespace DSharpPlus.Caching.Memory
{
    /// <summary>
    /// Represents options for <see cref="MemoryCacheService"/>.
    /// </summary>
    public class MemoryCacheOptions : AbstractCacheOptions, IOptions<MemoryCacheOptions>
    {
        MemoryCacheOptions IOptions<MemoryCacheOptions>.Value => this;
    }
}
