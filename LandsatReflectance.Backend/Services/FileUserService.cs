using System.Reflection;
using System.Text.Json;
using LandsatReflectance.Backend.Models;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace LandsatReflectance.Backend.Services;

public interface IUserService
{
    public void AddUser(User user);

    public User? TryEditUser(string email, Action<User> mapUser);

    public User? TryGetUser(string email);
    
    public User? TryRemoveUser(string email);
    
    [Obsolete("Use this method with care. Only meant for testing.")]
    public void ClearAll();
}

public class FileUserService : IUserService
{
    private static readonly string ExecutingAssemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
    private static readonly string SaveFilePath = @$"{ExecutingAssemblyDirectory}\Data\users.json";
    
    private readonly ILogger<FileUserService> m_logger;
    private readonly JsonSerializerOptions m_jsonSerializerOptions;
    private List<User> m_users = new();
    
    public FileUserService(ILogger<FileUserService> logger, IOptions<JsonOptions> jsonOptions)
    {
        m_logger = logger;
        m_jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
        
        InitUsersList();
    }
    
    public void AddUser(User user)
    {
        m_users.Add(user);
        
        File.WriteAllText(SaveFilePath, JsonSerializer.Serialize(m_users, m_jsonSerializerOptions));
    }

    public User? TryEditUser(string email, Action<User> mapUser)
    {
        User? editedUser = null;
        foreach (var user in m_users)
        {
            if (string.Equals(user.Email, email, StringComparison.InvariantCultureIgnoreCase))
            {
                mapUser(user);
                editedUser = user;
            }
        }

        if (editedUser is not null)
        {
            File.WriteAllText(SaveFilePath, JsonSerializer.Serialize(m_users, m_jsonSerializerOptions));
        }

        return editedUser;
    }

    public User? TryGetUser(string email)
    {
        return m_users.FirstOrDefault(user => string.Equals(user.Email, email, StringComparison.InvariantCultureIgnoreCase));
    }

    public User? TryRemoveUser(string email)
    {
        var removedUser = TryGetUser(email);

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