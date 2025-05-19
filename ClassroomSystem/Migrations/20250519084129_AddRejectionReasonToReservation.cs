using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassroomSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddRejectionReasonToReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_AcademicTerms_TermId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Terms_TermId1",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPreferences_Users_UserId1",
                table: "UserPreferences");

            migrationBuilder.DropIndex(
                name: "IX_UserPreferences_UserId1",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "UserPreferences");

            migrationBuilder.RenameColumn(
                name: "TermId1",
                table: "Reservations",
                newName: "AcademicTermId");

            migrationBuilder.RenameColumn(
                name: "HasConflict",
                table: "Reservations",
                newName: "IsRecurring");

            migrationBuilder.RenameIndex(
                name: "IX_Reservations_TermId1",
                table: "Reservations",
                newName: "IX_Reservations_AcademicTermId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Terms",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Terms",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Purpose",
                table: "Reservations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Reservations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "RecurrenceEndDate",
                table: "Reservations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecurrencePattern",
                table: "Reservations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Reservations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_AcademicTerms_AcademicTermId",
                table: "Reservations",
                column: "AcademicTermId",
                principalTable: "AcademicTerms",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Terms_TermId",
                table: "Reservations",
                column: "TermId",
                principalTable: "Terms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_AcademicTerms_AcademicTermId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Terms_TermId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Terms");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "RecurrenceEndDate",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "RecurrencePattern",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Reservations");

            migrationBuilder.RenameColumn(
                name: "IsRecurring",
                table: "Reservations",
                newName: "HasConflict");

            migrationBuilder.RenameColumn(
                name: "AcademicTermId",
                table: "Reservations",
                newName: "TermId1");

            migrationBuilder.RenameIndex(
                name: "IX_Reservations_AcademicTermId",
                table: "Reservations",
                newName: "IX_Reservations_TermId1");

            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "UserPreferences",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Terms",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Purpose",
                table: "Reservations",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_UserId1",
                table: "UserPreferences",
                column: "UserId1",
                unique: true,
                filter: "[UserId1] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_AcademicTerms_TermId",
                table: "Reservations",
                column: "TermId",
                principalTable: "AcademicTerms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Terms_TermId1",
                table: "Reservations",
                column: "TermId1",
                principalTable: "Terms",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPreferences_Users_UserId1",
                table: "UserPreferences",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
