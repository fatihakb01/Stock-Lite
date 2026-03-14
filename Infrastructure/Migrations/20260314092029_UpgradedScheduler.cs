using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpgradedScheduler : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRunning",
                table: "JobTasks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastStartedAt",
                table: "JobTasks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxRetries",
                table: "JobTasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextRunAt",
                table: "JobTasks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PeriodType",
                table: "JobTasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Result",
                table: "JobTasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Retries",
                table: "JobTasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RetryDelayMinutes",
                table: "JobTasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledAt",
                table: "JobTasks",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "JobTaskExecutionId",
                table: "JobExecutions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "JobTaskExecutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobTaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TriggerType = table.Column<int>(type: "int", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FinishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobTaskExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobTaskExecutions_JobTasks_JobTaskId",
                        column: x => x.JobTaskId,
                        principalTable: "JobTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobExecutions_JobTaskExecutionId",
                table: "JobExecutions",
                column: "JobTaskExecutionId");

            migrationBuilder.CreateIndex(
                name: "IX_JobTaskExecutions_JobTaskId_StartedAt",
                table: "JobTaskExecutions",
                columns: new[] { "JobTaskId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_JobTaskExecutions_JobTaskId_Status",
                table: "JobTaskExecutions",
                columns: new[] { "JobTaskId", "Status" });

            migrationBuilder.AddForeignKey(
                name: "FK_JobExecutions_JobTaskExecutions_JobTaskExecutionId",
                table: "JobExecutions",
                column: "JobTaskExecutionId",
                principalTable: "JobTaskExecutions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobExecutions_JobTaskExecutions_JobTaskExecutionId",
                table: "JobExecutions");

            migrationBuilder.DropTable(
                name: "JobTaskExecutions");

            migrationBuilder.DropIndex(
                name: "IX_JobExecutions_JobTaskExecutionId",
                table: "JobExecutions");

            migrationBuilder.DropColumn(
                name: "IsRunning",
                table: "JobTasks");

            migrationBuilder.DropColumn(
                name: "LastStartedAt",
                table: "JobTasks");

            migrationBuilder.DropColumn(
                name: "MaxRetries",
                table: "JobTasks");

            migrationBuilder.DropColumn(
                name: "NextRunAt",
                table: "JobTasks");

            migrationBuilder.DropColumn(
                name: "PeriodType",
                table: "JobTasks");

            migrationBuilder.DropColumn(
                name: "Result",
                table: "JobTasks");

            migrationBuilder.DropColumn(
                name: "Retries",
                table: "JobTasks");

            migrationBuilder.DropColumn(
                name: "RetryDelayMinutes",
                table: "JobTasks");

            migrationBuilder.DropColumn(
                name: "ScheduledAt",
                table: "JobTasks");

            migrationBuilder.DropColumn(
                name: "JobTaskExecutionId",
                table: "JobExecutions");
        }
    }
}
