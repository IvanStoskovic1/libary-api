using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibliotekaIznajmljivanje.Api.Migrations
{
    /// <inheritdoc />
    public partial class ZabraniCascadeBrisanje : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pozajmica_Knjige_KnjigaId",
                table: "Pozajmica");

            migrationBuilder.DropForeignKey(
                name: "FK_Pozajmica_Korisnici_KorisnikId",
                table: "Pozajmica");

            migrationBuilder.AddForeignKey(
                name: "FK_Pozajmica_Knjige_KnjigaId",
                table: "Pozajmica",
                column: "KnjigaId",
                principalTable: "Knjige",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pozajmica_Korisnici_KorisnikId",
                table: "Pozajmica",
                column: "KorisnikId",
                principalTable: "Korisnici",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pozajmica_Knjige_KnjigaId",
                table: "Pozajmica");

            migrationBuilder.DropForeignKey(
                name: "FK_Pozajmica_Korisnici_KorisnikId",
                table: "Pozajmica");

            migrationBuilder.AddForeignKey(
                name: "FK_Pozajmica_Knjige_KnjigaId",
                table: "Pozajmica",
                column: "KnjigaId",
                principalTable: "Knjige",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pozajmica_Korisnici_KorisnikId",
                table: "Pozajmica",
                column: "KorisnikId",
                principalTable: "Korisnici",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
