using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClassroomSystem.Data;
using ClassroomSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClassroomSystem.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int TotalUsers { get; set; }
        public int NewUsersThisWeek { get; set; }
        public int TotalClassrooms { get; set; }
        public int AvailableClassrooms { get; set; }
        public int TotalReservations { get; set; }
        public int PendingReservations { get; set; }
        public int ActiveTerms { get; set; }
        public int UpcomingTerms { get; set; }
        public List<ReservationViewModel> WeeklySchedule { get; set; }
        public List<FeedbackViewModel> RecentFeedback { get; set; }

        public class ReservationViewModel
        {
            public string ClassroomName { get; set; }
            public string InstructorName { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public ReservationStatus Status { get; set; }
        }

        public class FeedbackViewModel
        {
            public string InstructorName { get; set; }
            public string ClassroomName { get; set; }
            public int Rating { get; set; }
            public string Comment { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public async Task OnGetAsync()
        {
            var now = DateTime.Now;
            var weekStart = now.AddDays(-(int)now.DayOfWeek);
            var weekEnd = weekStart.AddDays(7);

            // Get user statistics
            TotalUsers = await _context.Users.CountAsync();
            NewUsersThisWeek = await _context.Users
                .Where(u => u.CreatedAt >= weekStart && u.CreatedAt < weekEnd)
                .CountAsync();

            // Get classroom statistics
            TotalClassrooms = await _context.Classrooms.CountAsync();
            AvailableClassrooms = await _context.Classrooms
                .Where(c => !c.Reservations.Any(r => 
                    r.StartTime <= now && r.EndTime >= now && r.Status == ReservationStatus.Approved))
                .CountAsync();

            // Get reservation statistics
            TotalReservations = await _context.Reservations.CountAsync();
            PendingReservations = await _context.Reservations
                .Where(r => r.Status == ReservationStatus.Pending)
                .CountAsync();

            // Get term statistics
            ActiveTerms = await _context.AcademicTerms
                .Where(t => t.StartDate <= now && t.EndDate >= now)
                .CountAsync();
            UpcomingTerms = await _context.AcademicTerms
                .Where(t => t.StartDate > now)
                .CountAsync();

            // Get weekly schedule
            WeeklySchedule = await _context.Reservations
                .Include(r => r.Classroom)
                .Include(r => r.User)
                .Where(r => r.StartTime >= weekStart && r.StartTime < weekEnd)
                .OrderBy(r => r.StartTime)
                .Select(r => new ReservationViewModel
                {
                    ClassroomName = r.Classroom.Name,
                    InstructorName = r.User.FullName,
                    StartTime = r.StartTime,
                    EndTime = r.EndTime,
                    Status = r.Status
                })
                .ToListAsync();

            // Get recent feedback
            RecentFeedback = await _context.Feedbacks
                .Include(f => f.User)
                .Include(f => f.Classroom)
                .OrderByDescending(f => f.CreatedAt)
                .Take(5)
                .Select(f => new FeedbackViewModel
                {
                    InstructorName = f.User.FullName,
                    ClassroomName = f.Classroom.Name,
                    Rating = f.Rating,
                    Comment = f.Comment,
                    CreatedAt = f.CreatedAt
                })
                .ToListAsync();
        }
    }
} 