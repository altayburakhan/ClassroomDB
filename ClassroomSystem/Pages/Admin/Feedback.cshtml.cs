using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClassroomSystem.Data;
using ClassroomSystem.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClassroomSystem.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class FeedbackModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public FeedbackModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Feedback> Feedbacks { get; set; }
        public List<Classroom> Classrooms { get; set; }

        // Feedback statistics
        public int TotalFeedback { get; set; }
        public int PositiveFeedback { get; set; }
        public int NeutralFeedback { get; set; }
        public int NegativeFeedback { get; set; }

        public async Task OnGetAsync()
        {
            // Get all feedbacks with related data
            Feedbacks = await _context.Feedbacks
                .Include(f => f.User)
                .Include(f => f.Classroom)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            // Get all active classrooms
            Classrooms = await _context.Classrooms
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            // Calculate statistics
            TotalFeedback = Feedbacks.Count;
            PositiveFeedback = Feedbacks.Count(f => f.Rating >= 4);
            NeutralFeedback = Feedbacks.Count(f => f.Rating == 3);
            NegativeFeedback = Feedbacks.Count(f => f.Rating <= 2);
        }

        public async Task<IActionResult> OnPostDeleteFeedbackAsync(int id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback != null)
            {
                _context.Feedbacks.Remove(feedback);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
} 