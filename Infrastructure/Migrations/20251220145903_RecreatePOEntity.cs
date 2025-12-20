using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RecreatePOEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderLineItem_PurchaseOrder_PurchaseOrderId",
                table: "PurchaseOrderLineItem");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderLineItem_PurchaseOrder_PurchaseOrderId",
                table: "PurchaseOrderLineItem",
                column: "PurchaseOrderId",
                principalTable: "PurchaseOrder",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderLineItem_PurchaseOrder_PurchaseOrderId",
                table: "PurchaseOrderLineItem");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderLineItem_PurchaseOrder_PurchaseOrderId",
                table: "PurchaseOrderLineItem",
                column: "PurchaseOrderId",
                principalTable: "PurchaseOrder",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
