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
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace ClassroomSystem.Pages.Instructor
{
    [Authorize(Roles = "Instructor")]
    public class FeedbackModel : BasePageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<FeedbackModel> _logger;

        public FeedbackModel(
            ApplicationDbContext context,
            IEmailService emailService,
            ILogger<FeedbackModel> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        [BindProperty]
        public Feedback Feedback { get; set; }
        public List<Classroom> Classrooms { get; set; }
        public List<Feedback> PreviousFeedback { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var userId = GetCurrentUserId();
                
                // Get classrooms that the instructor has used
                Classrooms = await _context.Classrooms
                    .Where(c => c.Reservations.Any(r => r.UserId == userId && r.Status == ReservationStatus.Approved))
                    .ToListAsync();

                // Get previous feedback
                PreviousFeedback = await _context.Feedbacks
                    .Include(f => f.Classroom)
                    .Where(f => f.UserId == userId)
                    .OrderByDescending(f => f.CreatedAt)
                    .ToListAsync();

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading feedback page");
                TempData["Error"] = "An error occurred while loading the feedback page.";
                return RedirectToPage("/Error");
            }
        }

        public async Task<IActionResult> OnPostSubmitFeedbackAsync(int classroomId, int rating, string comment)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var userId = GetCurrentUserId();

                // Validate rating
                if (rating < 1 || rating > 5)
                {
                    ModelState.AddModelError("", "Rating must be between 1 and 5");
                    return Page();
                }

                // Validate comment length
                if (string.IsNullOrWhiteSpace(comment) || comment.Length > 500)
                {
                    ModelState.AddModelError("", "Comment must be between 1 and 500 characters");
                    return Page();
                }

                var feedback = new Feedback
                {
                    UserId = userId,
                    ClassroomId = classroomId,
                    Rating = rating,
                    Comment = comment,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Feedbacks.Add(feedback);
                await _context.SaveChangesAsync();

                // Send email notification to admin
                var classroom = await _context.Classrooms.FindAsync(classroomId);
                var user = await _context.Users.FindAsync(userId);
                await _emailService.SendFeedbackNotificationEmailAsync(
                    "admin@example.com", // Replace with actual admin email
                    user.Name,
                    classroom.Name,
                    rating,
                    comment
                );

                TempData["Success"] = "Feedback submitted successfully.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting feedback");
                TempData["Error"] = "An error occurred while submitting feedback.";
                return Page();
            }
        }
    }
} 