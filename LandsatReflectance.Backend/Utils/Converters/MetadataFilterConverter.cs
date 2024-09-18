using System.Text.Json;
using System.Text.Json.Serialization;
using LandsatReflectance.Backend.Models.UsgsApi.Types.Request;

namespace LandsatReflectance.Backend.Utils;

public class MetadataFilterConverter : JsonConverter<MetadataFilter>
{
    public override MetadataFilter? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, MetadataFilter metadataFilter, JsonSerializerOptions options)
    {
        writer.WriteRawValue(JsonSerializer.Serialize(metadataFilter, metadataFilter.GetType(), options));
    }
}

public class MetadataFilterAndConverter : JsonConverter<MetadataFilterAnd>
{
    public override MetadataFilterAnd? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, MetadataFilterAnd metadataFilterAnd, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        writer.WriteString("filterType", "and");
        
        writer.WriteStartArray("childFilters");
        foreach (var childFilter in metadataFilterAnd.ChildFilters)
        {
            _ = childFilter.GetType() switch
            {
                var t when t == typeof(MetadataFilterValue) => 0,
                var t when t == typeof(MetadataFilterBetween) => 0,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            writer.WriteRawValue(JsonSerializer.Serialize(childFilter, childFilter.GetType(), options));
        }
        writer.WriteEndArray();
        
        writer.WriteEndObject();
    }
}

public class MetadataFilterOrConverter : JsonConverter<MetadataFilterOr>
{
    public override MetadataFilterOr? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, MetadataFilterOr metadataFilterOr, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        writer.WriteString("filterType", "or");
        
        writer.WriteStartArray("childFilters");
        foreach (var childFilter in metadataFilterOr.ChildFilters)
        {
            _ = childFilter.GetType() switch
            {
                var t when t == typeof(MetadataFilterValue) => 0,
                var t when t == typeof(MetadataFilterBetween) => 0,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            writer.WriteRawValue(JsonSerializer.Serialize(childFilter, childFilter.GetType(), options));
        }
        writer.WriteEndArray();
        
        writer.WriteEndObject();
    }
}


// TODO: Find a way use 'nameof', while using the json serializer options


public class MetadataFilterValueConverter : JsonConverter<MetadataFilterValue>
{
    public override MetadataFilterValue? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, MetadataFilterValue metadataFilterValue, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        writer.WriteString("filterType", "value");
        writer.WriteString("filterId", metadataFilterValue.FilterId);
        writer.WriteString("value", metadataFilterValue.Value);

        var operandAsString = metadataFilterValue.Operand switch
        {
            MetadataFilterValue.MetadataValueOperand.Equals => "=",
            MetadataFilterValue.MetadataValueOperand.Like => "like",
            _ => throw new ArgumentOutOfRangeException()
        };
        writer.WriteString("operand", operandAsString);
        
        writer.WriteEndObject();
    }
}

public class MetadataFilterBetweenConverter : JsonConverter<MetadataFilterBetween>
{
    public override MetadataFilterBetween? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, MetadataFilterBetween metadataFilterBetween, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        writer.WriteString("filterType", "between");
        writer.WriteString("filterId", metadataFilterBetween.FilterId);
        writer.WriteNumber("firstValue", metadataFilterBetween.FirstValue);
        writer.WriteNumber("secondValue", metadataFilterBetween.SecondValue);
        
        writer.WriteEndObject();
    }
}
