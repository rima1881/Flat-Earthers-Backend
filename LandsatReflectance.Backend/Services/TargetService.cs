using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using LandsatReflectance.Backend.Models;
using LandsatReflectance.Backend.Utils.EFConfigs;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LandsatReflectance.Backend.Services;


public interface ITargetService
{
    public void AddTargets(IEnumerable<(User, Target)> targets);
    
    public IEnumerable<Target> TryEditTarget(
        Action<Target> mapTarget, 
        Expression<Func<Target, bool>> targetPredicate, 
        Expression<Func<Guid, bool>>? userGuidPredicate = null);
    
    public IEnumerable<Target> GetTargets(
        Expression<Func<Target, bool>> targetPredicate, 
        Expression<Func<Guid, bool>>? userGuidPredicate = null);
    
    public IEnumerable<Target> TryRemoveTarget(
        Expression<Func<Target, bool>> targetPredicate, 
        Expression<Func<Guid, bool>>? userGuidPredicate = null);
    
    [Obsolete("Use this method with care. Only meant for testing.")]
    public void ClearAll();
}



public class FileTargetService : ITargetService
{
    private static readonly string ExecutingAssemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
    
    private static readonly string JoinTableSaveFilePath = @$"{ExecutingAssemblyDirectory}\Data\usersTargetsJoinTable.json";
    private static readonly string TargetsSaveFilePath = @$"{ExecutingAssemblyDirectory}\Data\targets.json";
    
    private readonly ILogger<FileUserService> m_logger;
    private readonly JsonSerializerOptions m_jsonSerializerOptions;
    
    private List<JoinTableEntry> m_joinTableEntries = new();
    private List<Target> m_targets = new();


    public class JoinTableEntry
    {
        public Guid UserGuid { get; set; } = Guid.Empty;
        public Guid TargetGuid { get; set; } = Guid.Empty;
    }
    
    
    public FileTargetService(ILogger<FileUserService> logger, IOptions<JsonOptions> jsonOptions)
    {
        m_logger = logger;
        m_jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
        
        InitTargetsList();
    }


    public void AddTargets(IEnumerable<(User, Target)> targets)
    {
        List<(User, Target)> asList = targets.ToList();
        var newJoinTableEntries = asList.Select(pair => new JoinTableEntry { UserGuid = pair.Item1.Guid, TargetGuid = pair.Item2.Guid });
        var newTargets = asList.Select(pair => pair.Item2);
        
        m_joinTableEntries.AddRange(newJoinTableEntries);
        m_targets.AddRange(newTargets);
        
        File.WriteAllText(JoinTableSaveFilePath, JsonSerializer.Serialize(m_joinTableEntries, m_jsonSerializerOptions));
        File.WriteAllText(TargetsSaveFilePath, JsonSerializer.Serialize(m_targets, m_jsonSerializerOptions));
    }

    public IEnumerable<Target> TryEditTarget(
        Action<Target> mapTarget, 
        Expression<Func<Target, bool>> targetPredicate, 
        Expression<Func<Guid, bool>>? userGuidPredicate = null)
    {
        var targetsToEdit = GetTargets(targetPredicate, userGuidPredicate).ToList();
        targetsToEdit.ForEach(mapTarget);
        File.WriteAllText(TargetsSaveFilePath, JsonSerializer.Serialize(m_targets, m_jsonSerializerOptions));
        return targetsToEdit;
    }

    public IEnumerable<Target> GetTargets(
        Expression<Func<Target, bool>> targetPredicate, 
        Expression<Func<Guid, bool>>? userGuidPredicate = null)
    {
        Func<Guid, bool> compiledUserGuidPredicate = null!;
        if (userGuidPredicate is not null)
        {
            compiledUserGuidPredicate = userGuidPredicate.Compile();
        }
        
        List<Target> shortenedTargetsList = userGuidPredicate is not null
            ? m_joinTableEntries
                .Where(joinTableEntry => compiledUserGuidPredicate(joinTableEntry.UserGuid))
                .Select(joinTableEntry => m_targets.Single(target => target.Guid == joinTableEntry.TargetGuid))
                .ToList()
            : m_targets;

        return shortenedTargetsList.Where(targetPredicate.Compile());
    }

