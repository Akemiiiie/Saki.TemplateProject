using System.ComponentModel.DataAnnotations;

namespace Saki.ModelTemplate.Bases
{
    public class OpenidUser
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; } // Store hashed password
        public string? Mobile { get; set; }
        public string? Remark { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
