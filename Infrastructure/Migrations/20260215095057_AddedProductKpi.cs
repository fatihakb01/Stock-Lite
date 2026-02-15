using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedProductKpi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductKpis",
                columns: table => new
                {
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchedulerFailed = table.Column<bool>(type: "bit", nullable: false),
                    TotalProductCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    BelowSafetyStockCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    RecommendedOrdersCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    WithoutStockAtLeadTimeCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    WithoutForecastCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductKpis", x => x.CompanyId);
                    table.ForeignKey(
                        name: "FK_ProductKpis_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductKpis");
        }
    }
}
