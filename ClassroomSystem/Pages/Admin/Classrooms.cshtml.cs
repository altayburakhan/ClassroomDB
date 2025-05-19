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

namespace ClassroomSystem.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class ClassroomsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ClassroomsModel(ApplicationDbContext context)
        {
            _context = context;
            Classrooms = new List<Classroom>();
        }

        public List<Classroom> Classrooms { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                Classrooms = await _context.Classrooms
                    .OrderBy(c => c.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error loading classrooms: " + ex.Message);
                Classrooms = new List<Classroom>();
            }
        }

        public async Task<IActionResult> OnPostAddClassroomAsync(
            string name, 
            string roomNumber, 
            string building, 
            int floor, 
            int capacity, 
            string features, 
            bool isAvailable)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Check if classroom name already exists
            if (await _context.Classrooms.AnyAsync(c => c.Name == name))
            {
                ModelState.AddModelError("Name", "Classroom name already exists");
                return Page();
            }

            var classroom = new Classroom
            {
                Name = name,
                RoomNumber = roomNumber,
                Building = building,
                Floor = floor,
                Capacity = capacity,
                Features = features,
                IsAvailable = isAvailable,
                IsActive = true,
                Description = $"Classroom {name} in {building}"
            };

            _context.Classrooms.Add(classroom);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteClassroomAsync(int id)
        {
            var classroom = await _context.Classrooms.FindAsync(id);
            if (classroom == null)
            {
                return NotFound();
            }

            // Check if classroom has any reservations
            if (await _context.Reservations.AnyAsync(r => r.ClassroomId == id))
            {
                ModelState.AddModelError("", "Cannot delete classroom with existing reservations");
                return Page();
            }

            _context.Classrooms.Remove(classroom);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleClassroomStatusAsync(int id)
        {
            var classroom = await _context.Classrooms.FindAsync(id);
            if (classroom == null)
            {
                return NotFound();
            }

            classroom.IsActive = !classroom.IsActive;
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateClassroomAsync(int id, string name, int capacity, string description)
        {
            var classroom = await _context.Classrooms.FindAsync(id);
            if (classroom == null)
            {
                return NotFound();
            }

            // Check if new name conflicts with existing classroom
            if (name != classroom.Name && await _context.Classrooms.AnyAsync(c => c.Name == name))
            {
                ModelState.AddModelError("Name", "Classroom name already exists");
                return Page();
            }

            classroom.Name = name;
            classroom.Capacity = capacity;
            classroom.Description = description;

            await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
} 