namespace LandsatReflectance.Backend.Models.UsgsApi.Endpoints;

[Serializable]
public class SceneListAddRequest
{
    public string ListId { get; set; } = string.Empty;
    public string DatasetName { get; set; } = string.Empty;
    public string IdField { get; set; } = "entityId";
    public string[] EntityIds { get; set; } = [];
    public string TimeToLive { get; set; } = "P1M";  // 1 month lifetime
    public bool CheckDownloadRestriction { get; set; } = false;
}

[Serializable]
public class SceneListAddResponse : IUsgsApiResponseData
{
    public int ListLength { get; set; }
}
