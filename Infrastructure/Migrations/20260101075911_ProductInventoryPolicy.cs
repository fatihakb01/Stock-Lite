using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProductInventoryPolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ProductInventoryPolicy_LeadTimeDays",
                table: "ProductInventoryPolicies");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProductInventoryPolicy_LeadTimeDays",
                table: "ProductInventoryPolicies",
                sql: "[LeadTimeDays] BETWEEN 1 AND 365");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ProductInventoryPolicy_LeadTimeDays",
                table: "ProductInventoryPolicies");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProductInventoryPolicy_LeadTimeDays",
                table: "ProductInventoryPolicies",
                sql: "[LeadTimeDays] > 0");
        }
    }
}
