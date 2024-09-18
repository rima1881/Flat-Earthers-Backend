namespace LandsatReflectance.Backend.Models.UsgsApi.Types;

public class Metadata
{
    
    public string Id { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string DictionaryLink { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public static class MetadataExtensions
{ }