using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClassroomSystem.Models
{
    public class UserPreference
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public string Theme { get; set; } = "light";

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
    }
} 