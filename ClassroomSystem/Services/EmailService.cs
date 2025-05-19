using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using ClassroomSystem.Models;
using Microsoft.EntityFrameworkCore;
using ClassroomSystem.Data;

namespace ClassroomSystem.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly ApplicationDbContext _context;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration, ApplicationDbContext context)
        {
            _logger = logger;
            _configuration = configuration;
            _smtpServer = _configuration["EmailSettings:SmtpServer"];
            _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
            _smtpUsername = _configuration["EmailSettings:SmtpUsername"];
            _smtpPassword = _configuration["EmailSettings:SmtpPassword"];
            _fromEmail = _configuration["EmailSettings:FromEmail"];
            _fromName = _configuration["EmailSettings:FromName"];
            _context = context;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                using var client = new SmtpClient(_smtpServer, _smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(_smtpUsername, _smtpPassword)
                };

                using var message = new MailMessage
                {
                    From = new MailAddress(_fromEmail, _fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                message.To.Add(to);

                await client.SendMailAsync(message);
                _logger.LogInformation($"Email sent successfully to {to}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending email to {to}");
                throw;
            }
        }

        public async Task SendReservationApprovalEmailAsync(string toEmail, string userName, string classroomName, string dayOfWeek, string timeSlot)
        {
            var subject = "Reservation Approved";
            var body = $@"
                <h2>Reservation Approved</h2>
                <p>Dear {userName},</p>
                <p>Your reservation request has been approved:</p>
                <ul>
                    <li>Classroom: {classroomName}</li>
                    <li>Day: {dayOfWeek}</li>
                    <li>Time: {timeSlot}</li>
                </ul>
                <p>Thank you for using our Classroom Reservation System.</p>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendReservationRejectionEmailAsync(string toEmail, string userName, string classroomName, string dayOfWeek, string timeSlot, string reason)
        {
            var subject = "Reservation Rejected";
            var body = $@"
                <h2>Reservation Rejected</h2>
                <p>Dear {userName},</p>
                <p>Your reservation request has been rejected:</p>
                <ul>
                    <li>Classroom: {classroomName}</li>
                    <li>Day: {dayOfWeek}</li>
                    <li>Time: {timeSlot}</li>
                    <li>Reason: {reason}</li>
                </ul>
                <p>Please contact the administrator if you have any questions.</p>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendHolidayWarningEmailAsync(string toEmail, string userName, string classroomName, string date, string timeSlot)
        {
            var subject = "Holiday Warning";
            var body = $@"
                <h2>Holiday Warning</h2>
                <p>Dear {userName},</p>
                <p>Your reservation falls on a public holiday:</p>
                <ul>
                    <li>Classroom: {classroomName}</li>
                    <li>Date: {date}</li>
                    <li>Time: {timeSlot}</li>
                </ul>
                <p>Please consider rescheduling your reservation.</p>";

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
            var subject = "New Reservation Request";
            var body = $@"
                <h2>New Reservation Request</h2>
                <p>A new reservation request has been submitted:</p>
                <ul>
                    <li>Classroom: {classroomName}</li>
                    <li>Date: {startTime.ToShortDateString()}</li>
                    <li>Time: {startTime.ToShortTimeString()} - {endTime.ToShortTimeString()}</li>
                    <li>Purpose: {purpose}</li>
                </ul>
                <p>Please review and take appropriate action.</p>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendFeedbackNotificationAsync(Feedback feedback)
        {
            var user = await _context.Users.FindAsync(feedback.UserId);
            if (user == null) return;

            var emailBody = $@"
                <h2>New Feedback Received</h2>
                <p>From: {user.FirstName} {user.LastName}</p>
                <p>Classroom: {feedback.Classroom.Name}</p>
                <p>Rating: {feedback.Rating}/5</p>
                <p>Comment: {feedback.Comment}</p>";

            await SendEmailAsync(_fromEmail, "New Feedback Received", emailBody);
        }
    }
} 