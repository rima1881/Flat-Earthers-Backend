using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LandsatReflectance.Backend.Utils;


// We need a custom converter since some dates in response data are in the format of "yyyy-MM-dd HH:mm:ss"
// (ex. "2024-09-15 00:00:00"). Serialization of this format is not directly supported.


public class CustomDateTimeConverter : JsonConverter<DateTime>
{
    private static string[] ValidDateFormats =
    [
        "yyyy-MM-dd HH:mm:sszzz",
        "yyyy-MM-dd HH:mm:ss"
    ];


    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            string? dateString = reader.GetString();
            if (dateString is null)
                throw new JsonException("Expected a string to parse into a date, got nothing.");

            if (DateTimeOffset.TryParse(dateString, out var dateTimeOffset))
                return dateTimeOffset.DateTime;
            
            /*
            if (DateTimeOffset.TryParseExact(dateString, ValidDateFormats[0], CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTimeOffset))
                return dateTimeOffset.DateTime;
            
            if (DateTime.TryParseExact(dateString, ValidDateFormats[1], CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return date;
             */
            
            throw new JsonException();

            // var formatsString = string.Join(", ", ValidDateFormats.Select(str => $"\"{str}\""));
            // throw new JsonException($"Unable to convert \"{dateString}\" to DateTime using any of the formats [{formatsString}].");
        }

        throw new JsonException($"Unexpected token type {reader.TokenType} when parsing DateTime.");
    }
    

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(ValidDateFormats[0], CultureInfo.InvariantCulture));
    }
}