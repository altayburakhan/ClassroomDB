using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClassroomSystem.Models
{
    public class Classroom
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string RoomNumber { get; set; }

        public string Building { get; set; }

        public int Floor { get; set; }

        [Required]
        public int Capacity { get; set; }

        public string Features { get; set; }

        public bool IsAvailable { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public ICollection<Reservation> Reservations { get; set; }
        public ICollection<Feedback> Feedbacks { get; set; }
    }
} 