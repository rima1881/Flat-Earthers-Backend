namespace LandsatReflectance.Backend.Models.UsgsApi.Endpoints;

public class LoginTokenRequest
{
    public string Username { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

public class LoginTokenResponse : IUsgsApiResponseData
{
    public string AuthToken { get; set; } = string.Empty;
}
