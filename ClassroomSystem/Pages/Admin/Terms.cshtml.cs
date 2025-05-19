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
    public class TermsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public TermsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Term> Terms { get; set; }

        public async Task OnGetAsync()
        {
            Terms = await _context.Terms
                .OrderByDescending(t => t.StartDate)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAddTermAsync(string name, DateTime startDate, DateTime endDate, bool isActive)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Validate dates
            if (startDate >= endDate)
            {
                ModelState.AddModelError("", "End date must be after start date");
                return Page();
            }

            // Check for overlapping terms
            if (await _context.Terms.AnyAsync(t =>
                (startDate <= t.EndDate && endDate >= t.StartDate)))
            {
                ModelState.AddModelError("", "Term dates overlap with existing term");
                return Page();
            }

            // If setting as active, deactivate other terms
            if (isActive)
            {
                var activeTerms = await _context.Terms.Where(t => t.IsActive).ToListAsync();
                foreach (var term in activeTerms)
                {
                    term.IsActive = false;
                }
            }

            var newTerm = new Term
            {
                Name = name,
                StartDate = startDate,
                EndDate = endDate,
                IsActive = isActive
            };

            _context.Terms.Add(newTerm);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteTermAsync(int id)
        {
            var term = await _context.Terms.FindAsync(id);
            if (term == null)
            {
                return NotFound();
            }

            // Check if term has any reservations
            if (await _context.Reservations.AnyAsync(r => r.TermId == id))
            {
                ModelState.AddModelError("", "Cannot delete term with existing reservations");
                return Page();
            }

            _context.Terms.Remove(term);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateTermAsync(int id, string name, DateTime startDate, DateTime endDate, bool isActive)
        {
            var term = await _context.Terms.FindAsync(id);
            if (term == null)
            {
                return NotFound();
            }

            // Validate dates
            if (startDate >= endDate)
            {
                ModelState.AddModelError("", "End date must be after start date");
                return Page();
            }

            // Check for overlapping terms (excluding current term)
            if (await _context.Terms.AnyAsync(t =>
                t.Id != id && (startDate <= t.EndDate && endDate >= t.StartDate)))
            {
                ModelState.AddModelError("", "Term dates overlap with existing term");
                return Page();
            }

            // If setting as active, deactivate other terms
            if (isActive && !term.IsActive)
            {
                var activeTerms = await _context.Terms.Where(t => t.IsActive).ToListAsync();
                foreach (var activeTerm in activeTerms)
                {
                    activeTerm.IsActive = false;
                }
            }

            term.Name = name;
            term.StartDate = startDate;
            term.EndDate = endDate;
            term.IsActive = isActive;

            await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
} 