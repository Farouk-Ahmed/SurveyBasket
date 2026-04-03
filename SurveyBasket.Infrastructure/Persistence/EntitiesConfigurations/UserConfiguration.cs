using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SurveyBasket.Domain.Entities;

namespace SurveyBasket.Infrastructure.Persistence.EntitiesConfigurations;

public class UserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.Property(x => x.FirstName).HasMaxLength(20);
        builder.Property(x => x.LastName).HasMaxLength(20);
        builder.OwnsMany(x => x.RefreshTokens, b =>
        {
            b.ToTable("RefrechTokens");
            b.WithOwner().HasForeignKey("UserId");
        });
    }
}
