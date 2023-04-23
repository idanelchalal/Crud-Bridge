using Microsoft.Extensions.Caching.Memory;

namespace crudApi
{
    public class CachedMemory
    {
            public MemoryCache Cache { get; } = new MemoryCache(
                new MemoryCacheOptions
                {
                    SizeLimit = 1024
                });
        }
    
}
