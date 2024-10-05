namespace LandsatReflectance.Backend.Models.UsgsApi.Types;

public class Metadata
{
    
    public string Id { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string DictionaryLink { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public static class MetadataExtensions
{
    public static Metadata? TryGetMetadataByName(Metadata[] metadatas, string fieldName)
    {
        foreach (var metadata in metadatas)
        {
            if (string.Equals(fieldName, metadata.FieldName, StringComparison.InvariantCultureIgnoreCase))
            {
                return metadata;
            }
        }

        return null;
    }
}