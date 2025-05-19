using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using ClassroomSystem.Models;
using ClassroomSystem.Services;
using Moq;

namespace ClassroomSystem.Tests
{
    public class ReservationTests : TestBase
    {
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IHolidayService> _holidayServiceMock;
        private readonly ReservationService _reservationService;

        public ReservationTests()
        {
            _emailServiceMock = new Mock<IEmailService>();
            _holidayServiceMock = new Mock<IHolidayService>();
            _reservationService = new ReservationService(Context, _emailServiceMock.Object, _holidayServiceMock.Object, LoggerMock.Object);
        }

        [Fact]
        public async Task CreateReservation_ValidRequest_Success()
        {
            // Arrange
            var instructor = new User { Id = "1", UserName = "test@example.com", Role = "Instructor" };
            var classroom = new Classroom { Id = 1, Name = "Room 101", Capacity = 30 };
            var term = new Term { Id = 1, StartDate = DateTime.Now, EndDate = DateTime.Now.AddMonths(3), IsActive = true };

            Context.Users.Add(instructor);
            Context.Classrooms.Add(classroom);
            Context.Terms.Add(term);
            await Context.SaveChangesAsync();

            var request = new ReservationRequest
            {
                ClassroomId = classroom.Id,
                InstructorId = instructor.Id,
                StartTime = DateTime.Now.AddDays(1),
                EndTime = DateTime.Now.AddDays(1).AddHours(2),
                Purpose = "Test Class",
                IsRecurring = false
            };

            // Act
            var result = await _reservationService.CreateReservationAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ReservationStatus.Pending, result.Status);
            Assert.Equal(request.Purpose, result.Purpose);
            Assert.Equal(request.InstructorId, result.InstructorId);
        }

        [Fact]
        public async Task CreateReservation_OutsideTerm_ThrowsException()
        {
            // Arrange
            var instructor = new User { Id = "1", UserName = "test@example.com", Role = "Instructor" };
            var classroom = new Classroom { Id = 1, Name = "Room 101", Capacity = 30 };
            var term = new Term { Id = 1, StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddMonths(3), IsActive = true };

            Context.Users.Add(instructor);
            Context.Classrooms.Add(classroom);
            Context.Terms.Add(term);
            await Context.SaveChangesAsync();

            var request = new ReservationRequest
            {
                ClassroomId = classroom.Id,
                InstructorId = instructor.Id,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(2),
                Purpose = "Test Class",
                IsRecurring = false
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _reservationService.CreateReservationAsync(request));
        }

        [Fact]
        public async Task CreateReservation_WithConflict_ThrowsException()
        {
            // Arrange
            var instructor = new User { Id = "1", UserName = "test@example.com", Role = "Instructor" };
            var classroom = new Classroom { Id = 1, Name = "Room 101", Capacity = 30 };
            var term = new Term { Id = 1, StartDate = DateTime.Now, EndDate = DateTime.Now.AddMonths(3), IsActive = true };

            var existingReservation = new Reservation
            {
                ClassroomId = classroom.Id,
                InstructorId = instructor.Id,
                StartTime = DateTime.Now.AddDays(1),
                EndTime = DateTime.Now.AddDays(1).AddHours(2),
                Purpose = "Existing Class",
                Status = ReservationStatus.Approved
            };

            Context.Users.Add(instructor);
            Context.Classrooms.Add(classroom);
            Context.Terms.Add(term);
            Context.Reservations.Add(existingReservation);
            await Context.SaveChangesAsync();

            var request = new ReservationRequest
            {
                ClassroomId = classroom.Id,
                InstructorId = instructor.Id,
                StartTime = DateTime.Now.AddDays(1),
                EndTime = DateTime.Now.AddDays(1).AddHours(2),
                Purpose = "Test Class",
                IsRecurring = false
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _reservationService.CreateReservationAsync(request));
        }

        [Fact]
        public async Task ApproveReservation_ValidRequest_Success()
        {
            // Arrange
            var instructor = new User { Id = "1", UserName = "test@example.com", Role = "Instructor" };
            var classroom = new Classroom { Id = 1, Name = "Room 101", Capacity = 30 };
            var reservation = new Reservation
            {
                ClassroomId = classroom.Id,
                InstructorId = instructor.Id,
                StartTime = DateTime.Now.AddDays(1),
                EndTime = DateTime.Now.AddDays(1).AddHours(2),
                Purpose = "Test Class",
                Status = ReservationStatus.Pending
            };

            Context.Users.Add(instructor);
            Context.Classrooms.Add(classroom);
            Context.Reservations.Add(reservation);
            await Context.SaveChangesAsync();

            // Act
            await _reservationService.ApproveReservationAsync(reservation.Id);

            // Assert
            var updatedReservation = await Context.Reservations.FindAsync(reservation.Id);
            Assert.Equal(ReservationStatus.Approved, updatedReservation.Status);
            _emailServiceMock.Verify(x => x.SendReservationApprovalEmailAsync(
                It.IsAny<string>(), It.IsAny<Reservation>()), Times.Once);
        }

        [Fact]
        public async Task RejectReservation_ValidRequest_Success()
        {
            // Arrange
            var instructor = new User { Id = "1", UserName = "test@example.com", Role = "Instructor" };
            var classroom = new Classroom { Id = 1, Name = "Room 101", Capacity = 30 };
            var reservation = new Reservation
            {
                ClassroomId = classroom.Id,
                InstructorId = instructor.Id,
                StartTime = DateTime.Now.AddDays(1),
                EndTime = DateTime.Now.AddDays(1).AddHours(2),
                Purpose = "Test Class",
                Status = ReservationStatus.Pending
            };

            Context.Users.Add(instructor);
            Context.Classrooms.Add(classroom);
            Context.Reservations.Add(reservation);
            await Context.SaveChangesAsync();

            var reason = "Room is under maintenance";

            // Act
            await _reservationService.RejectReservationAsync(reservation.Id, reason);

            // Assert
            var updatedReservation = await Context.Reservations.FindAsync(reservation.Id);
            Assert.Equal(ReservationStatus.Rejected, updatedReservation.Status);
            Assert.Equal(reason, updatedReservation.RejectionReason);
            _emailServiceMock.Verify(x => x.SendReservationRejectionEmailAsync(
                It.IsAny<string>(), It.IsAny<Reservation>()), Times.Once);
        }
    }
} 