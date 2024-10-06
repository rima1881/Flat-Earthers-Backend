using LandsatReflectance.Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LandsatReflectance.Backend.Utils.EFConfigs;

public class UserTargetNotificationTypeConfiguration : IEntityTypeConfiguration<UserTargetNotification>
{
    public void Configure(EntityTypeBuilder<UserTargetNotification> builder)
    {
        builder.ToTable("UserTargetNotifications");

        builder.HasKey(utn => new { utn.Path, utn.Row, utn.UserGuid, utn.TargetGuid, utn.PredictedAcquisitionDate });

        builder.Property(utn => utn.Path)
            .HasColumnName("ScenePath")
            .IsRequired();
        
        builder.Property(utn => utn.Row)
            .HasColumnName("SceneRow")
            .IsRequired();
        
        builder.Property(utn => utn.UserGuid)
            .HasColumnName("UserGuid")
            .ValueGeneratedNever()
            .IsRequired();
        
        builder.Property(utn => utn.TargetGuid)
            .HasColumnName("TargetGuid")
            .ValueGeneratedNever()
            .IsRequired();
        
        builder.Property(utn => utn.PredictedAcquisitionDate)
            .HasColumnName("PredictedAcquisitionDate")
            .IsRequired();
        
        builder.Property(utn => utn.HasBeenNotified)
            .HasColumnName("HasBeenNotified")
            .IsRequired();
    }
}