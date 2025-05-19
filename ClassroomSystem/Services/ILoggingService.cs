using System;
using System.Threading.Tasks;
using ClassroomSystem.Models;

namespace ClassroomSystem.Services
{
    public interface ILoggingService
    {
        Task LogUserActionAsync(string userId, string action, string details, bool isSuccess);
        Task LogErrorAsync(string userId, string action, Exception exception, string details = null);
        Task LogSecurityEventAsync(string userId, string action, string details, bool isSuccess);
        Task LogSystemEventAsync(string action, string details, bool isSuccess);
        Task LogLoginAttemptAsync(string userId, bool success, string details);
        Task LogReservationActionAsync(string userId, string action, int reservationId, string details);
        Task LogFeedbackSubmissionAsync(string userId, int feedbackId, string details);
        Task LogSystemActionAsync(string action, string details);
    }
} 