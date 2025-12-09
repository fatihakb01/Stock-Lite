using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ForecastEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Period_Start_End_Date",
                table: "SalesAggregates");

            migrationBuilder.CreateTable(
                name: "Forecasts",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PeriodType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PeriodStartDate = table.Column<DateTime>(type: "date", nullable: false),
                    PeriodEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PredictedQuantity = table.Column<int>(type: "int", nullable: false),
                    Confidence = table.Column<decimal>(type: "decimal(3,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Forecasts", x => new { x.ProductId, x.PeriodType, x.PeriodStartDate });
                    table.CheckConstraint("CK_Confidence", "[Confidence] > 0 AND [Confidence] < 1");
                    table.CheckConstraint("CK_Period_Start_End_Date", "[PeriodStartDate] <= [PeriodEndDate]");
                    table.ForeignKey(
                        name: "FK_Forecasts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Period_Start_End_Date1",
                table: "SalesAggregates",
                sql: "[PeriodStartDate] <= [PeriodEndDate]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Forecasts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Period_Start_End_Date1",
                table: "SalesAggregates");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Period_Start_End_Date",
                table: "SalesAggregates",
                sql: "[PeriodStartDate] <= [PeriodEndDate]");
        }
    }
}
