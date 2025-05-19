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
using System.Text.Json;

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
        public List<CalendarEvent> CalendarEvents { get; set; } = new List<CalendarEvent>();
        public List<TimeSlot> TimeSlots { get; set; }

        private async Task<Term> EnsureActiveTermExistsAsync()
        {
            try
            {
                _logger.LogInformation("Checking for active term...");
                
                var term = await _context.Terms
                    .FirstOrDefaultAsync(t => t.StartDate <= DateTime.Today && t.EndDate >= DateTime.Today && t.IsActive);

                if (term != null)
                {
                    _logger.LogInformation($"Found active term: {term.Name} ({term.StartDate:d} - {term.EndDate:d})");
                    return term;
                }

                _logger.LogWarning("No active term found. Creating a new term...");
                
                // Create a new academic term for the current semester
                var today = DateTime.Today;
                var termStart = new DateTime(today.Year, today.Month, 1);
                var termEnd = termStart.AddMonths(4).AddDays(-1);
                var termName = $"{today.Year} {(today.Month <= 6 ? "Spring" : "Fall")} Term";

                var newTerm = new Term
                {
                    Name = termName,
                    StartDate = termStart,
                    EndDate = termEnd,
                    IsActive = true,
                    Description = "Automatically created term"
                };

                _logger.LogInformation($"Creating new term: {JsonSerializer.Serialize(newTerm)}");

                _context.Terms.Add(newTerm);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Successfully created new term with ID: {newTerm.Id}");
                TempData["Info"] = $"Created new academic term: {newTerm.Name}";

                return newTerm;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create or retrieve active term");
                throw; // Re-throw to be handled by the caller
            }
        }

        public async Task<IActionResult> OnGetAsync(string start = null, string end = null)
        {
            try
            {
                // Get or create current term
                try
                {
                    CurrentTerm = await EnsureActiveTermExistsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to ensure active term exists");
                    TempData["Error"] = "Failed to create academic term. Please contact an administrator.";
                }

                // Get active classrooms
                _logger.LogInformation("Starting to fetch classrooms...");
                
                Classrooms = await _context.Classrooms
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                _logger.LogInformation($"Found {Classrooms.Count} classrooms in database");
                foreach (var classroom in Classrooms)
                {
                    _logger.LogInformation($"Classroom: {classroom.Name}, IsAvailable: {classroom.IsAvailable}, IsActive: {classroom.IsActive}, Capacity: {classroom.Capacity}, Id: {classroom.Id}");
                }

                if (Classrooms == null || !Classrooms.Any())
                {
                    _logger.LogWarning("No classrooms found in the database!");
                    TempData["Warning"] = "No classrooms are available for reservation.";
                }

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
                    try
                    {
                        var holidayName = await _holidayService.GetHolidayNameAsync(h);
                        var turkishHolidays = TurkishHolidays.GetHolidaysForYear(h.Year);
                        var turkishHoliday = turkishHolidays.FirstOrDefault(th => th.Date.Date == h.Date);

                        var isReligious = turkishHoliday != default ? turkishHoliday.IsReligious : false;
                        var holidayClassName = isReligious ? "holiday-religious" : "holiday-national";
                        var finalHolidayName = turkishHoliday != default ? turkishHoliday.Name : (holidayName ?? "Holiday");

                        _logger.LogInformation($"Adding holiday: {finalHolidayName} on {h:yyyy-MM-dd}, IsReligious: {isReligious}");

                        CalendarEvents.Add(new CalendarEvent
                        {
                            Id = $"holiday-{h:yyyyMMdd}",
                            Title = finalHolidayName,
                            Start = h,
                            End = h.AddDays(1),
                            ClassName = holidayClassName,
                            AllDay = true,
                            ExtendedProps = new Dictionary<string, object>
                            {
                                { "isHoliday", true },
                                { "isReligious", isReligious },
                                { "holidayName", finalHolidayName }
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error adding holiday for date {h:yyyy-MM-dd}");
                    }
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
            string startTime,
            string endTime,
            string purpose,
            string notes)
        {
            try
            {
                _logger.LogInformation($"Starting reservation process: ClassroomId={classroomId}, Date={date}, StartTime={startTime}, EndTime={endTime}, Purpose={purpose}");

                // Get or create current term first
                try
                {
                    CurrentTerm = await EnsureActiveTermExistsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to ensure active term exists during reservation");
                    return new JsonResult(new { success = false, message = "Failed to create academic term. Please try again or contact an administrator." });
                }

                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    _logger.LogWarning($"ModelState is invalid: {errors}");
                    return new JsonResult(new { success = false, message = "Invalid form data" });
                }

                // Parse time values
                if (!TimeSpan.TryParse(startTime, out TimeSpan startTimeSpan))
                {
                    _logger.LogWarning($"Invalid start time format: {startTime}");
                    return new JsonResult(new { success = false, message = "Invalid start time format" });
                }

                if (!TimeSpan.TryParse(endTime, out TimeSpan endTimeSpan))
                {
                    _logger.LogWarning($"Invalid end time format: {endTime}");
                    return new JsonResult(new { success = false, message = "Invalid end time format" });
                }

                // Validate date and time
                if (date.Date < DateTime.Today)
                {
                    _logger.LogWarning($"Past date selected: {date}");
                    return new JsonResult(new { success = false, message = "Cannot make reservations for past dates" });
                }

                if (startTimeSpan >= endTimeSpan)
                {
                    _logger.LogWarning($"Invalid time range: {startTime} - {endTime}");
                    return new JsonResult(new { success = false, message = "End time must be after start time" });
                }

                // Validate business hours
                var minTime = TimeSpan.FromHours(8); // 8:00 AM
                var maxTime = TimeSpan.FromHours(17); // 5:00 PM
                if (startTimeSpan < minTime || endTimeSpan > maxTime)
                {
                    _logger.LogWarning($"Outside business hours: {startTime} - {endTime}");
                    return new JsonResult(new { success = false, message = "Reservations are only allowed between 8:00 AM and 5:00 PM" });
                }

                // Check if classroom is available
                var reservationStart = date.Date.Add(startTimeSpan);
                var reservationEnd = date.Date.Add(endTimeSpan);

                _logger.LogInformation($"Checking availability for classroom {classroomId} at {reservationStart} - {reservationEnd}");

                var isAvailable = await _context.Reservations
                    .Where(r => r.ClassroomId == classroomId && 
                               r.StartTime.Date == date.Date && 
                               r.Status != ReservationStatus.Cancelled)
                    .AllAsync(r => reservationEnd <= r.StartTime || reservationStart >= r.EndTime);

                if (!isAvailable)
                {
                    _logger.LogWarning($"Classroom {classroomId} is not available at {reservationStart} - {reservationEnd}");
                    return new JsonResult(new { success = false, message = "Classroom is not available at the selected time" });
                }

                // Check if date is a holiday
                var holidays = await _holidayService.GetHolidaysAsync(date, date);
                if (holidays.Any())
                {
                    _logger.LogWarning($"Attempted reservation on holiday: {date}");
                    return new JsonResult(new { success = false, message = "Cannot make reservations on holidays" });
                }

                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in claims");
                    return new JsonResult(new { success = false, message = "User not found" });
                }

                var classroom = await _context.Classrooms.FindAsync(classroomId);
                if (classroom == null)
                {
                    _logger.LogWarning($"Classroom not found: {classroomId}");
                    return new JsonResult(new { success = false, message = "Classroom not found" });
                }

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning($"User not found: {userId}");
                    return new JsonResult(new { success = false, message = "User details not found" });
                }

                // Check if we have an active term
                if (CurrentTerm == null)
                {
                    _logger.LogWarning("No active term found");
                    return new JsonResult(new { success = false, message = "No active term found. Please contact an administrator." });
                }

                var newReservation = new Reservation
                {
                    ClassroomId = classroomId,
                    UserId = userId,
                    StartTime = reservationStart,
                    EndTime = reservationEnd,
                    Purpose = purpose,
                    Notes = notes ?? "No additional notes",
                    Status = ReservationStatus.Pending,
                    TermId = CurrentTerm.Id,
                    IsRecurring = false,
                    RecurrencePattern = "None",
                    RejectionReason = string.Empty
                };

                _logger.LogInformation($"Adding new reservation: {JsonSerializer.Serialize(newReservation)}");

                _context.Reservations.Add(newReservation);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Successfully created reservation with ID: {newReservation.Id}");

                // Send email notification
                if (classroom != null && user != null)
                {
                    await _emailService.SendReservationNotificationAsync(
                        user.Email,
                        classroom.Name,
                        newReservation.StartTime,
                        newReservation.EndTime,
                        newReservation.Purpose);
                }

                // Create calendar event object
                var calendarEvent = new CalendarEvent
                {
                    Id = newReservation.Id.ToString(),
                    Title = $"{classroom.Name} - {user.FirstName} {user.LastName}",
                    Start = newReservation.StartTime,
                    End = newReservation.EndTime,
                    ClassName = "event-pending",
                    AllDay = false,
                    ExtendedProps = new Dictionary<string, object>
                    {
                        { "classroomId", classroom.Id },
                        { "classroomName", classroom.Name },
                        { "instructorId", user.Id },
                        { "instructorName", $"{user.FirstName} {user.LastName}" },
                        { "purpose", purpose },
                        { "status", "Pending" },
                        { "notes", notes }
                    }
                };

                return new JsonResult(new { success = true, reservation = calendarEvent });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding reservation");
                return new JsonResult(new { 
                    success = false, 
                    message = "An error occurred while adding the reservation.",
                    error = ex.Message  // Include the actual error message for debugging
                });
            }
        }

        public async Task<IActionResult> OnGetDebugAsync()
        {
            var classrooms = await _context.Classrooms.ToListAsync();
            var result = classrooms.Select(c => new
            {
                c.Id,
                c.Name,
                c.IsAvailable,
                c.IsActive,
                c.Capacity
            }).ToList();

            return new JsonResult(result);
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