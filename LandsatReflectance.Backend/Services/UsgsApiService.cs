using System.Text;
using System.Text.Json;
using LandsatReflectance.Backend.Models.UsgsApi;
using LandsatReflectance.Backend.Models.UsgsApi.Endpoints;
using LandsatReflectance.Backend.Utils;
using LandsatReflectance.Backend.Utils.SourceGenerators;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace LandsatReflectance.Backend.Services;

public class UsgsApiService
{
    private readonly string m_username;
    private readonly string m_usgsToken;

    private readonly JsonSerializerOptions m_jsonSerializerOptions;
    private readonly HttpClient m_httpClient;

    public UsgsApiService(UsgsApiKeyService usgsApiKeyService, IOptions<JsonOptions> options)
    {
        m_username = usgsApiKeyService.Username;
        m_usgsToken = usgsApiKeyService.Token;
        m_jsonSerializerOptions = options.Value.SerializerOptions;
        
        m_httpClient = new HttpClient();
        m_httpClient.BaseAddress = new Uri("https://m2m.cr.usgs.gov/api/api/json/stable/");

        var loginTokenRequest = new LoginTokenRequest
        {
            Username = m_username,
            Token = m_usgsToken
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

        m_httpClient.DefaultRequestHeaders.Add("X-Auth-Token", loginTokenResponse.AuthToken);
    }


#region Query Endpoint

    public async Task<UsgsApiResponse<LoginTokenResponse>> QueryLoginToken(LoginTokenRequest loginTokenRequest)
    {
        string asJson = JsonSerializer.Serialize(loginTokenRequest, m_jsonSerializerOptions);
        return await QueryAsync<LoginTokenResponse>("login-token", asJson);
    }
    
    public async Task<UsgsApiResponse<SceneListAddResponse>> QuerySceneListAdd(SceneListAddResponse sceneListAddResponse)
    {
        string asJson = JsonSerializer.Serialize(sceneListAddResponse, m_jsonSerializerOptions);
        return await QueryAsync<SceneListAddResponse>("scene-list-add", asJson);
    }
    
    public async Task<UsgsApiResponse<SceneListGetResponse>> QuerySceneListGet(SceneListGetRequest sceneListGetRequest)
    {
        string asJson = JsonSerializer.Serialize(sceneListGetRequest, m_jsonSerializerOptions);
        return await QueryAsync<SceneListGetResponse>("scene-list-get", asJson);
    }
    
    public async Task<UsgsApiResponse<SceneMetadataListResponse>> QuerySceneMetadataList(SceneMetadataListRequest sceneMetadataListRequest)
    {
        string asJson = JsonSerializer.Serialize(sceneMetadataListRequest, m_jsonSerializerOptions);
        return await QueryAsync<SceneMetadataListResponse>("scene-metadata_list", asJson);
    }

    public async Task<UsgsApiResponse<SceneSearchResponse>> QuerySceneSearch(SceneSearchRequest sceneSearchRequest)
    {
        string asJson = JsonSerializer.Serialize(sceneSearchRequest, m_jsonSerializerOptions);
        return await QueryAsync<SceneSearchResponse>("scene-search", asJson);
    }
    
    
    private async Task<UsgsApiResponse<TResponseType>> QueryAsync<TResponseType>(
        string requestUri, 
        string requestContents) 
        where TResponseType : class, IUsgsApiResponseData
    {
        using var contents = new StringContent(requestContents, Encoding.UTF8, "application/json");
        var response = await m_httpClient.PostAsync(requestUri, contents);

        if (!response.IsSuccessStatusCode)
            throw new Exception();  // TODO: Write

        var responseContentsRaw = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(responseContentsRaw))
            throw new Exception();  // TODO: Write

        var deserializedObject = JsonSerializer.Deserialize<UsgsApiResponse<TResponseType>>(responseContentsRaw, m_jsonSerializerOptions);
        if (deserializedObject is null)
            throw new Exception();  // TODO: Write

        return deserializedObject;
    }
    
#endregion
}