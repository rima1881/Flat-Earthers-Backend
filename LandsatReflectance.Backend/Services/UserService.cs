using System.Reflection;
using System.Text.Json;
using LandsatReflectance.Backend.Models;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;

namespace LandsatReflectance.Backend.Services;

public interface IUserService
{
    public Task AddUser(User user);

    public Task<User?> TryEditUser(string email, Action<User> mapUser);

    public Task<User?> TryGetUser(string email);
    
    public Task<User?> TryRemoveUser(string email);
    
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
    
    public Task AddUser(User user)
    {
        m_users.Add(user);
        
        File.WriteAllText(SaveFilePath, JsonSerializer.Serialize(m_users, m_jsonSerializerOptions));
        return Task.CompletedTask;
    }

    public Task<User?> TryEditUser(string email, Action<User> mapUser)
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

        return Task.FromResult(editedUser);
    }

    public Task<User?> TryGetUser(string email)
    {
        var result = m_users.FirstOrDefault(user => string.Equals(user.Email, email, StringComparison.InvariantCultureIgnoreCase));
        return Task.FromResult(result);
    }

    public async Task<User?> TryRemoveUser(string email)
    {
        var removedUser = await TryGetUser(email);

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

public class DbUserService : IUserService
{
    private readonly ILogger<DbUserService> m_logger;
    private readonly string m_dbConnectionString;
    
    public DbUserService(ILogger<DbUserService> logger, KeysService keysService)
    {
        m_logger = logger;
        m_dbConnectionString = keysService.DbConnectionString;
    }
    
    public async Task AddUser(User user)
    {
        string insertCommandRaw = "INSERT INTO Users (UserGuid, Email, PasswordHash, EmailEnabled) VALUES (@UserGuid, @Email, @PasswordHash, @EmailEnabled)";

        await using var sqlConnection = new MySqlConnection(m_dbConnectionString);
        await sqlConnection.OpenAsync();

        var transaction = await sqlConnection.BeginTransactionAsync();
        
        try
        {
            await using var insertCommand = new MySqlCommand(insertCommandRaw, sqlConnection);
            _ = insertCommand.Parameters.AddWithValue("@UserGuid", user.Guid);
            _ = insertCommand.Parameters.AddWithValue("@Email", user.Email);
            _ = insertCommand.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            _ = insertCommand.Parameters.AddWithValue("@EmailEnabled", user.IsEmailEnabled ? 1 : 0);
            
            m_logger.LogInformation($"Attempting to write \"{user.ToLogString()}\" to the db.");
            _ = await insertCommand.ExecuteNonQueryAsync();
            await transaction.CommitAsync();
            m_logger.LogInformation($"Successfully wrote \"{user.ToLogString()}\" to the db.");
        }
        catch (Exception exception)
        {
            m_logger.LogError($"Failed to write \"{user.ToLogString()}\" to the db, with the exception message \"{exception.Message}\". Attempting to rollback.");
            try
            {
                await transaction.RollbackAsync();
            }
            catch (Exception rollbackException)
            {
                m_logger.LogCritical($"Failed to rollback the insertion of \"{user.ToLogString()}\" with exception message \"{rollbackException.Message}\"");
            }
        }
    }

    public Task<User?> TryEditUser(string email, Action<User> mapUser)
    {
        throw new NotImplementedException();
    }

    public async Task<User?> TryGetUser(string email)
    {
        string insertCommandRaw = "SELECT * FROM Users WHERE Email = @Email";

        await using var sqlConnection = new MySqlConnection(m_dbConnectionString);
        await sqlConnection.OpenAsync();

        try
        {
            await using var insertCommand = new MySqlCommand(insertCommandRaw, sqlConnection);
            _ = insertCommand.Parameters.AddWithValue("@Email", email);
            
            m_logger.LogInformation($"Attempting to fetch user with email \"{email}\".");
            var dbDataReader  = await insertCommand.ExecuteReaderAsync();

            User? userToReturn = null;
            while (await dbDataReader.ReadAsync())
            {
                if (userToReturn is not null)
                {
                    m_logger.LogCritical($"Fetching \"{email}\" resulted in more than one user being returned.");
                    return null;
                }

                userToReturn = new User
                {
                    Guid = (Guid)dbDataReader["UserGuid"],
                    Email = (string)dbDataReader["Email"],
                    PasswordHash = (string)dbDataReader["PasswordHash"],
                    IsEmailEnabled = (bool)dbDataReader["EmailEnabled"]
                };
            }

            var logInfoMsg = userToReturn is null
                ? $"No user with the email \"{email}\" found."
                : $"Found the user with the email \"{email}\".";
            
            m_logger.LogInformation(logInfoMsg);
            return userToReturn;
        }
        catch (Exception exception)
        {
            m_logger.LogError($"Failed fetching the user with email \"{email}\", with exception message {exception.Message}.");
            return null;
        }
    }

    public Task<User?> TryRemoveUser(string email)
    {
        throw new NotImplementedException();
    }


    public void ClearAll()
    {
        throw new NotImplementedException();
    }
}