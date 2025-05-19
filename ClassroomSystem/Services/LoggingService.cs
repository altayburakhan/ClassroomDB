using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ClassroomSystem.Data;
using ClassroomSystem.Models;

namespace ClassroomSystem.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LoggingService> _logger;
        private readonly string _errorLogPath;

        public LoggingService(
            ApplicationDbContext context,
            ILogger<LoggingService> logger)
        {
            _context = context;
            _logger = logger;
            _errorLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "Errors");
            
            // Ensure error log directory exists
            if (!Directory.Exists(_errorLogPath))
            {
                Directory.CreateDirectory(_errorLogPath);
            }
        }

        public async Task LogUserActionAsync(string userId, string action, string details, bool isSuccess)
        {
            try
            {
                var log = new SystemLog
                {
                    UserId = userId,
                    Action = action,
                    Details = details,
                    IsSuccess = isSuccess,
                    Type = "UserAction",
                    Timestamp = DateTime.UtcNow
                };

                _context.SystemLogs.Add(log);
                await _context.SaveChangesAsync();

                // Also log to application logger
                if (isSuccess)
                {
                    _logger.LogInformation("User Action: {Action} by {UserId} - {Details}", action, userId, details);
                }
                else
                {
                    _logger.LogWarning("Failed User Action: {Action} by {UserId} - {Details}", action, userId, details);
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync("LoggingService", "LogUserAction", ex, $"Failed to log user action: {action}");
            }
        }

        public async Task LogErrorAsync(string userId, string action, Exception exception, string details = null)
        {
            try
            {
                // Log to database
                var log = new SystemLog
                {
                    UserId = userId,
                    Action = action,
                    Details = $"{details}\nError: {exception.Message}\nStack Trace: {exception.StackTrace}",
                    IsSuccess = false,
                    Type = "Error",
                    Timestamp = DateTime.UtcNow
                };

                _context.SystemLogs.Add(log);
                await _context.SaveChangesAsync();

                // Log to file
                var errorLogFile = Path.Combine(_errorLogPath, $"error_{DateTime.UtcNow:yyyyMMdd}.log");
                var errorMessage = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] " +
                                 $"User: {userId}, Action: {action}\n" +
                                 $"Details: {details}\n" +
                                 $"Error: {exception.Message}\n" +
                                 $"Stack Trace: {exception.StackTrace}\n" +
                                 $"Source: {exception.Source}\n" +
                                 new string('-', 80) + "\n";

                await File.AppendAllTextAsync(errorLogFile, errorMessage);

                // Log to application logger
                _logger.LogError(exception, "Error in {Action} by {UserId}: {Details}", action, userId, details);
            }
            catch (Exception ex)
            {
                // If logging fails, at least log to application logger
                _logger.LogError(ex, "Failed to log error: {Message}", ex.Message);
            }
        }

        public async Task LogSecurityEventAsync(string userId, string action, string details, bool isSuccess)
        {
            try
            {
                var log = new SystemLog
                {
                    UserId = userId,
                    Action = action,
                    Details = details,
                    IsSuccess = isSuccess,
                    Type = "Security",
                    Timestamp = DateTime.UtcNow
                };

                _context.SystemLogs.Add(log);
                await _context.SaveChangesAsync();

                // Also log to application logger
                if (isSuccess)
                {
                    _logger.LogInformation("Security Event: {Action} by {UserId} - {Details}", action, userId, details);
                }
                else
                {
                    _logger.LogWarning("Failed Security Event: {Action} by {UserId} - {Details}", action, userId, details);
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync("LoggingService", "LogSecurityEvent", ex, $"Failed to log security event: {action}");
            }
        }

        public async Task LogSystemEventAsync(string action, string details, bool isSuccess)
        {
            try
            {
                var log = new SystemLog
                {
                    Action = action,
                    Details = details,
                    IsSuccess = isSuccess,
                    Type = "System",
                    Timestamp = DateTime.UtcNow
                };

                _context.SystemLogs.Add(log);
                await _context.SaveChangesAsync();

                // Also log to application logger
                if (isSuccess)
                {
                    _logger.LogInformation("System Event: {Action} - {Details}", action, details);
                }
                else
                {
                    _logger.LogWarning("Failed System Event: {Action} - {Details}", action, details);
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync("LoggingService", "LogSystemEvent", ex, $"Failed to log system event: {action}");
            }
        }

        public async Task LogLoginAttemptAsync(string userId, bool success, string details)
        {
            await LogSecurityEventAsync(
                userId,
                "LoginAttempt",
                $"Login attempt {(success ? "successful" : "failed")}: {details}",
                success
            );
        }

        public async Task LogReservationActionAsync(string userId, string action, int reservationId, string details)
        {
            await LogUserActionAsync(
                userId,
                $"Reservation_{action}",
                $"Reservation ID: {reservationId}, {details}",
                true
            );
        }

        public async Task LogFeedbackSubmissionAsync(string userId, int feedbackId, string details)
        {
            try
            {
                var log = new Logs
                {
                    UserId = userId,
                    Action = "Feedback_Submission",
                    Details = $"FeedbackId: {feedbackId}, {details}",
                    Timestamp = DateTime.UtcNow,
                    Status = "Success"
                };

                _context.Logs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging feedback submission");
            }
        }

        public async Task LogSystemActionAsync(string action, string details)
        {
            try
            {
                var log = new Logs
                {
                    UserId = "System",
                    Action = action,
                    Details = details,
                    Timestamp = DateTime.UtcNow,
                    Status = "Success"
                };

                _context.Logs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging system action: {Action}", action);
            }
        }
    }
} 