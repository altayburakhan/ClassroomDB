using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClassroomSystem.Data;
using ClassroomSystem.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace ClassroomSystem.Pages.Instructor
{
    [Authorize(Roles = "Instructor")]
    public class IndexModel : BasePageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ApplicationDbContext context, ILogger<IndexModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public List<Reservation> UpcomingReservations { get; set; }
        public List<Reservation> PendingReservations { get; set; }
        public List<Feedback> RecentFeedback { get; set; }
        public int TotalReservations { get; set; }
        public int ApprovedReservations { get; set; }
        public int PendingReservationsCount { get; set; }
        public int RejectedReservations { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var userId = GetCurrentUserId();
                var currentDate = DateTime.Now;

                // Get upcoming reservations
                UpcomingReservations = await _context.Reservations
                    .Include(r => r.Classroom)
                    .Where(r => r.UserId == userId && r.StartTime > currentDate)
                    .OrderBy(r => r.StartTime)
                    .Take(5)
                    .ToListAsync();

                // Get pending reservations
                PendingReservations = await _context.Reservations
                    .Include(r => r.Classroom)
                    .Where(r => r.UserId == userId && r.Status == ReservationStatus.Pending)
                    .OrderBy(r => r.StartTime)
                    .Take(5)
                    .ToListAsync();

                // Get recent feedback
                RecentFeedback = await _context.Feedbacks
                    .Include(f => f.Classroom)
                    .Where(f => f.UserId == userId)
                    .OrderByDescending(f => f.CreatedAt)
                    .Take(5)
                    .ToListAsync();

                // Get reservation statistics
                var reservations = await _context.Reservations
                    .Include(r => r.Classroom)
                    .Where(r => r.UserId == userId)
                    .OrderByDescending(r => r.StartTime)
                    .ToListAsync();

                TotalReservations = reservations.Count();
                ApprovedReservations = reservations.Count(r => r.Status == ReservationStatus.Approved);
                PendingReservationsCount = reservations.Count(r => r.Status == ReservationStatus.Pending);
                RejectedReservations = reservations.Count(r => r.Status == ReservationStatus.Rejected);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading instructor dashboard");
                TempData["Error"] = "An error occurred while loading the dashboard.";
                return RedirectToPage("/Error");
            }
        }
    }
} 