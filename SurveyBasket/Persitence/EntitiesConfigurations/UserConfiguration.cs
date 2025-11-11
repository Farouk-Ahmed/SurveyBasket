using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SurveyBasket.Entities;

namespace SurveyBasket.Persitence.EntitiesConfigurations
{
    public class UserConfiguration:IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.Property(x=>x.FirstName).HasMaxLength(20);
            builder.Property(x=>x.LastName).HasMaxLength(20); // Already string, just ensure config is correct
            builder.OwnsMany(x=>x.RefrechTokens)
                .ToTable("RefrechTokens")
                .WithOwner()
                .HasForeignKey("UserId");

        }
    }
}
