using BibliotekaIznajmljivanje.Api.Dtos;
using BibliotekaIznajmljivanje.Api.Services;

namespace BibliotekaIznajmljivanje.Api.EndPoints;

public static class PozajmicaEndPoints
{
    public static void MapPozajmica(this WebApplication app)
    {
        var group = app.MapGroup("/pozajmice");

        //POST PRAVLJENJE POZAJMICE
        group.MapPost("", async (IPozajmicaService servis, PozajmicaRequestDto noviDto) =>
        {
            var korisnikZaPozajmicu = await servis.PozajmljivanjeKnjigaAsync(noviDto);

            return korisnikZaPozajmicu.Ishod switch
            {
                PozajmicaIshod.NijeNadjenaKnjiga => Results.NotFound("Ne postoji izabrana knjiga"),
                PozajmicaIshod.NijeNadjenKorisnik => Results.NotFound("Ne postoji izabrani korisnik"),
                PozajmicaIshod.KnjigaPozajmljena => Results.Conflict("Knjiga je vec pozajmljena"),
                PozajmicaIshod.Kreirana => Results.Created($"/pozajmice/{korisnikZaPozajmicu.PozajmicaDto!.Id}", korisnikZaPozajmicu),

                _ => Results.StatusCode(500)
            };
        });

        //PUT VRACANJE KNJIGE
        group.MapPut("/{id}/vracanje", async (int id, IPozajmicaService servis) =>
        {
            var pozajmica = await servis.VracanjeKnjigeAsync(id);
            return pozajmica switch
            {
                PozajmicaIshod.NijeNadjena => Results.NotFound(),
                PozajmicaIshod.VecVracena => Results.Conflict("Knjiga je vec vracena sa pozajmice"),
                PozajmicaIshod.Uspeh => Results.NoContent(),
                _ => Results.StatusCode(500)
            };

        });

        // GET PRIKAZ SVIH REGISTRACIJA
        group.MapGet("", async (IPozajmicaService servis) =>
        {
            var listaRezervacija = await servis.PrikazSvihRegistracijaAsync();
            return Results.Ok(listaRezervacija);
        });

        // GET JEDNA POZAJMICA
        group.MapGet("/{id}", async (int id, IPozajmicaService servis) =>
        {
            var listaRezervacija = await servis.PrikazJednePozajmicePoIdAsync(id);
            if (listaRezervacija == null)
            {
                return Results.NotFound();
            }
            return Results.Ok(listaRezervacija);
        });

        // GET SVE POZAJMICE JEDNOG KORISNIKA
        group.MapGet("/korisnik/{id}", async (int id, IPozajmicaService servis) =>
        {
            var nadjenKorisnik = await servis.PrikazSvihPozajmicaJednogKorisnikaAsync(id);
            if (nadjenKorisnik == null)
            {
                return Results.NotFound();
            }
            return Results.Ok(nadjenKorisnik);
        });
        //GET POZAJMICE KOJE KASNE
        group.MapGet("/kasni", async (IPozajmicaService servis) =>
        {
            var listaKasnjenja = await servis.PrikazPozajmicaKojeKasneAsync();
            return Results.Ok(listaKasnjenja);

        });


    }
}