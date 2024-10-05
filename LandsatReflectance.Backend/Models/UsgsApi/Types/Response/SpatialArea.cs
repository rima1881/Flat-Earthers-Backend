namespace LandsatReflectance.Backend.Models.UsgsApi.Types;

public class SpatialArea
{
    public string Type { get; set; } = string.Empty;
    public object[] Coordinates { get; set; } = [];  // TODO: Find a better way to represent this, as it varies between 'Type'

    // public double[][][] Coordinates { get; set; } = [];
}