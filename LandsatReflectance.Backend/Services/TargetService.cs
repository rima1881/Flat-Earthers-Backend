using System.Reflection;
using System.Text.Json;
using LandsatReflectance.Backend.Models;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace LandsatReflectance.Backend.Services;


public interface ITargetService
{
    public void AddTargets(IEnumerable<(User, Target)> targets);
    
    public Target? TryEditTarget(
        Action<Target> mapTarget, 
        Predicate<Target> targetPredicate, 
        Predicate<Guid>? userGuidPredicate = null);
    
    public IEnumerable<Target> GetTargets(
        Predicate<Target> targetPredicate, 
        Predicate<Guid>? userGuidPredicate = null);
    
    public Target? TryRemoveTarget(
        Predicate<Target> targetPredicate, 
        Predicate<Guid>? userGuidPredicate = null);
    
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

    public Target? TryEditTarget(
        Action<Target> mapTarget, 
        Predicate<Target> targetPredicate, 
        Predicate<Guid>? userGuidPredicate = null)
    {
        var targetToEdit = GetTargets(targetPredicate, userGuidPredicate).FirstOrDefault();

        if (targetToEdit is null)
        {
            return null;
        }

        mapTarget(targetToEdit);
        File.WriteAllText(TargetsSaveFilePath, JsonSerializer.Serialize(m_targets, m_jsonSerializerOptions));
        return targetToEdit;
    }

    public IEnumerable<Target> GetTargets(
        Predicate<Target> targetPredicate, 
        Predicate<Guid>? userGuidPredicate = null)
    {
        List<Target> shortenedTargetsList = userGuidPredicate is not null
            ? m_joinTableEntries
                .Where(joinTableEntry => userGuidPredicate(joinTableEntry.UserGuid))
                .Select(joinTableEntry => m_targets.Single(target => target.Guid == joinTableEntry.TargetGuid))
                .ToList()
            : m_targets;

        return shortenedTargetsList.Where(target => targetPredicate(target));
    }

    public Target? TryRemoveTarget(
        Predicate<Target> targetPredicate, 
        Predicate<Guid>? userGuidPredicate = null)
    {
        var targetToDelete = GetTargets(targetPredicate, userGuidPredicate).FirstOrDefault();

        if (targetToDelete is null)
        {
            return null;
        }

        if (!m_targets.Remove(targetToDelete))
        {
            return null;
        }
        
        _ = m_joinTableEntries.RemoveAll(joinTableEntry => joinTableEntry.TargetGuid == targetToDelete.Guid);
        
        File.WriteAllText(JoinTableSaveFilePath, JsonSerializer.Serialize(m_joinTableEntries, m_jsonSerializerOptions));
        File.WriteAllText(TargetsSaveFilePath, JsonSerializer.Serialize(m_targets, m_jsonSerializerOptions));
        return targetToDelete;
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