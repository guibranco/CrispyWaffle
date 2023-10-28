# Caching

## About

Cache data is easy with Crispy Waffle, there are available two **repositories** for cache:

- **MemoryCacheRepository**: A cache repository that stores data in application memory.
- **RedisCacheRepository**: A cache repository that uses Redis as a persistence mechanism.

There is also a helper class, **CacheManager** that makes cache usage easy.

## Examples

### Single cache repository + CacheManager helper class

The following example uses the **RedisCacheRepository** and the **CacheManager** helper class:

Process #1 (write data to cache):

```cs
ServiceLocator.Register<ICacheRepository, RedisCacheRepository>(LifeStyle.SINGLETON);
CacheManager.AddRepository<ICacheRepository>();

CacheManager.Set("some string text", "MyKey");
```

Process #2 (read data from cache - Redis):

```cs
ServiceLocator.Register<ICacheRepository, RedisCacheRepository>(LifeStyle.SINGLETON);
CacheManager.AddRepository<ICacheRepository>();

var cachedValueFromRedis = CacheManager.Get<string>("MyKey"); //some string text
```

### Multiple cache repositories (registration precedence)

Using multiple cache repositories:

```cs
ServiceLocator.Register<ICacheRepository, MemoryCacheRepository>(LifeStyle.SINGLETON);
ServiceLocator.Register<RedisCacheRepository>(LifeStyle.SINGLETON);

CacheManager.AddRepository<ICacheRepository>(); //or directly: CacheManager.AddRepository<MemoryCacheRepository>(); 
CacheManager.AddRepository<RedisCacheRepository>(); //RedisCacheRepository

var cachedValue = CacheManager.Get<string>("MyKey"); //first will lookup in MemoryCacheRepository then RedisCacheRepository

var cachedValueFromSpecificRepository = CachemManager.GetFrom<RedisCacheRepository, string>("MyKey"); //will get the value only from RedisCacheRepository.
```
