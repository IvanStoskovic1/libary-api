using BibliotekaIznajmljivanje.Api.Data;
using BibliotekaIznajmljivanje.Api.Dtos;
using BibliotekaIznajmljivanje.Api.Models;
using Microsoft.EntityFrameworkCore;
namespace BibliotekaIznajmljivanje.Api.Services;

public class KorisnikService : IKorisnikService
{
    private readonly BibliotekaDbContext _db;

    public KorisnikService(BibliotekaDbContext db)
    {
        _db = db;
    }

    //PRIKAZ KORISNIKA
    public async Task<List<KorisnikDto>> PrikazSvihKorisnikaAsync()
    {

        var listaKorisnika = await _db.Korisnici.ToListAsync();
        var prikazListe = listaKorisnika.Select(k => new KorisnikDto(
         k.Id,
         k.Ime,
         k.Prezime,
         k.Email,
         k.DatumRegistracije
        )).ToList();
        return prikazListe;
    }
    //PRIKAZ KORISNIKA PO ID
    public async Task<KorisnikDto?> PrikazKorisnikaPoIdAsync(int id)
    {
        var pronadjenKorisnik = await _db.Korisnici.FindAsync(id);
        if (pronadjenKorisnik == null)
        {
            return null;
        }
        var ispisKorisnika = new KorisnikDto(
            pronadjenKorisnik.Id,
            pronadjenKorisnik.Ime,
            pronadjenKorisnik.Prezime,
            pronadjenKorisnik.Email,
            pronadjenKorisnik.DatumRegistracije
        );
        return ispisKorisnika;
    }
    //KREIRANJE KORISNIKA
    public async Task<KorisnikDto?> KreiranjeKorisnikaAsync(KorisnikRequestDto noviDto)
    {
        var email = noviDto.Email.Trim().ToLowerInvariant();
        if (await _db.Korisnici.AnyAsync(k => k.Email == email))
        {
            return null;
        }
        var noviKorisnik = new Korisnik
        {
            Ime = noviDto.Ime,
            Prezime = noviDto.Prezime,
            Email = email,

        };
        _db.Korisnici.Add(noviKorisnik);
        await _db.SaveChangesAsync();
        var prikazNovogKorisnika = new KorisnikDto(
        noviKorisnik.Id,
        noviKorisnik.Ime,
        noviKorisnik.Prezime,
        noviKorisnik.Email,
        noviKorisnik.DatumRegistracije
        );
        return prikazNovogKorisnika;
    }
    //IZMENA KORISNIKA PO ID
    public async Task<bool> IzmenaKorisnikaPoIdAsync(int id, KorisnikUpdateDto izmenjenKorisnik)
    {
        var pronadjenKorisnik = await _db.Korisnici.FindAsync(id);
        if (pronadjenKorisnik == null)
        {
            return false;
        }
        pronadjenKorisnik.Ime = izmenjenKorisnik.Ime;
        pronadjenKorisnik.Prezime = izmenjenKorisnik.Prezime;
        await _db.SaveChangesAsync();
        return true;
    }
    //BRISANJE KORISNIKA PO ID
    public async Task<BrisanjeIshod> BrisanjeKorisnikaPoIdAsync(int id)
    {
        var pronadjenKorisnik = await _db.Korisnici.FindAsync(id);
        if (pronadjenKorisnik == null)
        {
            return BrisanjeIshod.NijeNadjena;
        }

        bool pronadjenKorisnikPozajmica = await _db.Pozajmica.AnyAsync(pronadjen => pronadjen.KorisnikId == pronadjenKorisnik.Id);
        if (pronadjenKorisnikPozajmica)
        {
            return BrisanjeIshod.ImaPozajmicu;
        }

        _db.Korisnici.Remove(pronadjenKorisnik);
        await _db.SaveChangesAsync();
        return BrisanjeIshod.Uspeh;


    }
}
