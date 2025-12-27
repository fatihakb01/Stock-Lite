using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfirmedOrderAggregates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Period_Start_End_Date1",
                table: "SalesAggregates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Total_Quantity",
                table: "SalesAggregates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Confidence",
                table: "Forecasts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Period_Start_End_Date",
                table: "Forecasts");

            migrationBuilder.CreateTable(
                name: "ConfirmedOrderAggregates",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PeriodType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PeriodStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalConfirmedQuantity = table.Column<int>(type: "int", nullable: false),
                    TotalConfirmedRevenue = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfirmedOrderAggregates", x => new { x.ProductId, x.PeriodType, x.PeriodStartDate });
                    table.CheckConstraint("CK_ConfirmedOrderAggregate_Period_Start_End_Date", "[PeriodStartDate] <= [PeriodEndDate]");
                    table.CheckConstraint("CK_ConfirmedOrderAggregate_Total_Quantity", "[TotalConfirmedQuantity] >= 0");
                    table.ForeignKey(
                        name: "FK_ConfirmedOrderAggregates_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_SalesAggregate_Period_Start_End_Date",
                table: "SalesAggregates",
                sql: "[PeriodStartDate] <= [PeriodEndDate]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SalesAggregate_Total_Quantity",
                table: "SalesAggregates",
                sql: "[TotalQuantity] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Forecast_Confidence",
                table: "Forecasts",
                sql: "[Confidence] > 0 AND [Confidence] < 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Forecast_Period_Start_End_Date",
                table: "Forecasts",
                sql: "[PeriodStartDate] <= [PeriodEndDate]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfirmedOrderAggregates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SalesAggregate_Period_Start_End_Date",
                table: "SalesAggregates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SalesAggregate_Total_Quantity",
                table: "SalesAggregates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Forecast_Confidence",
                table: "Forecasts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Forecast_Period_Start_End_Date",
                table: "Forecasts");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Period_Start_End_Date1",
                table: "SalesAggregates",
                sql: "[PeriodStartDate] <= [PeriodEndDate]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Total_Quantity",
                table: "SalesAggregates",
                sql: "[TotalQuantity] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Confidence",
                table: "Forecasts",
                sql: "[Confidence] > 0 AND [Confidence] < 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Period_Start_End_Date",
                table: "Forecasts",
                sql: "[PeriodStartDate] <= [PeriodEndDate]");
        }
    }
}
