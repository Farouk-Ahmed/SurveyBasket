using SurveyBasket.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SurveyBasket.Entities
{
    public class Client : AuditableEntity
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = string.Empty;
        
        public string LastName { get; set; } = string.Empty;

        public string NationalId { get; set; } = string.Empty;

        public string MainAddress { get; set; } = string.Empty;

        public string? AlternateAddress { get; set; }

        public string MainMobile { get; set; } = string.Empty;

        public string? AlternateMobile { get; set; }

        public string? ProfilePicturePath { get; set; }

        public string Email { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        public string Gender { get; set; } = string.Empty;

        // One-to-One with AspNetUsers (Identity)
        public string AppUserId { get; set; } = string.Empty;
        
        public AppUser AppUser { get; set; } = default!;

        // Navigation relationships to other tables
        public ICollection<Poll> Polls { get; set; } = new List<Poll>();
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
