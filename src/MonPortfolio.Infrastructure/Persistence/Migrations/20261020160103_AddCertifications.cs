using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonPortfolio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCertifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Certifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    Issuer = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    IssuedOn = table.Column<DateOnly>(type: "date", nullable: true),
                    CredentialId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CredentialUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certifications", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Certifications_DisplayOrder",
                table: "Certifications",
                column: "DisplayOrder");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Certifications");
        }
    }
}
