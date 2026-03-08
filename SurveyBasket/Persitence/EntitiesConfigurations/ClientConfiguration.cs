using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SurveyBasket.Entities;

namespace SurveyBasket.Persitence.EntitiesConfigurations
{
    public class ClientConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.FirstName).HasMaxLength(100).IsRequired();
            builder.Property(c => c.LastName).HasMaxLength(100).IsRequired();
            builder.Property(c => c.NationalId).HasMaxLength(20).IsRequired();
            builder.HasIndex(c => c.NationalId).IsUnique(); // National ID must be unique
            
            builder.Property(c => c.Email).HasMaxLength(256).IsRequired();
            builder.HasIndex(c => c.Email).IsUnique(); 

            builder.Property(c => c.MainMobile).HasMaxLength(20).IsRequired();
            builder.Property(c => c.AlternateMobile).HasMaxLength(20);

            builder.Property(c => c.MainAddress).HasMaxLength(500).IsRequired();
            builder.Property(c => c.AlternateAddress).HasMaxLength(500);

            builder.Property(c => c.ProfilePicturePath).HasMaxLength(500);

            builder.Property(c => c.Gender).HasMaxLength(10).IsRequired();

            // Configure One-to-One with AspNetUsers
            builder.HasOne(c => c.AppUser)
                .WithOne() // Assuming AppUser lacks a Client navigation property
                .HasForeignKey<Client>(c => c.AppUserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict); // Changed to restrict to avoid cycles

            // Configure Auditable properties to restrict delete to avoid multiple cascade paths targeting AspNetUsers
            builder.HasOne(c => c.CreatedBy)
                .WithMany()
                .HasForeignKey(c => c.CreatedById)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.UpdatedBy)
                .WithMany()
                .HasForeignKey(c => c.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.DeletedBy)
                .WithMany()
                .HasForeignKey(c => c.DeletedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
