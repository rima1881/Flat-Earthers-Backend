namespace LandsatReflectance.Backend.Models.UsgsApi.Endpoints;

public class LoginTokenRequest
{
    
}

public class LoginTokenResponse : IUsgsApiResponseData
{
    public string AuthToken { get; set; } = string.Empty;
}
