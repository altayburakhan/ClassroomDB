using System;

namespace ClassroomSystem.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; }
        public string Type { get; set; } // "ReservationApproval", "ReservationRejection", "Holiday"
        public int? ReservationId { get; set; }
    }
} 