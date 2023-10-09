namespace DSharpPlus.Cache;

using System.Threading.Tasks;
using Entities;

public interface IDiscordCache
{
    ValueTask Add<T>(T entity);
    
    ValueTask Remove(ICacheKey key);
    
    ValueTask<bool> TryGet<T>(ICacheKey key, out T? entity);
}