    public IEnumerable<Target> TryRemoveTarget(
        Expression<Func<Target, bool>> targetPredicate, 
        Expression<Func<Guid, bool>>? userGuidPredicate = null)
    {
        var targetsToDelete = GetTargets(targetPredicate, userGuidPredicate).ToList();

        m_targets.RemoveAll(target => targetsToDelete.Contains(target));

        foreach (var targetToDelete in targetsToDelete)
        {
            _ = m_joinTableEntries.RemoveAll(joinTableEntry => joinTableEntry.TargetGuid == targetToDelete.Guid);
        }
        
        File.WriteAllText(JoinTableSaveFilePath, JsonSerializer.Serialize(m_joinTableEntries, m_jsonSerializerOptions));
        File.WriteAllText(TargetsSaveFilePath, JsonSerializer.Serialize(m_targets, m_jsonSerializerOptions));
        return targetsToDelete;
    }


    public void ClearAll()
    {
        m_targets.Clear();
        
        File.WriteAllText(JoinTableSaveFilePath, JsonSerializer.Serialize(m_joinTableEntries, m_jsonSerializerOptions));
        File.WriteAllText(TargetsSaveFilePath, JsonSerializer.Serialize(m_targets, m_jsonSerializerOptions));
    }
    
    
    private void InitTargetsList()
    {
        if (!File.Exists(JoinTableSaveFilePath))
        {
            File.WriteAllText(JoinTableSaveFilePath, "[]");
        }
        m_joinTableEntries = JsonSerializer.Deserialize<List<JoinTableEntry>>(File.ReadAllText(JoinTableSaveFilePath), m_jsonSerializerOptions) ?? [];
        
        
        if (!File.Exists(TargetsSaveFilePath))
        {
            File.WriteAllText(TargetsSaveFilePath, "[]");
        }
        m_targets = JsonSerializer.Deserialize<List<Target>>(File.ReadAllText(TargetsSaveFilePath), m_jsonSerializerOptions) ?? [];
    }
}

public class DbTargetService : ITargetService
{
    public class TargetDbContext : DbContext
    {
        public DbSet<Target> Targets { get; set; }
        public DbSet<UserTarget> UserTargets { get; set; }

        public TargetDbContext(DbContextOptions<TargetDbContext> options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            new UserTypeConfiguration().Configure(modelBuilder.Entity<User>());
            new TargetTypeConfiguration().Configure(modelBuilder.Entity<Target>());
            new UserTargetTypeConfiguration().Configure(modelBuilder.Entity<UserTarget>());
        }
    }

    
    private readonly ILogger<DbTargetService> m_logger;
    private readonly TargetDbContext m_targetDbContext;

    
    public DbTargetService(ILogger<DbTargetService> logger, TargetDbContext targetDbContext)
    {
        m_logger = logger;
        m_targetDbContext = targetDbContext;
    }
    
    
    public void AddTargets(IEnumerable<(User, Target)> targets)
    {
        var targetsArr = targets.ToArray();
        using var transaction = m_targetDbContext.Database.BeginTransaction();

        try
        {
            m_logger.LogInformation("Attempting to add targets to the db.");
            
            var targetsToAdd = targetsArr.Select(tuple => tuple.Item2).ToList();
            var joinTableEntriesToAdd = targetsArr.Select(tuple => new UserTarget(tuple.Item1.Guid, tuple.Item2.Guid));

            m_targetDbContext.Targets.AddRange(targetsToAdd);
            m_targetDbContext.UserTargets.AddRange(joinTableEntriesToAdd);
            
            _ = m_targetDbContext.SaveChanges();
            transaction.Commit();
            m_logger.LogInformation("Successfully added targets to the db.");
        }
        catch (Exception exception)
        {
            try
            {
                m_logger.LogError($"Failed to add targets (check Debug for more info), with error message: \"{exception.Message}\". Rolling back transaction.");
                var targetsAsStr = targetsArr
                    .Select((tuple, i) => $"Pair #{i}: {tuple.Item1.ToLogString()}, {tuple.Item2.ToLogString()}")
                    .Aggregate("", (str, acc) => $"{acc}\n{str}\n")
                    .Trim('\n');
                m_logger.LogDebug($"Failed to add the following entries:\n{targetsAsStr}");
                transaction.Rollback();
            }
            catch (Exception rollbackException)
            {
                m_logger.LogCritical($"Failed to rollback add targets transaction with error message \"{rollbackException.Message}\"");
            }
        }
    }

