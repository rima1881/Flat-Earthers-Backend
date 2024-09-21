using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using LandsatReflectance.Backend.Models.UsgsApi;
using LandsatReflectance.Backend.Models.UsgsApi.Endpoints;

namespace LandsatReflectance.Backend.Utils.SourceGenerators;

[JsonSerializable(typeof(UsgsApiResponse<LoginTokenResponse>), TypeInfoPropertyName = "UsgsApiResponseLoginTokenResponse" )]
[JsonSerializable(typeof(UsgsApiResponse<SceneListAddResponse>), TypeInfoPropertyName = "UsgsApiResponseSceneListAddResponse" )]
[JsonSerializable(typeof(UsgsApiResponse<SceneListGetResponse>), TypeInfoPropertyName = "UsgsApiResponseSceneListGetResponse" )]
[JsonSerializable(typeof(UsgsApiResponse<SceneMetadataListResponse>), TypeInfoPropertyName = "UsgsApiResponseSceneMetadataListResponse" )]
[JsonSerializable(typeof(UsgsApiResponse<SceneSearchResponse>), TypeInfoPropertyName = "UsgsApiResponseSceneSearchResponse" )]
public class UsgsApiResponseJsonContext : JsonSerializerContext
{
    private JsonSerializerContext _jsonSerializerContextImplementation;

    public UsgsApiResponseJsonContext(JsonSerializerOptions? options) : base(options)
    { }

    public static JsonSerializerOptions Options => new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new UsgsApiResponseConverter<LoginTokenResponse>(),
            new UsgsApiResponseConverter<SceneListAddResponse>(),
            new UsgsApiResponseConverter<SceneListGetResponse>(),
            new UsgsApiResponseConverter<SceneMetadataListResponse>(),
            new UsgsApiResponseConverter<SceneSearchResponse>(),
            new MetadataConverter(),
            new CustomDateTimeConverter()
        }
    };


    public override JsonTypeInfo? GetTypeInfo(Type type)
    {
        return _jsonSerializerContextImplementation.GetTypeInfo(type);
    }

    protected override JsonSerializerOptions? GeneratedSerializerOptions { get; }
}