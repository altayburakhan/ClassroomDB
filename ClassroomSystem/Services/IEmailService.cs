using System;
using System.Threading.Tasks;
using ClassroomSystem.Models;

namespace ClassroomSystem.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendReservationApprovalEmailAsync(string toEmail, string userName, string classroomName, string dayOfWeek, string timeSlot);
        Task SendReservationRejectionEmailAsync(string toEmail, string userName, string classroomName, string dayOfWeek, string timeSlot, string reason);
        Task SendHolidayWarningEmailAsync(string toEmail, string userName, string classroomName, string date, string timeSlot);
        Task SendFeedbackNotificationEmailAsync(string toEmail, string userName, string classroomName, int rating, string comment);
        Task SendPasswordResetEmailAsync(ApplicationUser user, string resetLink);
        Task SendWelcomeEmailAsync(ApplicationUser user, string password);
        Task SendReservationNotificationAsync(string toEmail, string classroomName, DateTime startTime, DateTime endTime, string purpose);
        Task SendFeedbackNotificationAsync(Feedback feedback);
    }
} 