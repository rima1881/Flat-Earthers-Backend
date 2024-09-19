namespace LandsatReflectance.Backend.Models.UsgsApi.Endpoints;

[Serializable]
public class SceneMetadataListRequest
{
    public string DatasetName { get; set; } = string.Empty;
    public string ListId { get; set; } = string.Empty;
    public string MetadataType { get; set; } = "full";
    public bool IncludeNullMetadataValues { get; set; } = true;
    public bool UseCustomization { get; set; } = false;
}

[Serializable]
public class SceneMetadataListResponse : IUsgsApiResponseData
{
    public SceneData[] ReturnedSceneData { get; set; } = [];
}
