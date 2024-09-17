using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LandsatReflectance.Backend.Utils;


// We need a custom converter since some dates in response data are in the format of "yyyy-MM-dd HH:mm:ss"
// (ex. "2024-09-15 00:00:00"). Serialization of this format is not directly supported.


public class CustomDateTimeConverter : JsonConverter<DateTime>
{
    private const string DateFormat = "yyyy-MM-dd HH:mm:ss";


    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            string? dateString = reader.GetString();
            if (dateString is null)
                throw new JsonException("Expected a string to parse into a date, got nothing.");

            if (DateTime.TryParseExact(dateString, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                return date;
            
            throw new JsonException($"Unable to convert \"{dateString}\" to DateTime using format \"{DateFormat}\".");
        }

        throw new JsonException($"Unexpected token type {reader.TokenType} when parsing DateTime.");
    }
    

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(DateFormat, CultureInfo.InvariantCulture));
    }
}