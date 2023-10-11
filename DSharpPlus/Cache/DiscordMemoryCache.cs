namespace DSharpPlus.Cache;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

public class DiscordMemoryCache : IDiscordCache
{
    private MemoryCache _cache = new(new MemoryCacheOptions());

    private Dictionary<Type, MemoryCacheEntryOptions> _memoryCacheEntryOptions;
    private Dictionary<Type, MethodInfo> _keyFunctions;

    public DiscordMemoryCache(CacheConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        this._memoryCacheEntryOptions = configuration.MemoryCacheEntryOptions;
        this._keyFunctions = this.GetKeyMethods();
    }

    public ValueTask Add<T>(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        MethodInfo? keyMethod = this._keyFunctions
            .Where(x => entity.GetType().IsAssignableFrom(x.Key)).FirstOrDefault()
            .Value;

        if (keyMethod is null)
        {
            return default;
        }

        MemoryCacheEntryOptions? entryOptions = this._memoryCacheEntryOptions
            .Where(x => entity.GetType().IsAssignableFrom(x.Key)).FirstOrDefault()
            .Value;

        if (entryOptions is null)
        {
            return default;
        }

        ICacheKey? key = (ICacheKey?)keyMethod.Invoke(null, new object[] {entity});
        ArgumentNullException.ThrowIfNull(key);
        _cache.Set(key, entity, entryOptions);
        return default;
    }

    public ValueTask Remove(ICacheKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        this._cache.Remove(key);
        return default;
    }

    public ValueTask<bool> TryGet<T>(ICacheKey key, out T? entity)
    {
        ArgumentNullException.ThrowIfNull(key);

        entity = this._cache.Get<T>(key);
        return new ValueTask<bool>(entity is not null);
    }

    public bool Validate(CacheConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        foreach (Type neededType in IDiscordCache.NeededTypes)
        {
            if (!this._memoryCacheEntryOptions.ContainsKey(neededType))
            {
                throw new ArgumentException($"Missing MemoryCacheEntryOptions for needed Type {neededType}");
            }

            if (!configuration.KeyFunctions.ContainsKey(neededType))
            {
                throw new ArgumentException($"Missing KeyFunction for needed Type {neededType}");
            }
        }

        foreach (MemoryCacheEntryOptions entryOptions in configuration.MemoryCacheEntryOptions.Values)
        {
            if (entryOptions.Size.HasValue)
            {
                throw new ArgumentException("Cache dont support limiting by Size");
            }
        }

        return true;
    }

    private Dictionary<Type, MethodInfo> GetKeyMethods()
    {
        Dictionary<Type, MethodInfo> keyMethods = new();

        Type[] dspTypes = Assembly.GetAssembly(typeof(DiscordClient))!.GetTypes();
        Type[]? userTypes = Assembly.GetEntryAssembly()?.GetTypes();

        IEnumerable<Type> types = dspTypes.Concat(userTypes ?? Array.Empty<Type>());
        MethodInfo[]? methods = types
            .SelectMany(x => x.GetMethods())
            .Where(x => x.IsStatic && x.ReturnType == typeof(ICacheKey))
            .ToArray();

        foreach (Type cachebleType in this._memoryCacheEntryOptions.Keys)
        {
            MethodInfo? method = methods?
                .Where(x => x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == cachebleType)
                .FirstOrDefault();
            if (method is null)
            {
                throw new ArgumentException($"Missing KeyFunction for Type {cachebleType}");
            }

            keyMethods.Add(cachebleType, method);
        }

        return keyMethods;
    }
}
