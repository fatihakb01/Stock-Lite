using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPeriodStartEndDateCheckConstraint2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Period_Start_End_Date",
                table: "SalesAggregates");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Period_Start_End_Date",
                table: "SalesAggregates",
                sql: "[PeriodStartDate] <= [PeriodEndDate]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Period_Start_End_Date",
                table: "SalesAggregates");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Period_Start_End_Date",
                table: "SalesAggregates",
                sql: "[PeriodStartDate] < [PeriodEndDate]");
        }
    }
}
