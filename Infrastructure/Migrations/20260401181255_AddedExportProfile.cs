using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedExportProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExportProfile",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ExportType = table.Column<int>(type: "int", nullable: true),
                    CustomType = table.Column<int>(type: "int", nullable: true),
                    CustomSource = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExportFileType = table.Column<int>(type: "int", nullable: false),
                    TargetPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExportProfile", x => x.Id);
                    table.CheckConstraint("CK_ExportProfile_Custom_Source_And_Type", "\r\n                    ([CustomSource] IS NULL AND [CustomType] IS NULL)\r\n                    OR ([CustomSource] IS NOT NULL AND [CustomType] IS NOT NULL)\r\n                    ");
                    table.CheckConstraint("CK_ExportProfile_Custom_Source_Or_Export_Type", "\r\n                    ([CustomSource] IS NULL AND [ExportType] IS NOT NULL)\r\n                    OR ([CustomSource] IS NOT NULL AND [ExportType] IS NULL)\r\n                    ");
                    table.ForeignKey(
                        name: "FK_ExportProfile_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExportProfile_CompanyId_Id",
                table: "ExportProfile",
                columns: new[] { "CompanyId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_ExportProfile_CompanyId_Name",
                table: "ExportProfile",
                columns: new[] { "CompanyId", "Name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExportProfile");
        }
    }
}
