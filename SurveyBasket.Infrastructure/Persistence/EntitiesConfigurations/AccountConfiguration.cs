using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SurveyBasket.Domain.Entities;

namespace SurveyBasket.Infrastructure.Persistence.EntitiesConfigurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.AppUserId).HasMaxLength(450).IsRequired();
        builder.HasIndex(a => a.AppUserId).IsUnique();
        builder.Property(a => a.Role).HasMaxLength(50).IsRequired();
        builder.Property(a => a.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(a => a.LastName).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Email).HasMaxLength(256).IsRequired();
        builder.Property(a => a.UserName).HasMaxLength(256).IsRequired();
        builder.Property(a => a.ProfilePicturePath).HasMaxLength(500);
        builder.Property(a => a.NationalId).HasMaxLength(20);
        builder.Property(a => a.MainAddress).HasMaxLength(500);
        builder.Property(a => a.AlternateAddress).HasMaxLength(500);
        builder.Property(a => a.MainMobile).HasMaxLength(20);
        builder.Property(a => a.AlternateMobile).HasMaxLength(20);
        builder.Property(a => a.Gender).HasMaxLength(10);
        builder.HasOne(a => a.AppUser)
            .WithOne()
            .HasForeignKey<Account>(a => a.AppUserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(a => a.Role);
    }
}
