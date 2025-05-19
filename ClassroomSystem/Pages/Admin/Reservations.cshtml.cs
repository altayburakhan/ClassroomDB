using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClassroomSystem.Data;
using ClassroomSystem.Models;
using ClassroomSystem.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;

namespace ClassroomSystem.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class ReservationsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<ReservationsModel> _logger;

        public ReservationsModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            ILogger<ReservationsModel> logger)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
        }

        public List<Reservation> Reservations { get; set; }
        public List<Classroom> Classrooms { get; set; }
        public IList<ApplicationUser> Instructors { get; set; }
        public PaginationModel Pagination { get; set; }

        public async Task OnGetAsync(int page = 1)
        {
            // Get total count for pagination
            var totalItems = await _context.Reservations.CountAsync();
            Pagination = new PaginationModel(page, totalItems);

            // Get paginated reservations with related data
            Reservations = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Classroom)
                .OrderByDescending(r => r.StartTime)
                .Skip((page - 1) * Pagination.PageSize)
                .Take(Pagination.PageSize)
                .ToListAsync();

            // Get all active classrooms
            Classrooms = await _context.Classrooms
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            // Get all instructors
            Instructors = await _userManager.GetUsersInRoleAsync("Instructor");
        }

        public async Task<IActionResult> OnPostEditReservationAsync(int id, ReservationStatus status, string rejectionReason, string notes)
        {
            var reservation = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Classroom)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }

            reservation.Status = status;
            reservation.RejectionReason = rejectionReason;
            reservation.Notes = notes;
            reservation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Send email notification
            if (status == ReservationStatus.Approved)
            {
                await _emailService.SendReservationApprovalEmailAsync(
                    reservation.User.Email,
                    reservation.UserId,
                    $"{reservation.User.FirstName} {reservation.User.LastName}",
                    reservation.Classroom.Name,
                    reservation.StartTime.DayOfWeek.ToString(),
                    $"{reservation.StartTime:HH:mm} - {reservation.EndTime:HH:mm}",
                    reservation.Id);
            }
            else if (status == ReservationStatus.Rejected)
            {
                await _emailService.SendReservationRejectionEmailAsync(
                    reservation.User.Email,
                    reservation.UserId,
                    $"{reservation.User.FirstName} {reservation.User.LastName}",
                    reservation.Classroom.Name,
                    reservation.StartTime.DayOfWeek.ToString(),
                    $"{reservation.StartTime:HH:mm} - {reservation.EndTime:HH:mm}",
                    rejectionReason,
                    reservation.Id);
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteReservationAsync(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddReservationAsync(
            int classroomId, 
            string instructorId, 
            string date,
            string startTime,
            string endTime,
            string purpose)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                // Parse date and time
                if (!DateTime.TryParse(date, out DateTime reservationDate))
                {
                    ModelState.AddModelError("", "Invalid date format");
                    return Page();
                }

                if (!TimeSpan.TryParse(startTime, out TimeSpan startTimeSpan))
                {
                    ModelState.AddModelError("", "Invalid start time format");
                    return Page();
                }

                if (!TimeSpan.TryParse(endTime, out TimeSpan endTimeSpan))
                {
                    ModelState.AddModelError("", "Invalid end time format");
                    return Page();
                }

                var startDateTime = reservationDate.Date.Add(startTimeSpan);
                var endDateTime = reservationDate.Date.Add(endTimeSpan);

                // Validate date and time
                if (startDateTime.Date < DateTime.Today)
                {
                    ModelState.AddModelError("", "Cannot make reservations for past dates");
                    return Page();
                }

                if (startDateTime >= endDateTime)
                {
                    ModelState.AddModelError("", "End time must be after start time");
                    return Page();
                }

                // Check if classroom is available
                var isAvailable = await _context.Reservations
                    .Where(r => r.ClassroomId == classroomId && 
                               r.StartTime.Date == startDateTime.Date && 
                               r.Status != ReservationStatus.Cancelled)
                    .AllAsync(r => endDateTime <= r.StartTime || startDateTime >= r.EndTime);

                if (!isAvailable)
                {
                    ModelState.AddModelError("", "Classroom is not available at the selected time");
                    return Page();
                }

                // Get current term
                var currentTerm = await _context.Terms
                    .Where(t => t.StartDate <= startDateTime && t.EndDate >= endDateTime && t.IsActive)
                    .FirstOrDefaultAsync();

                if (currentTerm == null)
                {
                    ModelState.AddModelError("", "No active term found for the selected dates");
                    return Page();
                }

                var reservation = new Reservation
                {
                    ClassroomId = classroomId,
                    UserId = instructorId,
                    StartTime = startDateTime,
                    EndTime = endDateTime,
                    TermId = currentTerm.Id,
                    Purpose = purpose,
                    Status = ReservationStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();

                // Send email notification
                var instructor = await _userManager.FindByIdAsync(instructorId);
                var classroom = await _context.Classrooms.FindAsync(classroomId);
                if (instructor != null && classroom != null)
                {
                    await _emailService.SendReservationNotificationAsync(
                        instructor.Email,
                        classroom.Name,
                        startDateTime,
                        endDateTime,
                        purpose);
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reservation");
                ModelState.AddModelError("", "An error occurred while creating the reservation");
                return Page();
            }
        }

        public async Task<IActionResult> OnPostApproveReservationAsync(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Classroom)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }

            reservation.Status = ReservationStatus.Approved;
            await _context.SaveChangesAsync();

            // Send approval email
            await _emailService.SendReservationApprovalEmailAsync(
                reservation.User.Email,
                reservation.UserId,
                $"{reservation.User.FirstName} {reservation.User.LastName}",
                reservation.Classroom.Name,
                reservation.StartTime.DayOfWeek.ToString(),
                $"{reservation.StartTime:HH:mm} - {reservation.EndTime:HH:mm}",
                reservation.Id);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectReservationAsync(int id, string reason)
        {
            var reservation = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Classroom)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }

            reservation.Status = ReservationStatus.Rejected;
            reservation.RejectionReason = reason;
            await _context.SaveChangesAsync();

            // Send rejection email
            await _emailService.SendReservationRejectionEmailAsync(
                reservation.User.Email,
                reservation.UserId,
                $"{reservation.User.FirstName} {reservation.User.LastName}",
                reservation.Classroom.Name,
                reservation.StartTime.DayOfWeek.ToString(),
                $"{reservation.StartTime:HH:mm} - {reservation.EndTime:HH:mm}",
                reason,
                reservation.Id);

            return RedirectToPage();
        }
    }
} 