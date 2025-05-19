using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using ClassroomSystem.Models;
using ClassroomSystem.Data;

namespace ClassroomSystem.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration, ApplicationDbContext context)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
            
            _smtpServer = _configuration["EmailSettings:SmtpServer"];
            _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
            _smtpUsername = _configuration["EmailSettings:SmtpUsername"];
            _smtpPassword = _configuration["EmailSettings:SmtpPassword"];
            _fromEmail = _configuration["EmailSettings:FromEmail"];
            _fromName = _configuration["EmailSettings:FromName"];
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                _logger.LogInformation($"[EMAIL NOT SENT - DISABLED] To: {to}, Subject: {subject}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending email to {to}");
                throw;
            }
        }

        public async Task SendReservationApprovalEmailAsync(string toEmail, string userId, string userName, string classroomName, string dayOfWeek, string timeSlot, int reservationId)
        {
            var subject = "Reservation Approved";
            var body = $"Dear {userName},\n\nYour reservation for {classroomName} on {dayOfWeek} at {timeSlot} has been approved.\n\nBest regards,\nClassroom System";

            // Create notification
            var notification = new Notification
            {
                UserId = userId,
                Title = "Reservation Approved",
                Message = $"Your reservation for {classroomName} on {dayOfWeek} at {timeSlot} has been approved.",
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                Type = "ReservationApproval",
                ReservationId = reservationId
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendReservationRejectionEmailAsync(string toEmail, string userId, string userName, string classroomName, string dayOfWeek, string timeSlot, string reason, int reservationId)
        {
            var subject = "Reservation Rejected";
            var body = $"Dear {userName},\n\nYour reservation for {classroomName} on {dayOfWeek} at {timeSlot} has been rejected.\n\nReason: {reason}\n\nBest regards,\nClassroom System";

            // Create notification
            var notification = new Notification
            {
                UserId = userId,
                Title = "Reservation Rejected",
                Message = $"Your reservation for {classroomName} on {dayOfWeek} at {timeSlot} has been rejected. Reason: {reason}",
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                Type = "ReservationRejection",
                ReservationId = reservationId
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendHolidayWarningEmailAsync(string toEmail, string userId, string userName, string classroomName, string date, int reservationId)
        {
            var subject = "Holiday Warning";
            var body = $"Dear {userName},\n\nYour reservation for {classroomName} on {date} falls on a holiday. Please consider rescheduling.\n\nBest regards,\nClassroom System";

            // Create notification
            var notification = new Notification
            {
                UserId = userId,
                Title = "Holiday Warning",
                Message = $"Your reservation for {classroomName} on {date} falls on a holiday. Please consider rescheduling.",
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                Type = "Holiday",
                ReservationId = reservationId
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendFeedbackNotificationEmailAsync(string toEmail, string userName, string classroomName, int rating, string comment)
        {
            var subject = "New Feedback Received";
            var body = $@"
                <h2>New Feedback</h2>
                <p>New feedback has been submitted:</p>
                <ul>
                    <li>Classroom: {classroomName}</li>
                    <li>User: {userName}</li>
                    <li>Rating: {rating}/5</li>
                    <li>Comment: {comment}</li>
                </ul>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(ApplicationUser user, string resetLink)
        {
            var subject = "Şifre Sıfırlama";
            var body = $@"
                <h2>Şifre Sıfırlama</h2>
                <p>Sayın {user.Name},</p>
                <p>Şifrenizi sıfırlamak için aşağıdaki bağlantıya tıklayın:</p>
                <p><a href='{resetLink}'>Şifremi Sıfırla</a></p>
                <p>Bu bağlantı 24 saat geçerlidir.</p>";

            await SendEmailAsync(user.Email, subject, body);
        }

        public async Task SendWelcomeEmailAsync(ApplicationUser user, string password)
        {
            var subject = "Hoş Geldiniz";
            var body = $@"
                <h2>Hoş Geldiniz</h2>
                <p>Sayın {user.Name},</p>
                <p>Sınıf Rezervasyon Sistemine hoş geldiniz.</p>
                <p>Hesap bilgileriniz:</p>
                <ul>
                    <li>E-posta: {user.Email}</li>
                    <li>Şifre: {password}</li>
                </ul>
                <p>İlk girişinizde şifrenizi değiştirmeniz önerilir.</p>";

            await SendEmailAsync(user.Email, subject, body);
        }

        public async Task SendReservationNotificationAsync(string toEmail, string classroomName, DateTime startTime, DateTime endTime, string purpose)
        {
            var subject = "New Reservation Created";
            var body = $"A new reservation has been created for {classroomName} from {startTime:g} to {endTime:g}.\n\nPurpose: {purpose}\n\nBest regards,\nClassroom System";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendFeedbackNotificationAsync(Feedback feedback)
        {
            var subject = "New Feedback Received";
            var body = $@"
                <h2>New Feedback Received</h2>
                <p>From: {feedback.User?.FirstName} {feedback.User?.LastName}</p>
                <p>Classroom: {feedback.Classroom?.Name}</p>
                <p>Rating: {feedback.Rating}/5</p>
                <p>Comment: {feedback.Comment}</p>";

            await SendEmailAsync(_fromEmail, subject, body);
        }
    }
} 