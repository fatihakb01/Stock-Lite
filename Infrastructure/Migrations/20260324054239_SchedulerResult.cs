using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SchedulerResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SchedulerFailed",
                table: "ProductKpis");

            migrationBuilder.DropColumn(
                name: "IsRunning",
                table: "JobTasks");

            migrationBuilder.AddColumn<int>(
                name: "SchedulerStatus",
                table: "ProductKpis",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SchedulerStatus",
                table: "ProductKpis");

            migrationBuilder.AddColumn<bool>(
                name: "SchedulerFailed",
                table: "ProductKpis",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRunning",
                table: "JobTasks",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
