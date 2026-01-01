using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StockLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ReorderPoint",
                table: "StockLevels",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "QuantityOnOrder",
                table: "StockLevels",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "QuantityOnHand",
                table: "StockLevels",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastCalculatedAt",
                table: "StockLevels",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "NeedsReorder",
                table: "StockLevels",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "NetStockPosition",
                table: "StockLevels",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProjectedStockAtLeadTime",
                table: "StockLevels",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuantityCommitted",
                table: "StockLevels",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RecommendedOrderQuantity",
                table: "StockLevels",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ProductInventoryPolicies",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeadTimeDays = table.Column<int>(type: "int", nullable: false),
                    MinimumOrderQuantity = table.Column<int>(type: "int", nullable: false),
                    OrderIncrementQuantity = table.Column<int>(type: "int", nullable: false),
                    SafetyStock = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    StockAdjustment = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductInventoryPolicies", x => x.ProductId);
                    table.CheckConstraint("CK_ProductInventoryPolicy_LeadTimeDays", "[LeadTimeDays] > 0");
                    table.CheckConstraint("CK_ProductInventoryPolicy_MinimumOrderQuantity", "[MinimumOrderQuantity] > 0");
                    table.CheckConstraint("CK_ProductInventoryPolicy_OrderIncrementQuantity", "[OrderIncrementQuantity] > 0");
                    table.ForeignKey(
                        name: "FK_ProductInventoryPolicies_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_StockLevel_RecommendedOrderQuantity",
                table: "StockLevels",
                sql: "[RecommendedOrderQuantity] >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductInventoryPolicies");

            migrationBuilder.DropCheckConstraint(
                name: "CK_StockLevel_RecommendedOrderQuantity",
                table: "StockLevels");

            migrationBuilder.DropColumn(
                name: "LastCalculatedAt",
                table: "StockLevels");

            migrationBuilder.DropColumn(
                name: "NeedsReorder",
                table: "StockLevels");

            migrationBuilder.DropColumn(
                name: "NetStockPosition",
                table: "StockLevels");

            migrationBuilder.DropColumn(
                name: "ProjectedStockAtLeadTime",
                table: "StockLevels");

            migrationBuilder.DropColumn(
                name: "QuantityCommitted",
                table: "StockLevels");

            migrationBuilder.DropColumn(
                name: "RecommendedOrderQuantity",
                table: "StockLevels");

            migrationBuilder.AlterColumn<int>(
                name: "ReorderPoint",
                table: "StockLevels",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "QuantityOnOrder",
                table: "StockLevels",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "QuantityOnHand",
                table: "StockLevels",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
