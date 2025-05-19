using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClassroomSystem.Data;
using ClassroomSystem.Models;
using ClassroomSystem.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ClassroomSystem.Pages.Instructor
{
    [Authorize(Roles = "Instructor")]
    public class ReservationsModel : BasePageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IHolidayService _holidayService;
        private readonly ILogger<ReservationsModel> _logger;

        public ReservationsModel(
            ApplicationDbContext context,
            IEmailService emailService,
            IHolidayService holidayService,
            ILogger<ReservationsModel> logger)
        {
            _context = context;
            _emailService = emailService;
            _holidayService = holidayService;
            _logger = logger;
        }

        public List<Reservation> Reservations { get; set; }
        public List<Classroom> Classrooms { get; set; }
        public List<AcademicTerm> Terms { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var userId = GetCurrentUserId();
                var currentDate = DateTime.Now;

                // Get all reservations for the instructor
                Reservations = await _context.Reservations
                    .Include(r => r.Classroom)
                    .Where(r => r.UserId == userId)
                    .OrderByDescending(r => r.StartTime)
                    .ToListAsync();

                // Get available classrooms
                Classrooms = await _context.Classrooms
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                // Get active terms
                Terms = await _context.AcademicTerms
                    .Where(t => t.EndDate >= currentDate)
                    .OrderByDescending(t => t.StartDate)
                    .ToListAsync();

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reservations page");
                TempData["Error"] = "An error occurred while loading the reservations page.";
                return RedirectToPage("/Error");
            }
        }

        public async Task<IActionResult> OnPostCreateAsync(Reservation reservation)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var userId = GetCurrentUserId();
                reservation.UserId = userId;
                reservation.Status = ReservationStatus.Pending;
                reservation.CreatedAt = DateTime.Now;

                // Check for holiday conflicts
                if (await _holidayService.IsHolidayAsync(reservation.StartTime))
                {
                    TempData["Warning"] = "Warning: The selected date is a public holiday.";
                }

                // Check for existing reservations
                var hasConflict = await _context.Reservations
                    .AnyAsync(r => r.ClassroomId == reservation.ClassroomId &&
                                 r.StartTime <= reservation.EndTime &&
                                 r.EndTime >= reservation.StartTime);

                if (hasConflict)
                {
                    TempData["Error"] = "The selected time slot conflicts with an existing reservation.";
                    return Page();
                }

                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Reservation request submitted successfully.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reservation");
                TempData["Error"] = "An error occurred while creating the reservation.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var userId = GetCurrentUserId();
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (reservation == null)
            {
                return NotFound();
            }

            if (reservation.Status == ReservationStatus.Approved)
            {
                TempData["Error"] = "Cannot cancel an approved reservation.";
                return RedirectToPage();
            }

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Reservation cancelled successfully.";
            return RedirectToPage();
        }
    }
} 