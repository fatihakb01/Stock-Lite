using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SchedulerLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobTaskKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AllowManualTrigger = table.Column<bool>(type: "bit", nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    CronExpression = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobTasks_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    JobType = table.Column<int>(type: "int", nullable: false),
                    CalculationType = table.Column<int>(type: "int", nullable: true),
                    ImportDataType = table.Column<int>(type: "int", nullable: true),
                    PeriodType = table.Column<int>(type: "int", nullable: true),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Target = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AllowManualTrigger = table.Column<bool>(type: "bit", nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    JobTaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExecutionOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledJobs", x => x.Id);
                    table.CheckConstraint("CK_ScheduledJob_Import_Requires_DataType_And_Source", "\r\n                (JobType <> 0) OR \r\n                (ImportDataType IS NOT NULL AND Source IS NOT NULL)\r\n                ");
                    table.CheckConstraint("CK_ScheduledJob_StandardCalculation_Requires_CalculationType", "\r\n                ([JobType] <> 1) OR ([CalculationType] IS NOT NULL)\r\n                ");
                    table.CheckConstraint("CK_ScheduledJob_Target_Only_For_Custom", "\r\n                ([JobType] = 2 AND [Target] IS NOT NULL)\r\n                OR ([JobType] <> 2 AND [Target] IS NULL)\r\n                ");
                    table.ForeignKey(
                        name: "FK_ScheduledJobs_JobTasks_JobTaskId",
                        column: x => x.JobTaskId,
                        principalTable: "JobTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobExecutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScheduledJobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FinishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobExecutions_ScheduledJobs_ScheduledJobId",
                        column: x => x.ScheduledJobId,
                        principalTable: "ScheduledJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobExecutions_ScheduledJobId_StartedAt",
                table: "JobExecutions",
                columns: new[] { "ScheduledJobId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_JobExecutions_ScheduledJobId_Status",
                table: "JobExecutions",
                columns: new[] { "ScheduledJobId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_JobTasks_CompanyId",
                table: "JobTasks",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_JobTasks_JobTaskKey",
                table: "JobTasks",
                column: "JobTaskKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledJobs_JobTaskId_ExecutionOrder",
                table: "ScheduledJobs",
                columns: new[] { "JobTaskId", "ExecutionOrder" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobExecutions");

            migrationBuilder.DropTable(
                name: "ScheduledJobs");

            migrationBuilder.DropTable(
                name: "JobTasks");
        }
    }
}
