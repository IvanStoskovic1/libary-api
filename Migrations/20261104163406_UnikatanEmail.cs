using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibliotekaIznajmljivanje.Api.Migrations
{
    /// <inheritdoc />
    public partial class UnikatanEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Korisnici_Email",
                table: "Korisnici",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Korisnici_Email",
                table: "Korisnici");
        }
    }
}
