using System;
using System.ComponentModel.DataAnnotations;

namespace ClassroomSystem.Models
{
    public class Logs
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string Action { get; set; }

        [Required]
        public string Details { get; set; }

        [Required]
        public string Status { get; set; } // "Success" or "Error"

        public string ErrorMessage { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string IpAddress { get; set; }
    }
} 