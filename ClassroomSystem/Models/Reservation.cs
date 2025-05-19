using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClassroomSystem.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        [Required]
        public int ClassroomId { get; set; }
        
        [ForeignKey("ClassroomId")]
        public Classroom Classroom { get; set; }

        [Required]
        public int TermId { get; set; }
        
        [ForeignKey("TermId")]
        public Term Term { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [Required]
        public string Purpose { get; set; }

        [Required]
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        public bool IsRecurring { get; set; }
        public string RecurrencePattern { get; set; } // Daily, Weekly, etc.
        public DateTime? RecurrenceEndDate { get; set; }

        public string Notes { get; set; }

        public string RejectionReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [NotMapped]
        public string FormattedTimeSlot => $"{StartTime:HH:mm} - {EndTime:HH:mm}";

        // Convenience property for just the date
        [NotMapped]
        public DateTime Date => StartTime.Date;

        [NotMapped]
        public TimeSpan Duration => EndTime - StartTime;

        [NotMapped]
        public DayOfWeek DayOfWeek => StartTime.DayOfWeek;

        [NotMapped]
        public bool IsValid => 
            StartTime < EndTime && 
            Duration.TotalHours <= 8 && 
            StartTime.TimeOfDay >= TimeSpan.FromHours(8) && 
            EndTime.TimeOfDay <= TimeSpan.FromHours(20);
    }
} 