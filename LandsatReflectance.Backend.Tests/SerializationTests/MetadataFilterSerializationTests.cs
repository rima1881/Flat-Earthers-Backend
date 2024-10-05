using System.Text.Json;
using LandsatReflectance.Backend.Models.UsgsApi.Types.Request;

namespace LandsatReflectance.Backend.Tests.SerializationTests;

public class MetadataFilterSerializationTests
{
    [Test]
    public void Test1()
    {
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
        };
        
        var valueFilter1 = new MetadataFilterValue
        {
            FilterId = "5e83d14fb9436d88",
            Value = "28",
            Operand = MetadataFilterValue.MetadataValueOperand.Equals
        };
        var valueFilter2 = new MetadataFilterValue
        {
            FilterId = "5e83d14ff1eda1b8",
            Value = "14",
            Operand = MetadataFilterValue.MetadataValueOperand.Equals
        };
        MetadataFilter andFilter = new MetadataFilterAnd
        {
            ChildFilters = [valueFilter1, valueFilter2]
        };

        _ = JsonSerializer.Serialize(andFilter, jsonSerializerOptions);
        
        Assert.Pass();
    }
}