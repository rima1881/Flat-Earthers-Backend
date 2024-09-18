namespace LandsatReflectance.Backend.Services;

public class UsgsApiKeyService
{
    public string Username { get; set; } = GetEnvironmentVariable("LANDSAT_REFLECTANCE_USGS_USERNAME");
    public string Token { get; set; } = GetEnvironmentVariable("LANDSAT_REFLECTANCE_USGS_TOKEN");

    private static string GetEnvironmentVariable(string envVarName)
    {
        var envVar = Environment.GetEnvironmentVariable(envVarName);
        if (envVar is null)
            throw new ArgumentException($"For \"{typeof(UsgsApiKeyService)}\", expected the environment variable \"{envVarName}\", but was not found.");

        return envVar;
    }
}