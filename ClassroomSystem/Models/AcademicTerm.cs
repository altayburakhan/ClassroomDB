using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ClassroomSystem.Models
{
    public class AcademicTerm
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool IsActive => DateTime.Now >= StartDate && DateTime.Now <= EndDate;

        public double GetProgressPercentage()
        {
            var totalDays = (EndDate - StartDate).TotalDays;
            var elapsedDays = (DateTime.Now - StartDate).TotalDays;
            return Math.Min(100, Math.Max(0, (elapsedDays / totalDays) * 100));
        }

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
} 