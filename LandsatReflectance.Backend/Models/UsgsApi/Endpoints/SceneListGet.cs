namespace LandsatReflectance.Backend.Models.UsgsApi.Endpoints;

[Serializable]
public class SceneListGetRequest
{
    public string ListId { get; set; } = string.Empty;
    public string DatasetName { get; set; } = string.Empty;
    public int StartingNumber { get; set; } = 0;
    public int MaxResults { get; set; } = 50;
}

[Serializable]
public class SceneListGetResponse : IUsgsApiResponseData
{
    public string[] EntityIds { get; set; } = [];
}
