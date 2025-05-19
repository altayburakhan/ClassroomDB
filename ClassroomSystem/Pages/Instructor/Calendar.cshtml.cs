using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClassroomSystem.Data;
using ClassroomSystem.Models;
using ClassroomSystem.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace ClassroomSystem.Pages.Instructor
{
    [Authorize(Roles = "Instructor")]
    public class CalendarModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IHolidayService _holidayService;
        private readonly ILogger<CalendarModel> _logger;

        public CalendarModel(
            ApplicationDbContext context,
            IEmailService emailService,
            IHolidayService holidayService,
            ILogger<CalendarModel> logger)
        {
            _context = context;
            _emailService = emailService;
            _holidayService = holidayService;
            _logger = logger;
        }

        public Term CurrentTerm { get; set; }
        public List<Classroom> Classrooms { get; set; }
        public List<CalendarEvent> CalendarEvents { get; set; }
        public List<TimeSlot> TimeSlots { get; set; }

        public async Task<IActionResult> OnGetAsync(string start = null, string end = null)
        {
            try
            {
                // Get current term
                CurrentTerm = await _context.Terms
                    .FirstOrDefaultAsync(t => t.StartDate <= DateTime.Today && t.EndDate >= DateTime.Today);

                if (CurrentTerm == null)
                {
                    TempData["Error"] = "No active term found.";
                    return Page();
                }

                // Get active classrooms
                Classrooms = await _context.Classrooms
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                // Generate time slots
                TimeSlots = new List<TimeSlot>();
                var startTime = new TimeSpan(8, 0, 0); // 8:00 AM
                var endTime = new TimeSpan(17, 0, 0); // 5:00 PM
                var interval = new TimeSpan(0, 30, 0); // 30 minutes

                while (startTime < endTime)
                {
                    TimeSlots.Add(new TimeSlot
                    {
                        StartTime = startTime,
                        EndTime = startTime.Add(interval)
                    });
                    startTime = startTime.Add(interval);
                }

                // Parse date range
                var startDate = !string.IsNullOrEmpty(start) ? DateTime.Parse(start) : DateTime.Today;
                var endDate = !string.IsNullOrEmpty(end) ? DateTime.Parse(end) : DateTime.Today.AddDays(30);

                // Convert to UTC for database queries
                startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
                endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

                // Get reservations
                var reservations = await _context.Reservations
                    .Include(r => r.Classroom)
                    .Include(r => r.User)
                    .Where(r => r.StartTime >= startDate && r.EndTime <= endDate)
                    .ToListAsync();

                // Get holidays
                var holidays = await _holidayService.GetHolidaysAsync(startDate, endDate);

                // Convert to calendar events
                CalendarEvents = new List<CalendarEvent>();

                // Add reservations
                foreach (var r in reservations)
                {
                    CalendarEvents.Add(new CalendarEvent
                    {
                        Id = r.Id.ToString(),
                        Title = $"{r.Classroom.Name} - {r.User.FirstName} {r.User.LastName}",
                        Start = r.StartTime,
                        End = r.EndTime,
                        ClassName = r.Status == ReservationStatus.Approved ? "event-approved" :
                                  r.Status == ReservationStatus.Pending ? "event-pending" :
                                  r.Status == ReservationStatus.Rejected ? "event-rejected" : "event-cancelled",
                        AllDay = false,
                        ExtendedProps = new Dictionary<string, object>
                        {
                            { "classroomId", r.ClassroomId },
                            { "classroomName", r.Classroom.Name },
                            { "instructorId", r.UserId.ToString() },
                            { "instructorName", $"{r.User.FirstName} {r.User.LastName}" },
                            { "purpose", r.Purpose },
                            { "status", r.Status.ToString() }
                        }
                    });
                }

                // Add holidays
                foreach (var h in holidays)
                {
                    var holidayName = await _holidayService.GetHolidayNameAsync(h);
                    CalendarEvents.Add(new CalendarEvent
                    {
                        Id = $"holiday-{h:yyyyMMdd}",
                        Title = holidayName ?? "Holiday",
                        Start = h,
                        End = h.AddDays(1),
                        ClassName = "event-holiday",
                        AllDay = true,
                        ExtendedProps = new Dictionary<string, object>
                        {
                            { "isHoliday", true },
                            { "holidayName", holidayName ?? "Holiday" }
                        }
                    });
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading calendar data");
                TempData["Error"] = "An error occurred while loading the calendar.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAddReservationAsync(
            int classroomId,
            DateTime date,
            TimeSpan startTime,
            TimeSpan endTime,
            string purpose)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                // Validate date and time
                if (date.Date < DateTime.Today)
                {
                    ModelState.AddModelError("", "Cannot make reservations for past dates");
                    return Page();
                }

                if (startTime >= endTime)
                {
                    ModelState.AddModelError("", "End time must be after start time");
                    return Page();
                }

                // Check if classroom is available
                var isAvailable = await _context.Reservations
                    .Where(r => r.ClassroomId == classroomId && 
                               r.StartTime.Date == date.Date && 
                               r.Status != ReservationStatus.Cancelled)
                    .AllAsync(r => endTime <= r.StartTime.TimeOfDay || startTime >= r.EndTime.TimeOfDay);

                if (!isAvailable)
                {
                    ModelState.AddModelError("", "Classroom is not available at the selected time");
                    return Page();
                }

                // Check if date is a holiday
                var holidays = await _holidayService.GetHolidaysAsync(date, date);
                if (holidays.Any())
                {
                    ModelState.AddModelError("", "Cannot make reservations on holidays");
                    return Page();
                }

                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    ModelState.AddModelError("", "User not found");
                    return Page();
                }

                var newReservation = new Reservation
                {
                    ClassroomId = classroomId,
                    UserId = userId,
                    StartTime = date.Date.Add(startTime),
                    EndTime = date.Date.Add(endTime),
                    Purpose = purpose,
                    Status = ReservationStatus.Pending
                };

                _context.Reservations.Add(newReservation);
                await _context.SaveChangesAsync();

                // Send email notification
                var classroom = await _context.Classrooms.FindAsync(classroomId);
                var user = await _context.Users.FindAsync(userId);
                if (classroom != null && user != null)
                {
                    await _emailService.SendReservationNotificationAsync(
                        user.Email,
                        classroom.Name,
                        newReservation.StartTime,
                        newReservation.EndTime,
                        newReservation.Purpose);
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding reservation");
                TempData["Error"] = "An error occurred while adding the reservation.";
                return Page();
            }
        }
    }

    public class CalendarEvent
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("start")]
        public DateTime Start { get; set; }

        [JsonPropertyName("end")]
        public DateTime End { get; set; }

        [JsonPropertyName("className")]
        public string ClassName { get; set; }

        [JsonPropertyName("allDay")]
        public bool AllDay { get; set; }

        [JsonPropertyName("extendedProps")]
        public Dictionary<string, object> ExtendedProps { get; set; }
    }

    public class TimeSlot
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
} 