using System.Text.Json.Serialization;
using LandsatReflectance.Backend.Utils;

namespace LandsatReflectance.Backend.Models.UsgsApi.Types.Request;


[JsonConverter(typeof(MetadataFilterConverter))]
public abstract class MetadataFilter 
{ }


[JsonConverter(typeof(MetadataFilterAndConverter))]
public class MetadataFilterAnd : MetadataFilter
{
    public MetadataFilter[] ChildFilters { get; set; } = [];
}


[JsonConverter(typeof(MetadataFilterOrConverter))]
public class MetadataFilterOr : MetadataFilter
{
    public MetadataFilter[] ChildFilters { get; set; } = [];
}


[JsonConverter(typeof(MetadataFilterValueConverter))]
public class MetadataFilterValue : MetadataFilter
{
    public enum MetadataValueOperand 
    {
        Equals, Like
    }

    public string FilterId { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public MetadataValueOperand Operand { get; set; }
}


[JsonConverter(typeof(MetadataFilterBetweenConverter))]
public class MetadataFilterBetween : MetadataFilter
{
    public string FilterId { get; set; } = string.Empty;
    public int FirstValue { get; set; }
    public int SecondValue { get; set; }
}
