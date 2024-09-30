using System.Reflection;
using System.Text.Json;
using LandsatReflectance.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LandsatReflectance.Backend.Services;

public interface IUserService
{
    public void AddUser(User user);

    public void EditUser(string email, Action<User> mapUser);

    public User? GetUser(string email);
    
    public User? RemoveUser(string email);
    
    [Obsolete("Use this method with care. Only meant for testing.")]
    public void ClearAll();
}

public class UserService : IUserService
{
    private static readonly string ExecutingAssemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
    private static readonly string SaveFilePath = @$"{ExecutingAssemblyDirectory}\Data\users.json";
    
    private readonly ILogger<UserService> m_logger;
    private readonly JsonSerializerOptions m_jsonSerializerOptions;
    private List<User> m_users = new();
    
    public UserService(ILogger<UserService> logger, IOptions<JsonOptions> jsonOptions)
    {
        m_logger = logger;
        m_jsonSerializerOptions = jsonOptions.Value.JsonSerializerOptions;
        
        InitUsersList();
    }
    
    public void AddUser(User user)
    {
        m_users.Add(user);
        
        File.WriteAllText(SaveFilePath, JsonSerializer.Serialize(m_users, m_jsonSerializerOptions));
    }

    public void EditUser(string email, Action<User> mapUser)
    {
        bool hasAnythingBeenModified = false;
        foreach (var user in m_users)
        {
            if (string.Equals(user.Email, email, StringComparison.InvariantCultureIgnoreCase))
            {
                mapUser(user);
                hasAnythingBeenModified = true;
            }
        }

        if (hasAnythingBeenModified)
        {
            File.WriteAllText(SaveFilePath, JsonSerializer.Serialize(m_users, m_jsonSerializerOptions));
        }
    }

    public User? GetUser(string email)
    {
        return m_users.FirstOrDefault(user => string.Equals(user.Email, email, StringComparison.InvariantCultureIgnoreCase));
    }

    public User? RemoveUser(string email)
    {
        var removedUser = GetUser(email);

        if (removedUser is not null)
        {
            _ = m_users.RemoveAll(user => string.Equals(user.Email, email, StringComparison.InvariantCultureIgnoreCase));
        }

        return removedUser;
    }

    public void ClearAll()
    {
        m_users.Clear();
        File.WriteAllText(SaveFilePath, "[]");
    }
    

    private void InitUsersList()
    {
        if (!File.Exists(SaveFilePath))
        {
            File.WriteAllText(SaveFilePath, "[]");
        }

        m_users = JsonSerializer.Deserialize<List<User>>(File.ReadAllText(SaveFilePath), m_jsonSerializerOptions) ?? [];
    }
}

public class DatabaseUserService : IUserService
{
    private ILogger<DatabaseUserService> m_logger;
    private string m_dbConnectionString;
    
    public DatabaseUserService(ILogger<DatabaseUserService> logger, KeysService keysService)
    {
        m_logger = logger;
        m_dbConnectionString = keysService.DbConnectionString;
    }
    
    
    public void AddUser(User user)
    {
        throw new NotImplementedException();
    }

    public void EditUser(string email, Action<User> mapUser)
    {
        throw new NotImplementedException();
    }

    public User? GetUser(string email)
    {
        throw new NotImplementedException();
    }

    public User? RemoveUser(string email)
    {
        throw new NotImplementedException();
    }

    public void ClearAll()
    {
        throw new NotImplementedException();
    }
}