using System.Reflection;
using System.Text.Json;
using LandsatReflectance.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

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
    public class UserDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        
        
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        { }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("Users");

            modelBuilder.Entity<User>().HasKey(user => user.Guid);
                
            modelBuilder.Entity<User>()
                .Property(user => user.Guid)
                .HasColumnName("UserGuid")
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(user => user.Email)
                .HasColumnName("Email")
                .IsRequired();
            
            modelBuilder.Entity<User>()
                .Property(user => user.PasswordHash)
                .HasColumnName("PasswordHash")
                .IsRequired();
            
            modelBuilder.Entity<User>()
                .Property(user => user.IsEmailEnabled)
                .HasColumnName("EmailEnabled")
                .IsRequired();
        }
    }
    
    private readonly ILogger<DbUserService> m_logger;
    private readonly UserDbContext m_userDbContext;
    
    public DbUserService(ILogger<DbUserService> logger, UserDbContext userDbContext)
    {
        m_logger = logger;
        m_userDbContext = userDbContext;
    }
    
    public Task AddUser(User user)
    {
        using var transaction = m_userDbContext.Database.BeginTransaction();

        try
        {
            m_logger.LogInformation($"Attempting to add \"{user.ToLogString()}\" to the database.");
            _ = m_userDbContext.Users.Add(user);
            _ = m_userDbContext.SaveChanges();

            transaction.Commit();
        }
        catch (Exception exception)
        {
            m_logger.LogError($"Failed to add \"{user.ToLogString()}\", with error message: \"{exception.Message}\". Rolling back transaction.");
            try
            {
                transaction.Rollback();
            }
            catch (Exception rollbackException)
            {
                m_logger.LogCritical($"Failed to rollback transaction with error message \"{rollbackException.Message}\"");
            }
        }
        
        return Task.CompletedTask;
    }

    public Task<User?> TryEditUser(string email, Action<User> mapUser)
    {
        using var transaction = m_userDbContext.Database.BeginTransaction();

        try
        {
            var users = m_userDbContext.Users.Where(user => user.Email == email);

            if (users.Count() > 1)
            {
                m_logger.LogCritical($"There were multiple users with email \"{email}\" were found.");
                return Task.FromResult<User?>(null);
            }

            var user = users.FirstOrDefault();
            
            if (user is null)
            {
                return Task.FromResult<User?>(null);
            }
            
            m_logger.LogInformation($"Attempting to modify \"{user.ToLogString()}\".");

            var oldUserGuid = user.Guid;
            mapUser(user);

            if (oldUserGuid != user.Guid)
            {
                m_logger.LogError("Attempted to modify the user's GUID, this is not permitted.");
                return Task.FromResult<User?>(null);
            }
            
            _ = m_userDbContext.SaveChanges();

            transaction.Commit();

            return Task.FromResult<User?>(user);
        }
        catch (Exception exception)
        {
            m_logger.LogError($"Failed to modify the user with email \"{email}\", with error message: \"{exception.Message}\". Rolling back transaction.");
            try
            {
                transaction.Rollback();
            }
            catch (Exception rollbackException)
            {
                m_logger.LogCritical($"Failed to rollback transaction with error message \"{rollbackException.Message}\"");
            }
        }
        
        return Task.FromResult<User?>(null);
    }

    public Task<User?> TryGetUser(string email)
    {
        var users = m_userDbContext.Users
            .AsNoTracking()
            .Where(user => user.Email == email);

        if (users.Count() > 1)
        {
            m_logger.LogCritical($"There were multiple users with email \"{email}\" were found.");
        }

        return Task.FromResult(users.FirstOrDefault());
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