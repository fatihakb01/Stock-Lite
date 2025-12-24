using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NewTransactionMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransactionLineItems_Transactions_TransactionId",
                table: "TransactionLineItems");

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionLineItems_Transactions_TransactionId",
                table: "TransactionLineItems",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransactionLineItems_Transactions_TransactionId",
                table: "TransactionLineItems");

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionLineItems_Transactions_TransactionId",
                table: "TransactionLineItems",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "Id");
        }
    }
}
