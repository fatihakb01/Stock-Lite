using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class JobExecutionDeleteBehaviorCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobExecutions_ScheduledJobs_ScheduledJobId",
                table: "JobExecutions");

            migrationBuilder.AddForeignKey(
                name: "FK_JobExecutions_ScheduledJobs_ScheduledJobId",
                table: "JobExecutions",
                column: "ScheduledJobId",
                principalTable: "ScheduledJobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobExecutions_ScheduledJobs_ScheduledJobId",
                table: "JobExecutions");

            migrationBuilder.AddForeignKey(
                name: "FK_JobExecutions_ScheduledJobs_ScheduledJobId",
                table: "JobExecutions",
                column: "ScheduledJobId",
                principalTable: "ScheduledJobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
