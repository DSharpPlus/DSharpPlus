namespace DSharpPlus.Cache;

using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;

public interface IDiscordCache
{
    /// <summary>
    /// Add entity of type T to the cache
    /// </summary>
    /// <param name="entity">Entity to cache</param>
    /// <param name="key"></param>
    public ValueTask Add<T>(T entity, ICacheKey key);
    
    /// <summary>
    /// Remove entity with given key from the cache
    /// </summary>
    /// <param name="key"></param>
    public ValueTask Remove(ICacheKey key);
    
    /// <summary>
    /// Tries to get a cached entity of type T with given key
    /// </summary>
    /// <param name="key"></param>
    /// <param name="entity"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>Returns the entity if present otherwise returns null</returns>
    public ValueTask<T?> TryGet<T>(ICacheKey key);
}
