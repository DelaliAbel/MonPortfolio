using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonPortfolio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BilingualContent : Migration
    {
        /// <summary>Colonnes de texte devenues bilingues : {"fr":"…","en":"…"}.</summary>
        private static readonly (string Table, string Column)[] TextColumns =
        [
            ("Projects", "Title"), ("Projects", "Summary"), ("Projects", "Description"), ("Projects", "Category"),
            ("Skills", "Name"),
            ("Experiences", "Title"), ("Experiences", "Description"),
            ("CommunityActivities", "Title"), ("CommunityActivities", "Description")
        ];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Colonnes élargies d'abord (le JSON est plus long que le texte), données converties à la fin.
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Skills",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(60)",
                oldMaxLength: 60);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(120)",
                oldMaxLength: 120);

            migrationBuilder.AlterColumn<string>(
                name: "Summary",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(60)",
                oldMaxLength: 60);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Experiences",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(120)",
                oldMaxLength: 120);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Experiences",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "CommunityActivities",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "CommunityActivities",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            // Le contenu existant devient sa version française. STRING_ESCAPE : SQL Server 2016 et suivants.
            foreach (var (table, column) in TextColumns)
            {
                migrationBuilder.Sql(
                    $"UPDATE [{table}] SET [{column}] = CONCAT(N'{{\"fr\":\"', STRING_ESCAPE([{column}], 'json'), N'\"}}') " +
                    $"WHERE [{column}] IS NOT NULL AND [{column}] NOT LIKE N'{{%';");
            }

            // Réalisations : tableau JSON français ["…"] → {"fr":["…"]}.
            migrationBuilder.Sql(
                "UPDATE [Experiences] SET [Highlights] = CONCAT(N'{\"fr\":', [Highlights], N'}') WHERE [Highlights] LIKE N'[[]%';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Retour au texte seul (la version française), avant de réduire les colonnes.
            foreach (var (table, column) in TextColumns)
                migrationBuilder.Sql($"UPDATE [{table}] SET [{column}] = JSON_VALUE([{column}], '$.fr') WHERE ISJSON([{column}]) = 1;");

            migrationBuilder.Sql(
                "UPDATE [Experiences] SET [Highlights] = JSON_QUERY([Highlights], '$.fr') " +
                "WHERE ISJSON([Highlights]) = 1 AND JSON_QUERY([Highlights], '$.fr') IS NOT NULL;");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Skills",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Projects",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Summary",
                table: "Projects",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Projects",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Projects",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Experiences",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Experiences",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "CommunityActivities",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "CommunityActivities",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
