using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SurveyBasket.Domain.Entities;

namespace SurveyBasket.Infrastructure.Persistence.EntitiesConfigurations;

public class PollConfiguration : IEntityTypeConfiguration<Poll>
{
    public void Configure(EntityTypeBuilder<Poll> builder)
    {
        builder.HasIndex(x => x.Title).IsUnique();
        builder.Property(x => x.Title).HasMaxLength(100);
        builder.Property(x => x.Summray).HasMaxLength(1000);
        builder.Property(x => x.DeletionReason).HasMaxLength(500);
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasOne(p => p.Account)
            .WithMany(a => a.Polls)
            .HasForeignKey(p => p.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
