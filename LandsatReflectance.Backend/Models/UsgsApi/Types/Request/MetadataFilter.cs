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
    
    public required string FilterId { get; set; }
    public required string Value { get; set; }
    public required MetadataValueOperand Operand { get; set; }
}


[JsonConverter(typeof(MetadataFilterBetweenConverter))]
public class MetadataFilterBetween : MetadataFilter
{
    public required string FilterId { get; set; }
    public required int FirstValue { get; set; }
    public required int SecondValue { get; set; }
}
