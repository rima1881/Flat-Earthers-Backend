using System.Globalization;
using LandsatReflectance.Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LandsatReflectance.Backend.Utils.EFConfigs;

public class TargetTypeConfiguration : IEntityTypeConfiguration<Target>
{
    public void Configure(EntityTypeBuilder<Target> builder)
    {
         builder.ToTable("Targets");

         builder.HasKey(target => target.Guid);

         builder
              .Property(target => target.Guid)
              .HasColumnName("TargetGuid")
              .IsRequired();

         builder
              .Property(target => target.Guid)
              .HasColumnName("TargetGuid")
              .IsRequired();

         builder
              .Property(target => target.Path)
              .HasColumnName("ScenePath")
              .IsRequired();

         builder
              .Property(target => target.Row)
              .HasColumnName("SceneRow")
              .IsRequired();

         builder
              .Property(target => target.Latitude)
              .HasColumnName("Latitude")
              .IsRequired();

         builder
              .Property(target => target.Longitude)
              .HasColumnName("Longitude")
              .IsRequired();

         builder
              .Property(target => target.MinCloudCover)
              .HasColumnName("MinCloudCover");

         builder
              .Property(target => target.MaxCloudCover)
              .HasColumnName("MaxCloudCover");

         builder
              .Property(target => target.NotificationOffset)
              .HasColumnName("NotificationOffset")
              .IsRequired()
              .HasConversion(
                   timespan => DateTime.MinValue.Add(timespan),
                   dateTime => dateTime - DateTime.MinValue);
    }
}