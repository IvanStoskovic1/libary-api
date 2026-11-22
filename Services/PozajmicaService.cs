using System;
using BibliotekaIznajmljivanje.Api.Data;
using BibliotekaIznajmljivanje.Api.Dtos;
using BibliotekaIznajmljivanje.Api.Models;
using Microsoft.EntityFrameworkCore;
namespace BibliotekaIznajmljivanje.Api.Services;

public class PozajmicaService : IPozajmicaService
{
    private readonly BibliotekaDbContext _db;
    public PozajmicaService(BibliotekaDbContext db)
    {
        _db = db;
    }
    //VRACANJE POZAJMLJENIH KNJIGA
    public async Task<PozajmicaIshod> VracanjeKnjigeAsync(int id)
    {
        var pozajmica = await _db.Pozajmica.FindAsync(id);
        if (pozajmica == null)
        {
            return PozajmicaIshod.NijeNadjena;
        }
        
        if (pozajmica.DatumVracanja is not null)
        {
            return PozajmicaIshod.VecVracena;
        }
        pozajmica.DatumVracanja = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return PozajmicaIshod.Uspeh;
    }

    //PRIKAZ SVIH POZAJMICA
    public async Task<List<PozajmicaDto>> PrikazSvihPozajmicaAsync()
    {
        var listaRezervacija = await _db.Pozajmica.AsNoTracking().Include(i => i.Korisnik)
                                            .Include(i => i.Knjiga)
                                            .ToListAsync();
        var lista = listaRezervacija.Select(k => new PozajmicaDto(
            k.Id,
            k.KorisnikId,
            k.Korisnik.Ime,
            k.KnjigaId,
            k.Knjiga.NaslovKnjige,
            k.DatumPreuzimanja,
            k.RokVracanja,
            k.DatumVracanja

        )).ToList();
        return lista;
    }

    //PRIKAZ JEDNE POZAJMICE PO ID
    public async Task<PozajmicaDto?> PrikazJednePozajmicePoIdAsync(int id)
    {
        var listaRezervacija = await _db.Pozajmica.AsNoTracking().Include(k => k.Korisnik)
                                                .Include(k => k.Knjiga)
                                                .FirstOrDefaultAsync(k => k.Id == id);
        if (listaRezervacija == null)
        {
            return null;
        }
        var ispis = new PozajmicaDto(
            listaRezervacija.Id,
            listaRezervacija.KorisnikId,
            listaRezervacija.Korisnik.Ime,
            listaRezervacija.KnjigaId,
            listaRezervacija.Knjiga.NaslovKnjige,
            listaRezervacija.DatumPreuzimanja,
            listaRezervacija.RokVracanja,
            listaRezervacija.DatumVracanja
        );
        return ispis;
    }

    //PRIKAZ SVIH POZAJMICA OD JEDNOG KORISNIKA
    public async Task<List<PozajmicaDto>?> PrikazSvihPozajmicaJednogKorisnikaAsync(int id)
    {

        bool nadjenKorisnik = await _db.Korisnici.AnyAsync(k => k.Id == id);
        if (!nadjenKorisnik)
        {
            return null;
        }
        var pozajmiceKorisnika = await _db.Pozajmica.AsNoTracking().Include(k => k.Knjiga).Include(k => k.Korisnik).Where(k => k.KorisnikId == id).ToListAsync();
        var ispis = pozajmiceKorisnika.Select(k => new PozajmicaDto(
            k.Id,
            k.KorisnikId,
            k.Korisnik.Ime,
            k.KnjigaId,
            k.Knjiga.NaslovKnjige,
            k.DatumPreuzimanja,
            k.RokVracanja,
            k.DatumVracanja
        )).ToList();
        return ispis;
    }

    //POZAJMICE KOJE KASNE
    public async Task<List<PozajmicaDto>> PrikazPozajmicaKojeKasneAsync()
    {
        var listaKasnjenja = await _db.Pozajmica.AsNoTracking().Include(k => k.Korisnik)
                                            .Include(k => k.Knjiga)
                                            .Where(k => k.DatumVracanja == null
                                             && DateTime.UtcNow > k.RokVracanja).ToListAsync();
        var ispisKasnjenja = listaKasnjenja.Select(k => new PozajmicaDto(
            k.Id,
            k.KorisnikId,
            k.Korisnik.Ime,
            k.KnjigaId,
            k.Knjiga.NaslovKnjige,
            k.DatumPreuzimanja,
            k.RokVracanja,
            k.DatumVracanja
        )).ToList();
        return ispisKasnjenja;
    }

    //POZAJMLJIVANJE KNJIGA
    public async Task<PozajmicaRezultat> PozajmljivanjeKnjigaAsync(PozajmicaRequestDto noviDto)
    {
          var korisnik = await _db.Korisnici.FindAsync(noviDto.KorisnikId);
            if (korisnik == null)
            {
                return new PozajmicaRezultat(PozajmicaIshod.NijeNadjenKorisnik,null);
            }
            var knjiga = await _db.Knjige.FindAsync(noviDto.KnjigaId);
            if (knjiga == null)
            {
                return new PozajmicaRezultat(PozajmicaIshod.NijeNadjenaKnjiga,null);
            }
            if (await _db.Pozajmica.AnyAsync(k => k.DatumVracanja == null && k.KnjigaId == noviDto.KnjigaId))
            {
                return new PozajmicaRezultat(PozajmicaIshod.KnjigaPozajmljena,null);
            }
            var novaPozajmica = new Pozajmica
            {
                KorisnikId = noviDto.KorisnikId,
                KnjigaId = noviDto.KnjigaId,
            };
            _db.Pozajmica.Add(novaPozajmica);
            await _db.SaveChangesAsync();
            var ispisNovePozajmice = new PozajmicaDto(
                novaPozajmica.Id,
                novaPozajmica.KorisnikId,
                korisnik.Ime,
                novaPozajmica.KnjigaId,
                knjiga.NaslovKnjige,
                novaPozajmica.DatumPreuzimanja,
                novaPozajmica.RokVracanja,
                novaPozajmica.DatumVracanja
                
            );
            return new PozajmicaRezultat(PozajmicaIshod.Kreirana,ispisNovePozajmice);
    }
}
