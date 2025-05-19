using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClassroomSystem.Data;
using ClassroomSystem.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Identity;

namespace ClassroomSystem.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class InstructorsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;

        public InstructorsModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IPasswordHasher<ApplicationUser> passwordHasher)
        {
            _context = context;
            _userManager = userManager;
            _passwordHasher = passwordHasher;
            Instructors = new List<ApplicationUser>();
        }

        public List<ApplicationUser> Instructors { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                Instructors = await _userManager.Users
                    .Where(u => u.Role == "Instructor")
                    .OrderBy(u => u.LastName)
                    .ThenBy(u => u.FirstName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error loading instructors: " + ex.Message);
                Instructors = new List<ApplicationUser>();
            }
        }

        public async Task<IActionResult> OnPostAddInstructorAsync(
            string firstName,
            string lastName,
            string email,
            string title,
            string department,
            string password,
            bool isActive)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Check if email already exists
            if (await _userManager.FindByEmailAsync(email) != null)
            {
                ModelState.AddModelError("Email", "Email already exists");
                return Page();
            }

            var instructor = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Title = title,
                Department = department,
                Role = "Instructor",
                IsActive = isActive
            };

            var result = await _userManager.CreateAsync(instructor, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(instructor, "Instructor");
                return RedirectToPage();
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateInstructorAsync(
            string id,
            string firstName,
            string lastName,
            string email,
            string title,
            string department,
            bool isActive)
        {
            var instructor = await _userManager.FindByIdAsync(id);
            if (instructor == null)
            {
                return NotFound();
            }

            // Check if email is changed and already exists
            if (email != instructor.Email && await _userManager.FindByEmailAsync(email) != null)
            {
                ModelState.AddModelError("Email", "Email already exists");
                return Page();
            }

            instructor.FirstName = firstName;
            instructor.LastName = lastName;
            instructor.Email = email;
            instructor.UserName = email;
            instructor.Title = title;
            instructor.Department = department;
            instructor.IsActive = isActive;

            var result = await _userManager.UpdateAsync(instructor);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return Page();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteInstructorAsync(string id)
        {
            var instructor = await _userManager.FindByIdAsync(id);
            if (instructor == null)
            {
                return NotFound();
            }

            // Check if instructor has any reservations
            if (await _context.Reservations.AnyAsync(r => r.UserId == id))
            {
                ModelState.AddModelError("", "Cannot delete instructor with existing reservations");
                return Page();
            }

            var result = await _userManager.DeleteAsync(instructor);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return Page();
            }

            return RedirectToPage();
        }
    }
} 