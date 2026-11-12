using BibliotekaIznajmljivanje.Api.Dtos;
using BibliotekaIznajmljivanje.Api.Services;

namespace BibliotekaIznajmljivanje.Api.EndPoints;

public static class KorisnikEndPoints
{
    public static void MapKorisnik(this WebApplication app)
    {
        var group = app.MapGroup("/korisnici");

        //PRIKAZ KORISNIKA
        group.MapGet("", async (IKorisnikService servis) =>
        {
            var listaKorisnika = await servis.PrikazSvihKorisnikaAsync();
            return Results.Ok(listaKorisnika);
        });

        //PRIKAZ KORISNIKA PO ID
        group.MapGet("/{id}", async (int id, IKorisnikService servis) =>
        {
            var pronadjenKorisnik = await servis.PrikazKorisnikaPoIdAsync(id);
            if (pronadjenKorisnik == null)
            {
                return Results.NotFound();
            }
            return Results.Ok(pronadjenKorisnik);

        });

        //KREIRANJE KORISNIKA
        group.MapPost("", async (KorisnikRequestDto noviDto, IKorisnikService servis) =>
        {
            var korisnikKreiran = await servis.KreiranjeKorisnikaAsync(noviDto);
            if (korisnikKreiran == null)
            {
                return Results.Conflict("Email vec postoji, izaberite neki drugi");
            }
            return Results.Created($"/korisnici/{korisnikKreiran.id}", korisnikKreiran);
        });

        //IZMENA KORISNIKA
        group.MapPut("/{id}", async (int id, KorisnikUpdateDto izmenjenKorisnik, IKorisnikService servis) =>
        {
            var pronadjenKorisnik = await servis.IzmenaKorisnikaPoIdAsync(id, izmenjenKorisnik);
            if (pronadjenKorisnik == false)
            {
                return Results.NotFound();
            }
            return Results.NoContent();

        });

        //BRISANJE KORISNIKA
        group.MapDelete("/{id}", async (int id, IKorisnikService servis) =>
        {
            var pronadjenKorisnik = await servis.BrisanjeKorisnikaPoIdAsync(id);
            if (pronadjenKorisnik == false)
            {
                return Results.NotFound();
            }
            return Results.NoContent();
        });


    }
}
