using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PurchaseForecast : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "ProjectedStockAtLeadTime",
                table: "StockLevels",
                type: "decimal(15,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "NetStockPosition",
                table: "StockLevels",
                type: "decimal(15,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<decimal>(
                name: "ForecastQuantity",
                table: "StockLevels",
                type: "decimal(15,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "PurchaseForecasts",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WeekStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WeekEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WeekIndex = table.Column<int>(type: "int", nullable: false),
                    ForecastQuantity = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    QuantityCommitted = table.Column<int>(type: "int", nullable: false),
                    QuantityOnOrder = table.Column<int>(type: "int", nullable: false),
                    ProjectedStock = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    NeedsReorder = table.Column<bool>(type: "bit", nullable: false),
                    RecommendedOrderQuantity = table.Column<int>(type: "int", nullable: false),
                    RecommendedOrderDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CalculatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseForecasts", x => new { x.ProductId, x.WeekStartDate });
                    table.CheckConstraint("CK_PurchaseForecast_ForecastQuantity", "[ForecastQuantity] >= 0");
                    table.CheckConstraint("CK_PurchaseForecast_QuantityCommitted", "[QuantityCommitted] >= 0");
                    table.CheckConstraint("CK_PurchaseForecast_QuantityOnOrder", "[QuantityOnOrder] >= 0");
                    table.CheckConstraint("CK_PurchaseForecast_RecommendedOrderQuantity", "[RecommendedOrderQuantity] >= 0");
                    table.ForeignKey(
                        name: "FK_PurchaseForecasts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_StockLevel_ForecastQuantity",
                table: "StockLevels",
                sql: "[ForecastQuantity] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_StockLevel_QuantityCommitted",
                table: "StockLevels",
                sql: "[QuantityCommitted] >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PurchaseForecasts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_StockLevel_ForecastQuantity",
                table: "StockLevels");

            migrationBuilder.DropCheckConstraint(
                name: "CK_StockLevel_QuantityCommitted",
                table: "StockLevels");

            migrationBuilder.DropColumn(
                name: "ForecastQuantity",
                table: "StockLevels");

            migrationBuilder.AlterColumn<int>(
                name: "ProjectedStockAtLeadTime",
                table: "StockLevels",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(15,2)");

            migrationBuilder.AlterColumn<int>(
                name: "NetStockPosition",
                table: "StockLevels",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(15,2)");
        }
    }
}
