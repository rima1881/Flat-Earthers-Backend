using System.Reflection;
using System.Text.Json;
using LandsatReflectance.Backend.Models;

namespace LandsatReflectance.Backend.Services;

public class UserService
{
    private readonly Lazy<User[]> _users = new(InitUsers);

    public User[] Users => _users.Value;

    
    public UserService()
    { }

    private static User[] InitUsers()
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
        var selectedRegion3 = new SelectedRegion
        {
            Path = "190",
            Row = "26",
            NotificationOffset = TimeSpan.MaxValue
        };
        var selectedRegion4 = new SelectedRegion
        {
            Path = "143",
            Row = "50",
            NotificationOffset = TimeSpan.MaxValue
        };
        var selectedRegion5 = new SelectedRegion
        {
            Path = "160",
            Row = "37",
            NotificationOffset = TimeSpan.MaxValue
        };
        var selectedRegion6 = new SelectedRegion
        {
            Path = "116",
            Row = "50",
            NotificationOffset = TimeSpan.MaxValue
        };
        
        
        var user1 = new User
        {
            Email = "amir@gmail.com",
            SelectedRegions = [selectedRegion1, selectedRegion2]
        };
        var user2 = new User
        {
            Email = "joyal@gmail.com",
            SelectedRegions = [selectedRegion3, selectedRegion1]
        };
        var user3 = new User
        {
            Email = "umar@gmail.com",
            SelectedRegions = [selectedRegion4, selectedRegion5, selectedRegion6]
        };

        return [ user1, user2, user3 ];
    }
}