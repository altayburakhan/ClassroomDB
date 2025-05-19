using System;
using System.ComponentModel.DataAnnotations;

namespace ClassroomSystem.Models
{
    public class SystemLog
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; }
        
        [Required]
        public string Action { get; set; }
        
        [Required]
        public string Message { get; set; }
        
        public string Details { get; set; }
        
        public bool IsSuccess { get; set; }
        
        [Required]
        public string Type { get; set; } // UserAction, Error, Security, System
        
        public DateTime Timestamp { get; set; }
    }
} 