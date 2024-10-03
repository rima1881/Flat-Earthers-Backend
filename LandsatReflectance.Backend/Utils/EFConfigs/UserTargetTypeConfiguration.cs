using LandsatReflectance.Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LandsatReflectance.Backend.Utils.EFConfigs;

public class UserTargetTypeConfiguration : IEntityTypeConfiguration<UserTarget>
{
    public void Configure(EntityTypeBuilder<UserTarget> builder)
    {
         builder.ToTable("UsersTargets");

         builder.HasKey(ut => new { ut.UserGuid, ut.TargetGuid });

         builder
              .Property(ut => ut.UserGuid)
              .ValueGeneratedNever();

         builder
              .Property(ut => ut.TargetGuid)
              .ValueGeneratedNever();

         builder
              .HasOne<User>()
              .WithMany()
              .HasForeignKey(ut => ut.UserGuid);

         builder
              .HasOne<Target>()
              .WithMany()
              .HasForeignKey(ut => ut.TargetGuid);
    }
}