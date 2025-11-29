using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSalesAggregatesId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SalesAggregates",
                table: "SalesAggregates");

            migrationBuilder.DropIndex(
                name: "IX_SalesAggregates_ProductId",
                table: "SalesAggregates");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "SalesAggregates");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SalesAggregates",
                table: "SalesAggregates",
                columns: new[] { "ProductId", "PeriodType", "PeriodStartDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SalesAggregates",
                table: "SalesAggregates");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "SalesAggregates",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_SalesAggregates",
                table: "SalesAggregates",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_SalesAggregates_ProductId",
                table: "SalesAggregates",
                column: "ProductId");
        }
    }
}
