using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CK_PurchaseOrderLineItem_ReceivedQuantity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_PurchaseOrderLineItem_ReceivedQuantity",
                table: "PurchaseOrderLineItem",
                sql: "[ReceivedQuantity] >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_PurchaseOrderLineItem_ReceivedQuantity",
                table: "PurchaseOrderLineItem");
        }
    }
}
