using LandsatReflectance.Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LandsatReflectance.Backend.Utils.EFConfigs;

public class UserTypeConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Guid);

        builder.Property(user => user.Guid)
            .HasColumnName("UserGuid")
            .IsRequired();

        builder.Property(user => user.Email)
            .HasColumnName("Email")
            .IsRequired();

        builder.Property(user => user.PasswordHash)
            .HasColumnName("PasswordHash")
            .IsRequired();

        builder.Property(user => user.IsEmailEnabled)
            .HasColumnName("EmailEnabled")
            .IsRequired();
    }
}