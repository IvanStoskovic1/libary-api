using BibliotekaIznajmljivanje.Api.Dtos;
using BibliotekaIznajmljivanje.Api.Services;


namespace BibliotekaIznajmljivanje.Api.EndPoints;

public static class KnjigaEndPoints
{
    public static void MapKnjige(this WebApplication app)
    {
        var group = app.MapGroup("/knjige");

        //PRIKAZ SVIH KNJIGA
        group.MapGet("", async (IKnjigaService servis, string? search, int page = 1, int pageSize = 10) =>
        {

            var prikazKnjiga = await servis.PrikaziSveKnjigeAsync(search, page, pageSize);
            return Results.Ok(prikazKnjiga);
        });

        //DODAVANJE KNJIGE
        group.MapPost("", async (KnjigaRequestDto createDto, IKnjigaService servis) =>
        {
            var novaKnjiga = await servis.DodajKnjiguAsync(createDto);

            return Results.Created($"/knjige/{novaKnjiga.Id}", novaKnjiga);
        });

        //IZMENI KNJIGU
        group.MapPut("/{id}", async (int id, KnjigaRequestDto izmeniDto, IKnjigaService servis) =>
        {
            var pronadjenaKnjiga = await servis.IzmenaKnjigaPoIdAsync(id, izmeniDto);
            if (pronadjenaKnjiga == false)
            {
                return Results.NotFound();
            }

            return Results.NoContent();

        });

        //IZBRISI KNJIGU
        group.MapDelete("/{id}", async (IKnjigaService servis, int id) =>
        {
            var pronadjenaKnjigaZaBrisanje = await servis.IzbrisiKnjiguPoIdAsync(id);
           
         return pronadjenaKnjigaZaBrisanje switch
            {
                BrisanjeIshod.NijeNadjena=>Results.NotFound(),
                BrisanjeIshod.ImaPozajmicu=>Results.Conflict("Ne moze knjiga da se obrise jer je vec pozajmljena"),
                BrisanjeIshod.Uspeh=>Results.NoContent(),
            _ => Results.StatusCode(500)
            };

           /*
            if (pronadjenaKnjigaZaBrisanje != true)
            {
                return Results.NotFound();
            }

            return Results.NoContent();
*/
        });

        //PRETRAZI KNJIGU PO ID
        group.MapGet("/{id}", async (int id, IKnjigaService servis) =>
        {
            var rezultat = await servis.DajKnjiguPoIdAsync(id);
            if (rezultat == null)
            {
                return Results.NotFound();
            }
            return Results.Ok(rezultat);

        });
    }

}
