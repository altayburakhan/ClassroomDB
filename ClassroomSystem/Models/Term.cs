using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClassroomSystem.Models
{
    public class Term
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public string Description { get; set; }

        // Navigation properties
        public ICollection<Reservation> Reservations { get; set; }
    }
} 