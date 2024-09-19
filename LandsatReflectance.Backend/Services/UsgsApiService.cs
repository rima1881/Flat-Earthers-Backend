using System.Text;
using System.Text.Json;
using LandsatReflectance.Backend.Models.UsgsApi;
using LandsatReflectance.Backend.Models.UsgsApi.Endpoints;
using LandsatReflectance.Backend.Utils;

namespace LandsatReflectance.Backend.Services;

public class UsgsApiService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new UsgsApiResponseConverter<SceneSearchResponse>(),
            new UsgsApiResponseConverter<LoginTokenResponse>(),
            new MetadataConverter(),
            new CustomDateTimeConverter()
        }
    };
    
    
    public readonly string Username;
    public readonly string UsgsToken;

    public readonly HttpClient HttpClient;

    public UsgsApiService(UsgsApiKeyService usgsApiKeyService)
    {
        Username = usgsApiKeyService.Username;
        UsgsToken = usgsApiKeyService.Token;
        
        HttpClient = new HttpClient();
        HttpClient.BaseAddress = new Uri("https://m2m.cr.usgs.gov/api/api/json/stable/");

        var loginTokenRequest = new LoginTokenRequest
        {
            Username = Username,
            Token = UsgsToken
        };

        var response = QueryLoginToken(loginTokenRequest).Result;

        if (response.ErrorCode is not null)
        {
            throw new NotImplementedException();
        }

        var loginTokenResponse = response.Data;
        if (loginTokenResponse is null)
        {
            throw new NotImplementedException();
        }

        HttpClient.DefaultRequestHeaders.Add("X-Auth-Token", loginTokenResponse.AuthToken);
    }


#region Query Endpoint

    public async Task<UsgsApiResponse<SceneSearchResponse>> QuerySceneSearch(SceneSearchRequest sceneSearchRequest)
    {
        string asJson = JsonSerializer.Serialize(sceneSearchRequest, JsonSerializerOptions);
        return await QueryAsync<SceneSearchResponse>(HttpClient, "scene-search", asJson);
    }

    public async Task<UsgsApiResponse<LoginTokenResponse>> QueryLoginToken(LoginTokenRequest loginTokenRequest)
    {
        string asJson = JsonSerializer.Serialize(loginTokenRequest, JsonSerializerOptions);
        return await QueryAsync<LoginTokenResponse>(HttpClient, "login-token", asJson);
    }
    
    private static async Task<UsgsApiResponse<TResponseType>> QueryAsync<TResponseType>(
        HttpClient httpClient, 
        string requestUri, 
        string requestContents) 
        where TResponseType : class, IUsgsApiResponseData
    {
        using var contents = new StringContent(requestContents, Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(requestUri, contents);

        if (!response.IsSuccessStatusCode)
            throw new Exception();  // TODO: Write

        var responseContentsRaw = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(responseContentsRaw))
            throw new Exception();  // TODO: Write

        var deserializedObject = JsonSerializer.Deserialize<UsgsApiResponse<TResponseType>>(responseContentsRaw, JsonSerializerOptions);
        if (deserializedObject is null)
            throw new Exception();  // TODO: Write

        return deserializedObject;
    }
    
#endregion
}