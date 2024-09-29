namespace LandsatReflectance.Backend.Services;

public class KeysService
{
    public string UsgsUsername { get; set; } = GetEnvironmentVariable("LANDSAT_REFLECTANCE_USGS_USERNAME");
    public string UsgsAppToken { get; set; } = GetEnvironmentVariable("LANDSAT_REFLECTANCE_USGS_TOKEN");
    
    public string AuthSecretKey { get; set; } = GetEnvironmentVariable("FLAT_EARTHERS_AUTH_SECRET_KEY");

    private static string GetEnvironmentVariable(string envVarName)
    {
        var envVar = Environment.GetEnvironmentVariable(envVarName);
        if (envVar is null)
            throw new ArgumentException($"For \"{typeof(KeysService)}\", expected the environment variable \"{envVarName}\", but was not found.");

        return envVar;
    }
}