    public IEnumerable<Target> TryEditTarget(Action<Target> mapTarget, Expression<Func<Target, bool>> targetPredicate, Expression<Func<Guid, bool>>? userGuidPredicate = null)
    {
        Expression<Func<Guid, bool>> nonNullGuidPredicate = userGuidPredicate ?? (_ => true);
        Expression<Func<UserTarget, Guid>> guidPropertySelector = t => t.UserGuid;
        Expression<Func<UserTarget, bool>> userTargetPredicate = CombinePredicate(nonNullGuidPredicate, guidPropertySelector);
        
        using var transaction = m_targetDbContext.Database.BeginTransaction();

        try
        {
            m_logger.LogInformation($"Attempting edit targets, given: targetPredicate: \"{targetPredicate}\", userGuidPredicate: \"{nonNullGuidPredicate}\".");

            var targets =
                m_targetDbContext.UserTargets
                    .Where(userTargetPredicate)
                    .Join(m_targetDbContext.Targets, userTarget => userTarget.TargetGuid, target => target.Guid, (userTarget, target) => target)
                    .Where(targetPredicate)
                    .ToList();

            var oldGuids = targets.Select(target => target.Guid);
            targets.ForEach(mapTarget);

            if (oldGuids.Zip(targets).Any(tuple => tuple.First != tuple.Second.Guid))
            {
                m_logger.LogError("Attempted to modify a target's GUID, this is not permitted.");
                return [];
            }
            
            _ = m_targetDbContext.SaveChanges();
            transaction.Commit();
            m_logger.LogInformation($"Successfully edited {targets.Count} targets.");

            targets.ForEach(target => m_targetDbContext.Entry(target).State = EntityState.Detached);
            return targets;
        }
        catch (Exception exception)
        {
            try
            {
                m_logger.LogError($"Failed to edit targets given: targetPredicate: \"{targetPredicate}\", userGuidPredicate: \"{nonNullGuidPredicate}\", " +
                                  $"with error message: \"{exception.Message}\". Rolling back transaction.");
                transaction.Rollback();
            }
            catch (Exception rollbackException)
            {
                m_logger.LogCritical($"Failed to rollback add targets transaction with error message \"{rollbackException.Message}\"");
            }
        }

        return [];
    }

    public IEnumerable<Target> GetTargets(Expression<Func<Target, bool>> targetPredicate, Expression<Func<Guid, bool>>? userGuidPredicate = null)
    {
        Expression<Func<Guid, bool>> nonNullGuidPredicate = userGuidPredicate ?? (_ => true);
        Expression<Func<UserTarget, Guid>> guidPropertySelector = t => t.UserGuid;
        Expression<Func<UserTarget, bool>> userTargetPredicate = CombinePredicate(nonNullGuidPredicate, guidPropertySelector);
        
        using var transaction = m_targetDbContext.Database.BeginTransaction();
        
        m_logger.LogInformation($"Attempting query targets with: targetPredicate: \"{targetPredicate}\", userGuidPredicate: \"{nonNullGuidPredicate}\".");

        var targets =
            m_targetDbContext.UserTargets
                .AsNoTracking()
                .Where(userTargetPredicate)
                .Join(m_targetDbContext.Targets, userTarget => userTarget.TargetGuid, target => target.Guid, (userTarget, target) => target)
                .Where(targetPredicate)
                .ToList();
        
        m_logger.LogInformation($"Found {targets.Count} targets matching: targetPredicate: \"{targetPredicate}\", userGuidPredicate: \"{nonNullGuidPredicate}\".");
        return targets;
    }

