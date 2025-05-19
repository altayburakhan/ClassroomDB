using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClassroomSystem.Data;
using ClassroomSystem.Models;
using ClassroomSystem.Services;

namespace ClassroomSystem.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class LogsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LogsModel> _logger;

        public LogsModel(ApplicationDbContext context, ILogger<LogsModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public List<SystemLog> Logs { get; set; }
        public List<SelectListItem> LogTypes { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public PaginationModel Pagination { get; set; }

        public async Task<IActionResult> OnGetAsync(
            string type = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int page = 1,
            int pageSize = 20)
        {
            try
            {
                // Get all log types
                var logTypes = await _context.SystemLogs
                    .Select(l => l.Type)
                    .Distinct()
                    .ToListAsync();

                LogTypes = logTypes.Select(lt => new SelectListItem
                {
                    Value = lt,
                    Text = lt
                }).ToList();

                // Build query
                var query = _context.SystemLogs.AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(type))
                {
                    query = query.Where(l => l.Type == type);
                }

                if (startDate.HasValue)
                {
                    query = query.Where(l => l.Timestamp >= startDate.Value);
                    StartDate = startDate;
                }

                if (endDate.HasValue)
                {
                    query = query.Where(l => l.Timestamp <= endDate.Value);
                    EndDate = endDate;
                }

                // Get total count
                var totalItems = await query.CountAsync();

                // Setup pagination
                Pagination = new PaginationModel(page, totalItems, pageSize);

                // Get logs for current page
                Logs = await query
                    .OrderByDescending(l => l.Timestamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading logs");
                TempData["Error"] = "An error occurred while loading the logs.";
                return Page();
            }
        }

        public async Task<IActionResult> OnGetLogDetailsAsync(int id)
        {
            try
            {
                var log = await _context.SystemLogs.FindAsync(id);
                if (log == null)
                {
                    return NotFound();
                }

                return new JsonResult(new
                {
                    id = log.Id,
                    type = log.Type,
                    message = log.Message,
                    timestamp = log.Timestamp,
                    details = log.Details
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading log details");
                return new JsonResult(new { error = "An error occurred while loading the log details." });
            }
        }

        public async Task<IActionResult> OnGetExportLogsAsync(
            string type = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                // Build query
                var query = _context.SystemLogs.AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(type))
                {
                    query = query.Where(l => l.Type == type);
                }

                if (startDate.HasValue)
                {
                    query = query.Where(l => l.Timestamp >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(l => l.Timestamp <= endDate.Value);
                }

                // Get logs
                var logs = await query
                    .OrderByDescending(l => l.Timestamp)
                    .ToListAsync();

                // Generate CSV
                var csv = new System.Text.StringBuilder();
                csv.AppendLine("ID,Type,Message,Timestamp,Details");

                foreach (var log in logs)
                {
                    csv.AppendLine($"{log.Id},{log.Type},{log.Message},{log.Timestamp},{log.Details}");
                }

                // Return file
                var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
                return File(bytes, "text/csv", $"system_logs_{DateTime.Now:yyyyMMdd}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting logs");
                TempData["Error"] = "An error occurred while exporting the logs.";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostClearLogsAsync()
        {
            var logs = await _context.SystemLogs.ToListAsync();
            _context.SystemLogs.RemoveRange(logs);
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }
    }
} 