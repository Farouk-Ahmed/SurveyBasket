using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SurveyBasket.Domain.Entities;

namespace SurveyBasket.Infrastructure.Persistence.EntitiesConfigurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Action).HasMaxLength(50).IsRequired();
        builder.Property(x => x.PerformedById).IsRequired();
        builder.Property(x => x.Details).HasMaxLength(2000);
        builder.HasQueryFilter(a => !a.Poll.IsDeleted);
        builder.HasOne(x => x.Poll).WithMany().HasForeignKey(x => x.PollId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.PerformedBy).WithMany().HasForeignKey(x => x.PerformedById).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.PollId);
        builder.HasIndex(x => x.PerformedOn);
        builder.HasOne(a => a.Account).WithMany(x => x.AuditLogs).HasForeignKey(a => a.AccountId).OnDelete(DeleteBehavior.Restrict);
    }
}