    public IEnumerable<Target> TryRemoveTarget(Expression<Func<Target, bool>> targetPredicate, Expression<Func<Guid, bool>>? userGuidPredicate = null)
    {
        Expression<Func<Guid, bool>> nonNullGuidPredicate = userGuidPredicate ?? (_ => true);
        Expression<Func<UserTarget, Guid>> guidPropertySelector = t => t.UserGuid;
        Expression<Func<UserTarget, bool>> userTargetPredicate = CombinePredicate(nonNullGuidPredicate, guidPropertySelector);
        
        using var transaction = m_targetDbContext.Database.BeginTransaction();

        try
        {
            m_logger.LogInformation($"Attempting delete targets, given: targetPredicate: \"{targetPredicate}\", userGuidPredicate: \"{nonNullGuidPredicate}\".");

            var targetsToRemove =
                m_targetDbContext.UserTargets
                    .Where(userTargetPredicate)
                    .Join(m_targetDbContext.Targets, userTarget => userTarget.TargetGuid, target => target.Guid, (userTarget, target) => target)
                    .Where(targetPredicate)
                    .ToList();

            var guidsToRemove = new HashSet<Guid>(targetsToRemove.Select(target => target.Guid));
            var userTargetsToRemove =
                m_targetDbContext.UserTargets
                    .Where(userTarget => guidsToRemove.Contains(userTarget.TargetGuid))
                    .ToList();
            
            m_targetDbContext.Targets.RemoveRange(targetsToRemove);
            m_targetDbContext.UserTargets.RemoveRange(userTargetsToRemove);
            
            _ = m_targetDbContext.SaveChanges();
            transaction.Commit();
            m_logger.LogInformation($"Successfully deleted {targetsToRemove.Count} targets.");

            targetsToRemove.ForEach(target => m_targetDbContext.Entry(target).State = EntityState.Detached);
            return targetsToRemove;
        }
        catch (Exception exception)
        {
            try
            {
                m_logger.LogError($"Failed to delete targets given: targetPredicate: \"{targetPredicate}\", userGuidPredicate: \"{nonNullGuidPredicate}\", " +
                                  $"with error message: \"{exception.Message}\". Rolling back transaction.");
                transaction.Rollback();
            }
            catch (Exception rollbackException)
            {
                m_logger.LogCritical($"Failed to rollback add targets transaction with error message \"{rollbackException.Message}\"");
            }
        }

        return [];
    }

    public void ClearAll()
    {
        m_targetDbContext.Database.ExecuteSqlRaw(
            """
            -- noinspection SqlWithoutWhereForFile
            SET FOREIGN_KEY_CHECKS = 0;
            
            DELETE FROM UsersTargets;
            DELETE FROM Targets;
            
            SET FOREIGN_KEY_CHECKS = 1;
            """);
    }
    
    
    // ty chatgpt
    private static Expression<Func<T, bool>> CombinePredicate<T>(
        Expression<Func<Guid, bool>> guidPredicate,
        Expression<Func<T, Guid>> guidPropertySelector)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var propertyAccess = Expression.Invoke(guidPropertySelector, parameter);
        var predicateBody = Expression.Invoke(guidPredicate, propertyAccess);
        return Expression.Lambda<Func<T, bool>>(predicateBody, parameter);
    }
}