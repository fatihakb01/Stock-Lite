using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPeriodStartEndDateCheckConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Total_Revenue",
                table: "SalesAggregates");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Period_Start_End_Date",
                table: "SalesAggregates",
                sql: "[PeriodStartDate] < [PeriodEndDate]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Total_Revenue",
                table: "SalesAggregates",
                sql: "[TotalRevenue] >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Period_Start_End_Date",
                table: "SalesAggregates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Total_Revenue",
                table: "SalesAggregates");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Total_Revenue",
                table: "SalesAggregates",
                sql: "[PeriodStartDate] < [PeriodEndDate]");
        }
    }
}
