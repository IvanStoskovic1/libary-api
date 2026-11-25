using BibliotekaIznajmljivanje.Api.Data;
using BibliotekaIznajmljivanje.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace BibliotekaIznajmljivanje.Api.Services;

public class KnjigaService : IKnjigaService
{
    private readonly BibliotekaDbContext _db;

    public KnjigaService(BibliotekaDbContext db)
    {
        _db = db;
    }
    //PRIKAZ KNJIGE PO ID
    public async Task<KnjigaDto?> DajKnjiguPoIdAsync(int id)
    {
        var pretragaKnjigePoId = await _db.Knjige.FindAsync(id);

        if (pretragaKnjigePoId == null)
        {
            return null;
        }
        var knjigeDto = new KnjigaDto(
            pretragaKnjigePoId.Id,
            pretragaKnjigePoId.NaslovKnjige,
            pretragaKnjigePoId.Autor,
            pretragaKnjigePoId.Zanr,
            pretragaKnjigePoId.GodinaIzdavanja,
            pretragaKnjigePoId.Cena
        );
        return knjigeDto;
    }
    //DODAVANJE KNJIGE 
    public async Task<KnjigaDto> DodajKnjiguAsync(KnjigaRequestDto createDto)
    {
        var novaKnjiga = new Knjiga
        {
            NaslovKnjige = createDto.NaslovKnjige,
            Autor = createDto.Autor,
            Zanr = createDto.Zanr,
            GodinaIzdavanja = createDto.GodinaIzdavanja,
            Cena = createDto.Cena
        };
        _db.Knjige.Add(novaKnjiga);
        await _db.SaveChangesAsync();
        var knjigaDto = new KnjigaDto
        (
            novaKnjiga.Id,
            novaKnjiga.NaslovKnjige,
            novaKnjiga.Autor,
            novaKnjiga.Zanr,
            novaKnjiga.GodinaIzdavanja,
            novaKnjiga.Cena

        );
        return knjigaDto;
    }

    //PRIKAZ SVIH KNJIGA
    public async Task<KnjigaPrikazStavkiDto> PrikaziSveKnjigeAsync(string? search, int page = 1, int pageSize = 10)
    {
        var upit = _db.Knjige.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            upit = upit.Where(k => k.NaslovKnjige.Contains(search) || k.Autor.Contains(search));
        }
        if (page < 1)
            page = 1;
        if (pageSize < 1)
            pageSize = 10;
        if (pageSize > 50)
            pageSize = 50;
        var brojKnjiga = await upit.CountAsync();
        var prikaz = await upit.OrderBy(k => k.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var knjigeDto = prikaz.Select(k => new KnjigaDto(
            k.Id,
            k.NaslovKnjige,
            k.Autor,
            k.Zanr,
            k.GodinaIzdavanja,
            k.Cena
        )).ToList();
        var prikazStavki = new KnjigaPrikazStavkiDto(
           knjigeDto,
           page,
           pageSize,
           brojKnjiga
       );
        return prikazStavki;
    }

    //IZMENA KNJIGE PO ID
    public async Task<bool> IzmenaKnjigaPoIdAsync(int id, KnjigaRequestDto izmeniDto)
    {
        var pronadjenaKnjiga = await _db.Knjige.FindAsync(id);
        if (pronadjenaKnjiga == null)
        {
            return false;
        }
        pronadjenaKnjiga.NaslovKnjige = izmeniDto.NaslovKnjige;
        pronadjenaKnjiga.Autor = izmeniDto.Autor;
        pronadjenaKnjiga.Zanr = izmeniDto.Zanr;
        pronadjenaKnjiga.GodinaIzdavanja = izmeniDto.GodinaIzdavanja;
        pronadjenaKnjiga.Cena = izmeniDto.Cena;
        await _db.SaveChangesAsync();
        return true;
    }

    //IZBRISI KNJIGU PO ID
    public async Task<BrisanjeIshod> IzbrisiKnjiguPoIdAsync(int id)
    {
        var pronadjenaKnjiga = await _db.Knjige.FindAsync(id);
        if (pronadjenaKnjiga == null)
        {
            return BrisanjeIshod.NijeNadjena;
        }

        bool knjigaZaBrisanje = await _db.Pozajmica.AnyAsync(pozajmica => pozajmica.KnjigaId == pronadjenaKnjiga.Id);
        if (knjigaZaBrisanje)
        {
            return BrisanjeIshod.ImaPozajmicu;
        }
        else
        {
            _db.Knjige.Remove(pronadjenaKnjiga);
            await _db.SaveChangesAsync();
            return BrisanjeIshod.Uspeh;
        }

    }
}
