using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StockLevelMigration2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockLevel_Products_ProductId",
                table: "StockLevel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StockLevel",
                table: "StockLevel");

            migrationBuilder.RenameTable(
                name: "StockLevel",
                newName: "StockLevels");

            migrationBuilder.RenameIndex(
                name: "IX_StockLevel_ProductId",
                table: "StockLevels",
                newName: "IX_StockLevels_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StockLevels",
                table: "StockLevels",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StockLevels_Products_ProductId",
                table: "StockLevels",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockLevels_Products_ProductId",
                table: "StockLevels");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StockLevels",
                table: "StockLevels");

            migrationBuilder.RenameTable(
                name: "StockLevels",
                newName: "StockLevel");

            migrationBuilder.RenameIndex(
                name: "IX_StockLevels_ProductId",
                table: "StockLevel",
                newName: "IX_StockLevel_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StockLevel",
                table: "StockLevel",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StockLevel_Products_ProductId",
                table: "StockLevel",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
