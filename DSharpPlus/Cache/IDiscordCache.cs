namespace DSharpPlus.Cache;

using System.Threading.Tasks;

public interface IDiscordCache
{
    /// <summary>
    /// Add entity of type T to the cache
    /// </summary>
    /// <param name="entity">Entity to cache</param>
    public ValueTask Add<T>(T entity);
    
    /// <summary>
    /// Remove entity with given key from the cache
    /// </summary>
    /// <param name="key"></param>
    public ValueTask Remove(ICacheKey key);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="entity"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public ValueTask<bool> TryGet<T>(ICacheKey key, out T? entity);

    
    /// <summary>
    /// Validates if the given configuration is valid for the cache implementation
    /// </summary>
    public bool Validate(CacheConfiguration configuration);
}
