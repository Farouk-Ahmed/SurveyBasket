using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SurveyBasket.Entities;

namespace SurveyBasket.Persitence.EntitiesConfigurations
{
    public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
    {
        public void Configure(EntityTypeBuilder<Attachment> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FileName)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.StoredPath)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.ContentType)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.UploadedById)
                .IsRequired();

            builder.HasOne(x => x.UploadedBy)
                .WithMany()
                .HasForeignKey(x => x.UploadedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Poll)
                .WithMany(p => p.Attachments)
                .HasForeignKey(x => x.PollId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(x => x.UploadedById);
            builder.HasIndex(x => x.UploadedOn);
            builder.HasIndex(x => x.PollId);

            builder.HasOne(a => a.Client)
                .WithMany(c => c.Attachments)
                .HasForeignKey(a => a.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
