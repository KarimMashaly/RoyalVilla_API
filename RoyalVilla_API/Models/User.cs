using System.ComponentModel.DataAnnotations;

namespace RoyalVilla_API.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public required string Name { get; set; } = null!;

        [Required]
        public required string Password { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public required string Role { get; set; } = "Customer";

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
