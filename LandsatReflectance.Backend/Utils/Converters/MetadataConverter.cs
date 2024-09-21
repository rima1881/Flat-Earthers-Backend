using System.Text.Json;
using System.Text.Json.Serialization;
using LandsatReflectance.Backend.Models.UsgsApi.Types;

namespace LandsatReflectance.Backend.Utils;


// We need a custom converter since the "Value" property could either be a string or an int.
// The default implementation of the serializer throws since it only expects the declared type of the property on the model class.
// Here we convert the int into a string.


public class MetadataConverter : JsonConverter<Metadata>
{
    public override Metadata? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException($"Expected a \"JsonTokenType.StartObject\", got a {reader.TokenType.ToString()}");

        var metadata = new Metadata();

        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.EndObject:
                    return metadata;
                case JsonTokenType.PropertyName:
                    string? propertyName = reader.GetString();
                    if (propertyName is null)
                        break;

                    reader.Read();
                    DeserializeProperty(ref reader, options, propertyName, ref metadata);
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }
        
        throw new JsonException("Reached the end of the reader without encountering a \"JsonTokenType.StartObject\"");
    }
    
    private void DeserializeProperty(ref Utf8JsonReader reader, JsonSerializerOptions _, string propertyName, ref Metadata usgsApiResponse)
    {
        switch (propertyName)
        {
            case "id":
                usgsApiResponse.Id = reader.GetString() ?? string.Empty;
                break;
            case "fieldName":
                usgsApiResponse.FieldName = reader.GetString() ?? string.Empty;
                break;
            case "dictionaryLink":
                usgsApiResponse.DictionaryLink = reader.GetString() ?? string.Empty;
                break;
            case "value":
                usgsApiResponse.Value = reader.TokenType switch
                {
                    JsonTokenType.Number => reader.GetInt32().ToString(),
                    JsonTokenType.String => reader.GetString() ?? "",
                    JsonTokenType.Null => "",
                    _ => throw new JsonException($"Error parsing the value for the property \"Value\", expected a number or string, got token type\"{reader.TokenType.ToString()}\"")
                };
                break;
        }
    }

    public override void Write(Utf8JsonWriter writer, Metadata value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}