using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class JobTaskKeyUniquenessPerCompanyId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobTasks_JobTaskKey",
                table: "JobTasks");

            migrationBuilder.CreateIndex(
                name: "IX_JobTasks_JobTaskKey_CompanyId",
                table: "JobTasks",
                columns: new[] { "JobTaskKey", "CompanyId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobTasks_JobTaskKey_CompanyId",
                table: "JobTasks");

            migrationBuilder.CreateIndex(
                name: "IX_JobTasks_JobTaskKey",
                table: "JobTasks",
                column: "JobTaskKey",
                unique: true);
        }
    }
}
