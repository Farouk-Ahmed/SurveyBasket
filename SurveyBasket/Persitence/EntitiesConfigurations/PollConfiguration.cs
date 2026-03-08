using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SurveyBasket.Persitence.EntitiesConfigurations
{
    public class PollConfiguration:IEntityTypeConfiguration<Poll>
    {
        public void Configure(EntityTypeBuilder<Poll> builder)
        {
            builder.HasIndex(x=>x.Title).IsUnique(); // Ensure Title is unique
            builder.Property(x=>x.Title).HasMaxLength(100); // Set max length for Title
            builder.Property(x=>x.Summray).HasMaxLength(1000); // Set max length for Summray
            builder.Property(x => x.DeletionReason).HasMaxLength(500);
            builder.HasQueryFilter(x => !x.IsDeleted);

            builder.HasOne(p => p.Client)
                .WithMany(c => c.Polls)
                .HasForeignKey(p => p.ClientId)
                .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Restrict);
        }
    }
}
