using LandsatReflectance.Backend.Models;
using LandsatReflectance.Backend.Utils.EFConfigs;
using Microsoft.EntityFrameworkCore;

namespace LandsatReflectance.Backend.Services;

public class DbUserTargetNotificationService
{
    public class UserTargetNotificationDbContext : DbContext
    {
        public DbSet<UserTargetNotification> UserTargetNotifications { get; set; }
        
        public UserTargetNotificationDbContext(DbContextOptions<UserTargetNotificationDbContext> options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            new UserTargetNotificationTypeConfiguration().Configure(modelBuilder.Entity<UserTargetNotification>());
        }
    }


    private ILogger<DbUserTargetNotificationService> m_logger;
    private UserTargetNotificationDbContext m_userTargetNotificationDbContext;

    public DbUserTargetNotificationService(ILogger<DbUserTargetNotificationService> logger,
        UserTargetNotificationDbContext userTargetNotificationDbContext)
    {
        m_logger = logger;
        m_userTargetNotificationDbContext = userTargetNotificationDbContext;
    }


    /// Tries getting the user target notification record matching the info.
    /// If it doesn't exist, we try to create and return it.
    /// A null value indicates no match AND an unsuccessful creation.
    public UserTargetNotification? GetNotificationRecord(int path, int row, Guid userGuid, Guid targetGuid, DateTime dateTime)
    {
        var matchingRecord = m_userTargetNotificationDbContext.UserTargetNotifications
            .AsTracking()
            .Where(utn => utn.Path == path && utn.Row == row && utn.UserGuid == userGuid && utn.TargetGuid == targetGuid && utn.PredictedAcquisitionDate == dateTime)
            .ToList();

        if (matchingRecord.Count > 1)
        {
            m_logger.LogCritical($"Found multiple records with the key ({path}, {row}, {userGuid}, {targetGuid}, {dateTime})");
        }

        return matchingRecord.FirstOrDefault() ?? AddNotificationRecord(path, row, userGuid, targetGuid, dateTime);
    }

    public UserTargetNotification? AddNotificationRecord(int path, int row, Guid userGuid, Guid targetGuid,
        DateTime dateTime, bool hasBeenNotified = false)
    {
        var userTargetNotification = new UserTargetNotification
        {
            Path = path,
            Row = row,
            UserGuid = userGuid,
            TargetGuid = targetGuid,
            PredictedAcquisitionDate = dateTime,
            HasBeenNotified = hasBeenNotified,
        };
        string userTargetNotificationStr = $"({path}, {row}, {userGuid}, {targetGuid}, {dateTime}, {hasBeenNotified})";

        using var transaction = m_userTargetNotificationDbContext.Database.BeginTransaction();
        
        try
        {
            m_logger.LogInformation($"Attempting to add a new user target notification \"{userTargetNotificationStr}\"");

            m_userTargetNotificationDbContext.UserTargetNotifications.Add(userTargetNotification);
            _ = m_userTargetNotificationDbContext.SaveChanges();
            transaction.Commit();
            
            m_logger.LogInformation($"Successfully added user target notification \"{userTargetNotificationStr}\"");

            return userTargetNotification;
        }
        catch (Exception exception)
        {
            m_logger.LogError($"Failed to add user target notification \"{userTargetNotificationStr}\" with error message:" +
                              $" \"{exception.Message}\". Attempting to rollback transaction.");
            try
            {
                transaction.Rollback();
            }
            catch (Exception rollbackException)
            {
                m_logger.LogCritical($"Failed to rollback add user target notification with error message: \"{rollbackException.Message}\"");
            }
        }

        return null;
    }

    public bool SetNotificationStatus(int path, int row, Guid userGuid, Guid targetGuid,
        DateTime dateTime, bool newNotificationStatus)
    {
        var notificationRecord = GetNotificationRecord(path, row, userGuid, targetGuid, dateTime);

        // If the record doesn't exist, 'GetNotificationRecord' tries to create it. 
        // A null value indicates a failed fetch AND creation.
        if (notificationRecord is null)
        {
            return false;
        }

        string userTargetNotificationStr = $"({path}, {row}, {userGuid}, {targetGuid}, {dateTime})";
        using var transaction = m_userTargetNotificationDbContext.Database.BeginTransaction();

        try
        {
            m_logger.LogInformation($"Attempting to change the notification status of record \"{userTargetNotificationStr}\" " +
                                    $"from {notificationRecord.HasBeenNotified} to {newNotificationStatus}");

            notificationRecord.HasBeenNotified = newNotificationStatus;
            _ = m_userTargetNotificationDbContext.SaveChanges();
            transaction.Commit();
            
            m_logger.LogInformation($"Successfully changed the notification status of record \"{userTargetNotificationStr}\" " +
                                    $"from {notificationRecord.HasBeenNotified} to {newNotificationStatus}");

            return true;
        }
        catch (Exception exception)
        {
            m_logger.LogError($"FAILED to change the notification status of record \"{userTargetNotificationStr}\" " +
                                    $"from {notificationRecord.HasBeenNotified} to {newNotificationStatus}, with error message: \"{exception.Message}\"");
            try
            {
                transaction.Rollback();
            }
            catch (Exception rollbackException)
            {
                m_logger.LogCritical($"Failed to rollback user target notification status with error message: \"{rollbackException.Message}\"");
            }

            return false;
        }
    }
}