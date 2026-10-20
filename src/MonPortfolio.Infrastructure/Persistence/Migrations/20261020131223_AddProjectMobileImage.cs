using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonPortfolio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectMobileImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MobileImageUrl",
                table: "Projects",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MobileImageUrl",
                table: "Projects");
        }
    }
}
