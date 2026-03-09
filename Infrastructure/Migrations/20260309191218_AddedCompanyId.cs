using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedCompanyId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserDefinedFields",
                table: "UserDefinedFields");

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "UserDefinedFields",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserDefinedFields",
                table: "UserDefinedFields",
                columns: new[] { "ProductId", "CompanyId", "Key" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserDefinedFields",
                table: "UserDefinedFields");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "UserDefinedFields");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserDefinedFields",
                table: "UserDefinedFields",
                columns: new[] { "ProductId", "Key" });
        }
    }
}
