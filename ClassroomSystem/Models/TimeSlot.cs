using System;
using System.ComponentModel.DataAnnotations;

namespace ClassroomSystem.Models
{
    public class TimeSlot
    {
        public int Id { get; set; }
        
        [Required]
        public TimeSpan StartTime { get; set; }
        
        [Required]
        public TimeSpan EndTime { get; set; }
        
        public DayOfWeek DayOfWeek { get; set; }
        
        public bool IsAvailable { get; set; }
        
        public int? ReservationId { get; set; }
        public Reservation Reservation { get; set; }
        
        public int ClassroomId { get; set; }
        public Classroom Classroom { get; set; }
    }
} 