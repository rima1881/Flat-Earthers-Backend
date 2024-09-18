using System.Text.Json;
using LandsatReflectance.Backend.Models;

namespace LandsatReflectance.Backend.Tests;

public class UserSerializationTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        var selectedRegion1 = new SelectedRegion
        {
            Path = "14",
            Row = "27",
            NotificationOffset = TimeSpan.MaxValue
        };
        var selectedRegion2 = new SelectedRegion
        {
            Path = "19",
            Row = "11",
            NotificationOffset = TimeSpan.MaxValue
        };
        var user = new User
        {
            Email = "amir@gmail.com",
            SelectedRegions = [selectedRegion1, selectedRegion2]
        };

        var jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
        };
        
        _ = JsonSerializer.Serialize(user, jsonSerializerOptions);
        
        Assert.Pass();
    }
}