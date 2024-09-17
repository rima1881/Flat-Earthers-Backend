namespace LandsatReflectance.Backend.Models.UsgsApi.Types.Request;

public class SceneFilter
{
    public AcquisitionFilter? AcquisitionFilter { get; set; }
    public CloudCoverFilter? CloudCoverFilter { get; set; }
    public IngestFilter? IngestFilter { get; set; }
    public MetadataFilter? MetadataFilter { get; set; }
    public SpatialFilter? SpatialFilter { get; set; }

    public string DatasetName { get; set; } = string.Empty;
    public int[] SeasonalFilter { get; set; } = [];
}