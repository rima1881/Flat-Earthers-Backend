using Microsoft.Extensions.Caching.Memory;

namespace LandsatReflectance.Backend.Services;

public class SceneEntityIdCachingService
{
    private readonly IMemoryCache m_memoryCache;

    public SceneEntityIdCachingService(IMemoryCache memoryCache)
    {
        m_memoryCache = memoryCache;
    }
    
    public static string PathAndRowToCacheKey(int path, int row) => $"{path},{row}";

    public string AddSceneEntityIds(int path, int row, string[] sceneEntityIds)
    {
        string key = PathAndRowToCacheKey(path, row);
        m_memoryCache.Set(key, sceneEntityIds);
        return key;
    }

    public string[]? GetSceneEntityIds(int path, int row)
    {
        try
        {
            string key = PathAndRowToCacheKey(path, row);
            if (m_memoryCache.TryGetValue(key, out object? value) && value is not null)
            {
                return (string[])value;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
    
    
}