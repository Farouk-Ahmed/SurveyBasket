using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using SurveyBasket.Entities;
using System.Security.Claims;

namespace SurveyBasket.NewFolder
{
    public class AppDBContext : IdentityDbContext<AppUser>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AppDBContext(DbContextOptions<AppDBContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public DbSet<Poll> Polls { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public DbSet<Attachment> Attachments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new PollConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
            modelBuilder.ApplyConfiguration(new AttachmentConfiguration());
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<AuditableEntity>();
            foreach (var entityEntry in entries)
            {
                var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                if (entityEntry.State == EntityState.Added)
                {
                    entityEntry.Property(x => x.CreatedById).CurrentValue = currentUserId;
                }
                else if (entityEntry.State == EntityState.Modified)
                {
                    // Check if this is a soft-delete operation
                    if (entityEntry.Property(x => x.IsDeleted).IsModified && entityEntry.Property(x => x.IsDeleted).CurrentValue)
                    {
                        entityEntry.Property(x => x.DeletedById).CurrentValue = currentUserId;
                        entityEntry.Property(x => x.DeletedOn).CurrentValue = DateTime.UtcNow;
                    }
                    else
                    {
                        entityEntry.Property(x => x.UpdatedById).CurrentValue = currentUserId;
                        entityEntry.Property(x => x.UpdatedOn).CurrentValue = DateTime.UtcNow;
                    }
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
