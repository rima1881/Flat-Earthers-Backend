using System.Text.Json;
using System.Text.Json.Serialization;
using LandsatReflectance.Backend.Models.UsgsApi;
using LandsatReflectance.Backend.Models.UsgsApi.Endpoints;


namespace LandsatReflectance.Backend.Utils;


public class UsgsApiResponseConverter<T> : JsonConverter<UsgsApiResponse<T>> where T : class, IUsgsApiResponseData
{
    
#region Read/Deserialization

    public override UsgsApiResponse<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException($"Expected a \"JsonTokenType.StartObject\", got a {reader.TokenType.ToString()}");

        var usgsApiResponse = new UsgsApiResponse<T>();

        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.EndObject:
                    return usgsApiResponse;
                case JsonTokenType.PropertyName:
                    string? propertyName = reader.GetString();
                    if (propertyName is null)
                        break;

                    reader.Read();
                    DeserializeProperty(ref reader, options, propertyName, ref usgsApiResponse);
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }
        
        throw new JsonException("Reached the end of the reader without encountering a \"JsonTokenType.StartObject\"");
    }

    private static void DeserializeProperty(ref Utf8JsonReader reader, JsonSerializerOptions options, string propertyName, ref UsgsApiResponse<T> usgsApiResponse)
    {
        switch (propertyName)
        {
            case "requestId":
                usgsApiResponse.RequestId = reader.GetInt32();
                break;
            case "version":
                usgsApiResponse.Version = reader.GetString() ?? string.Empty;
                break;
            case "data":
                HandleDataDeserialization(ref reader, options, ref usgsApiResponse);
                break;
            case "errorCode":
                usgsApiResponse.ErrorCode = reader.TokenType is not JsonTokenType.Null 
                    ? reader.GetInt32() 
                    : null;
                break;
            case "errorMessage":
                usgsApiResponse.ErrorMessage = reader.GetString();
                break;
        }
    }

    private static void HandleDataDeserialization(ref Utf8JsonReader reader, JsonSerializerOptions options, ref UsgsApiResponse<T> usgsApiResponse)
    {
        using var jsonDocument = JsonDocument.ParseValue(ref reader);
        string rawJson = jsonDocument.RootElement.GetRawText();
        
        switch (typeof(T))
        {
            case var t when t == typeof(LoginTokenResponse):
                var loginTokenResponse = new LoginTokenResponse
                {
                    AuthToken = JsonSerializer.Deserialize<string>(rawJson, options) ?? ""
                };
                usgsApiResponse.Data = (T)(object)loginTokenResponse;
                break;
            
            case var t when t == typeof(SceneListAddResponse):
                var listLen = JsonSerializer.Deserialize<int?>(rawJson);
                if (listLen is not null)
                {
                    usgsApiResponse.Data = (T)(object)new SceneListAddResponse
                    {
                        ListLength = listLen.Value
                    };
                }
                break;
            
            case var t when t == typeof(SceneListGetResponse):
                var entityIds = JsonSerializer
                    .Deserialize<SceneListGetHelperClass[]>(rawJson, options)
                    ?.Select(data => data.EntityId)
                    .ToArray();

                if (entityIds is not null)
                {
                    usgsApiResponse.Data = (T)(object)new SceneListGetResponse
                    {
                        EntityIds = entityIds
                    };
                }
                break;
            
            case var t when t == typeof(SceneMetadataListResponse):
                var returnedSceneData = JsonSerializer.Deserialize<SceneData[]>(rawJson, options);
                if (returnedSceneData is not null)
                {
                    usgsApiResponse.Data = (T)(object)new SceneMetadataListResponse
                    {
                        ReturnedSceneData = returnedSceneData
                    };
                }
                break;
            
            case var t when t == typeof(SceneSearchResponse):
                var sceneSearchResponse = JsonSerializer.Deserialize<SceneSearchResponse>(rawJson, options);
                usgsApiResponse.Data = sceneSearchResponse is not null ? (T)(object)sceneSearchResponse : null;
                break;
        }
    }

#endregion


    public override void Write(Utf8JsonWriter writer, UsgsApiResponse<T> value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}



#region Helper Model Classes

class SceneListGetHelperClass
{
    [JsonPropertyName("entityId")]
    public string EntityId { get; set; } = string.Empty;
    [JsonPropertyName("datasetName")]
    public string DatasetName { get; set; } = string.Empty;
}

#endregion
