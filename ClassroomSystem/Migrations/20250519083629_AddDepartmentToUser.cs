using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassroomSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Building",
                table: "Classrooms",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Features",
                table: "Classrooms",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Floor",
                table: "Classrooms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Classrooms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RoomNumber",
                table: "Classrooms",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Department",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Building",
                table: "Classrooms");

            migrationBuilder.DropColumn(
                name: "Features",
                table: "Classrooms");

            migrationBuilder.DropColumn(
                name: "Floor",
                table: "Classrooms");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Classrooms");

            migrationBuilder.DropColumn(
                name: "RoomNumber",
                table: "Classrooms");
        }
    }
}
