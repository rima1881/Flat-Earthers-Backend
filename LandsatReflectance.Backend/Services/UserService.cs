using System.Reflection;
using System.Text.Json;
using LandsatReflectance.Backend.Models;

namespace LandsatReflectance.Backend.Services;

public class UserService
{
    private static readonly string JsonSaveFilePath = @"Files\users.json";

    private User[] _users = [];

    public User[] Users
    {
        get => _users;
        private set => _users = value;
    }
    
    
    public UserService()
    {
        Users = GetUsers();
    }

    private static User[] GetUsers()
    {
        if (!File.Exists(JsonSaveFilePath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(JsonSaveFilePath) ?? "");
            File.WriteAllText(JsonSaveFilePath, "[]");
        }

        var fullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", JsonSaveFilePath);
        var rawJson = File.ReadAllText(fullPath);
        return JsonSerializer.Deserialize<User[]>(rawJson) ?? [];
    }
}