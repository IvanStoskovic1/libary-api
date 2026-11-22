using System;
using System.Formats.Asn1;
using BibliotekaIznajmljivanje.Api.Data;
using BibliotekaIznajmljivanje.Api.Dtos;
using BibliotekaIznajmljivanje.Api.Models;
using BibliotekaIznajmljivanje.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace BibliotekaIznajmljivanje.Tests;

public class PozajmicaServiceTests : IDisposable
{
    private readonly BibliotekaDbContext _baza;
    private readonly PozajmicaService _servis;
    public PozajmicaServiceTests()
    {
        _baza = TestBaza.MemorijskaBaza();
        _servis = new PozajmicaService(_baza);
    }
    public void Dispose()
    {
        var konekcija = _baza.Database.GetDbConnection();
        _baza.Dispose();
        konekcija.Dispose();
    }

    private async Task<Korisnik> NoviKorisnik(string ime, string prezime, string email)
    {
        var noviKorisnik = new Korisnik
        {

            Ime = ime,
            Prezime = prezime,
            Email = email

        };
        _baza.Korisnici.Add(noviKorisnik);
        await _baza.SaveChangesAsync();
        return noviKorisnik;
    }

    private async Task<Knjiga> NovaKnjiga(string naslovKnjige, string autor, string zanr, int godinaIzdavanja, decimal cena)
    {
        var novaKnjiga = new Knjiga
        {

            NaslovKnjige = naslovKnjige,
            Autor = autor,
            Zanr = zanr,
            GodinaIzdavanja = godinaIzdavanja,
            Cena = cena
        };
        _baza.Knjige.Add(novaKnjiga);
        await _baza.SaveChangesAsync();
        return novaKnjiga;
    }
    private async Task<Pozajmica> NovaPozajmica(Korisnik korisnik, Knjiga knjiga
    , DateTime datumPreuzimanja, DateTime rokVracanja, DateTime? datumVracanja)
    {
        var novaPozajmica = new Pozajmica
        {
            KorisnikId = korisnik.Id,
            KnjigaId = knjiga.Id,
            DatumPreuzimanja = datumPreuzimanja,
            RokVracanja = rokVracanja,
            DatumVracanja = datumVracanja
        };
        _baza.Pozajmica.Add(novaPozajmica);
        await _baza.SaveChangesAsync();
        return novaPozajmica;
    }


    //VRACANJE KNJIGE
    [Fact]
    public async Task VracanjeKnjigeAsync_NePostojiTrazenaPozajmica()
    {
        var vremeSad = DateTime.UtcNow;
        var korisnik = await NoviKorisnik("Marko", "a", "test@example.com");
        var knjiga = await NovaKnjiga("Tri Praseta", "", "", 1999, 230);
        var pozajmicaProvera = await NovaPozajmica(korisnik, knjiga, vremeSad, vremeSad.AddDays(14), null);

        var rezultat = await _servis.VracanjeKnjigeAsync(pozajmicaProvera.Id + 1);
        Assert.Equal(PozajmicaIshod.NijeNadjena, rezultat);

    }
    [Fact]
    public async Task VracanjeKnjigeAsync_PozajmicaJeVecVracena()
    {
        var vremeSad = DateTime.UtcNow;
        var korisnik = await NoviKorisnik("Marko", "a", "test@example.com");
        var knjiga = await NovaKnjiga("Tri Praseta", "", "", 1999, 230);
        var pozajmica = await NovaPozajmica(korisnik, knjiga, vremeSad.AddDays(-2), vremeSad.AddDays(14), vremeSad.AddDays(-1));

        var rezultat = await _servis.VracanjeKnjigeAsync(pozajmica.Id);
        Assert.Equal(PozajmicaIshod.VecVracena, rezultat);
    }
    [Fact]
    public async Task VracanjeKnjigeAsync_UspesnoJeVracenaPozajmica()
    {
        var vremeSad = DateTime.UtcNow;
        var korisnik = await NoviKorisnik("Ivan", "a", "test@example.com");
        var knjiga = await NovaKnjiga("Tri Praseta", "", "", 1999, 230);
        var pozajmica = await NovaPozajmica(korisnik, knjiga, vremeSad, vremeSad.AddDays(14), null);

        var rezultat = await _servis.VracanjeKnjigeAsync(pozajmica.Id);
        Assert.Equal(PozajmicaIshod.Uspeh, rezultat);

        //PROVERA BAZE
        _baza.ChangeTracker.Clear();
        var pregledBaze = await _baza.Pozajmica.AsNoTracking().FirstAsync(p => p.Id == pozajmica.Id);
        Assert.NotNull(pregledBaze.DatumVracanja);

    }
    //VRACANJE SVIH POZAJMICA
    [Fact]
    public async Task PrikazSvihPozajmicaAsync_KadNemaPozajmica_VracaPraznuListu()
    {
        var rezultat = await _servis.PrikazSvihPozajmicaAsync();
        Assert.Empty(rezultat);
    }
    [Fact]
    public async Task PrikazSvihPozajmicaAsync_KadImaPozajmica_VracaListuPozajmica()
    {
        var vremeSad = DateTime.UtcNow;
        var korisnikPrvi = await NoviKorisnik("Marko", "a", "test@example.com");
        var korisnikDrugi = await NoviKorisnik("Nikola", "b", "test1@example.com");

        var knjigaPrva = await NovaKnjiga("Tri Praseta", "", "", 1999, 230);
        var knjigaDruga = await NovaKnjiga("Plavi Cuperak", "", "", 2000, 1230);
        var knjigaTreca = await NovaKnjiga("Zeleno", "", "", 2000, 1230);

        var pozajmicaPrva = await NovaPozajmica(korisnikPrvi, knjigaPrva, vremeSad, vremeSad.AddDays(14), null);
        var pozajmicaDruga = await NovaPozajmica(korisnikPrvi, knjigaDruga, vremeSad, vremeSad.AddDays(14), null);
        var pozajmicaTreca = await NovaPozajmica(korisnikDrugi, knjigaTreca, vremeSad, vremeSad.AddDays(14), null);

        var rezultat = await _servis.PrikazSvihPozajmicaAsync();
        Assert.Equal(3, rezultat.Count);
        Assert.Contains(rezultat, r => r.Id == pozajmicaPrva.Id);
        Assert.Contains(rezultat, r => r.Id == pozajmicaDruga.Id);
        Assert.Contains(rezultat, r => r.Id == pozajmicaTreca.Id);

        var prva = rezultat.Single(r => r.Id == pozajmicaPrva.Id);
        Assert.Equal("Marko", prva.ImeKorisnika);
        Assert.Equal("Tri Praseta", prva.NaslovKnjige);
        var treca = rezultat.Single(r => r.Id == pozajmicaTreca.Id);
        Assert.Equal("Nikola", treca.ImeKorisnika);
        Assert.Equal("Zeleno", treca.NaslovKnjige);

    }
    //PRIKAZ JEDNE POZAJMICE PO ID
    [Fact]
    public async Task PrikazJednePozajmicePoIdAsync_KadNemaPozajmice_VracaNull()
    {
        var vremeSad = DateTime.UtcNow;
        var korisnik = await NoviKorisnik("Marko", "a", "test@example.com");
        var knjiga = await NovaKnjiga("Tri Praseta", "", "", 1999, 230);
        var pozajmicaProvera = await NovaPozajmica(korisnik, knjiga, vremeSad, vremeSad.AddDays(14), null);

        var rezultat = await _servis.PrikazJednePozajmicePoIdAsync(pozajmicaProvera.Id + 1);
        Assert.Null(rezultat);
    }

    [Fact]
    public async Task PrikazJednePozajmicePoIdAsync_KadPostojiPozajmica_VracaPozajmicu()
    {
        var vremeSad = DateTime.UtcNow;
        var korisnik = await NoviKorisnik("Marko", "a", "test@example.com");
        var knjiga = await NovaKnjiga("Tri Praseta", "", "", 1999, 230);
        var pozajmicaProvera = await NovaPozajmica(korisnik, knjiga, vremeSad, vremeSad.AddDays(14), null);
        var korisnik2 = await NoviKorisnik("Nikola", "a", "test1@example.com");
        var knjiga2 = await NovaKnjiga("Cetiri Praseta", "", "", 1999, 230);
        await NovaPozajmica(korisnik2, knjiga2, vremeSad, vremeSad.AddDays(14), null);

        var rezultat = await _servis.PrikazJednePozajmicePoIdAsync(pozajmicaProvera.Id);

        Assert.NotNull(rezultat);
        Assert.Equal(pozajmicaProvera.Id, rezultat.Id);
        Assert.Equal(korisnik.Id, rezultat.KorisnikId);
        Assert.Equal(knjiga.Id, rezultat.KnjigaId);
        Assert.Equal("Marko", rezultat.ImeKorisnika);
        Assert.Equal("Tri Praseta", rezultat.NaslovKnjige);
    }

    //PRIKAZ SVIH POZAJMICA OD JEDNOG KORISNIKA
    [Fact]
    public async Task PrikazSvihPozajmicaJednogKorisnikaAsync_KadKorisnikNePostoji_VracaNull()
    {
        var korisnik = await NoviKorisnik("Marko", "a", "test@example.com");
        var rezultat = await _servis.PrikazSvihPozajmicaJednogKorisnikaAsync(korisnik.Id + 1);
        Assert.Null(rezultat);
    }

    [Fact]
    public async Task PrikazSvihPozajmicaJednogKorisnikaAsync_KadNemaPozajmicaKorisnik_VracaPraznuListu()
    {
        var korisnik = await NoviKorisnik("Marko", "a", "test@example.com");
        var rezultat = await _servis.PrikazSvihPozajmicaJednogKorisnikaAsync(korisnik.Id);
        Assert.NotNull(rezultat);
        Assert.Empty(rezultat);
    }

    [Fact]
    public async Task PrikazSvihPozajmicaJednogKorisnikaAsync_KadaPostojePozajmice_VracaListuPozajmica()
    {
        var vreme = DateTime.UtcNow;
        var korisnik_Prvi = await NoviKorisnik("primer1", "primer1", "test1@example.com");
        var korisnik_Drugi = await NoviKorisnik("primer2", "primer2", "test2@example.com");


        var knjiga_Prva = await NovaKnjiga("Rat i mi", "Tolstoj", "", 1930, 2000);
        var knjiga_Druga = await NovaKnjiga("Rec", "Kornelije", "", 1990, 1000);
        var knjiga_Treca = await NovaKnjiga("Lap", "Cehov", "", 1900, 1200);


        var pozajmica_Prva = await NovaPozajmica(korisnik_Prvi, knjiga_Prva, vreme, vreme.AddDays(14), null);
        var pozajmica_Druga = await NovaPozajmica(korisnik_Prvi, knjiga_Druga, vreme, vreme.AddDays(14), null);
        var pozajmica_Prva_Drugi_Korisnik = await NovaPozajmica(korisnik_Drugi, knjiga_Treca, vreme, vreme.AddDays(14), null);

        var rezultat = await _servis.PrikazSvihPozajmicaJednogKorisnikaAsync(korisnik_Prvi.Id);

        Assert.NotNull(rezultat);
        Assert.Equal(2, rezultat.Count);
        Assert.Contains(rezultat, r => r.Id == pozajmica_Prva.Id);
        Assert.Contains(rezultat, r => r.Id == pozajmica_Druga.Id);
        Assert.DoesNotContain(rezultat, r => r.Id == pozajmica_Prva_Drugi_Korisnik.Id);
    }

    //PRIKAZ POZAJMICA KOJE KASNE
    [Fact]
    public async Task PrikazPozajmicaKojeKasneAsync_KadaNemaPozajmicaKojeKasne_VracaPraznuListu()
    {
        var vreme = DateTime.UtcNow;
        var korisnik_Prvi = await NoviKorisnik("primer1", "primer1", "test1@example.com");
        var korisnik_Drugi = await NoviKorisnik("primer2", "primer2", "test2@example.com");


        var knjiga_Prva = await NovaKnjiga("Rat i mi", "Tolstoj", "", 1930, 2000);
        var knjiga_Druga = await NovaKnjiga("Rec", "Kornelije", "", 1990, 1000);
        var knjiga_Treca = await NovaKnjiga("Lap", "Cehov", "", 1900, 1200);


        var pozajmica_Prva = await NovaPozajmica(korisnik_Prvi, knjiga_Prva, vreme, vreme.AddDays(14), vreme.AddDays(1));
        var pozajmica_Druga = await NovaPozajmica(korisnik_Prvi, knjiga_Druga, vreme, vreme.AddDays(14), null);
        var pozajmica_Prva_Drugi_Korisnik = await NovaPozajmica(korisnik_Drugi, knjiga_Treca, vreme, vreme.AddDays(14), vreme.AddDays(2));

        var rezultat = await _servis.PrikazPozajmicaKojeKasneAsync();
        Assert.Empty(rezultat);
    }

    [Fact]
    public async Task PrikazPozajmicaKojeKasneAsync_KadPostojePozajmice_VracaOneKojeKasne()
    {
        var vreme = DateTime.UtcNow;
        var korisnikPrvi = await NoviKorisnik("primer1", "primer1", "test1@example.com");
        var korisnikDrugi = await NoviKorisnik("primer2", "primer2", "test2@example.com");


        var knjigaPrva = await NovaKnjiga("Rat i mi", "Tolstoj", "", 1930, 2000);
        var knjigaDruga = await NovaKnjiga("Rec", "Kornelije", "", 1990, 1000);
        var knjigaTreca = await NovaKnjiga("Lap", "Cehov", "", 1900, 1200);
        var knjigaCetvrta = await NovaKnjiga("Brat", "Dostojevski", "", 1880, 900);

        //kasne
        var pozajmicaPrva = await NovaPozajmica(korisnikPrvi, knjigaPrva, vreme.AddDays(-20), vreme.AddDays(-3), null);
        var pozajmicaDruga = await NovaPozajmica(korisnikDrugi, knjigaDruga, vreme.AddDays(-30), vreme.AddDays(-5), null);

        // ne kasne
        await NovaPozajmica(korisnikPrvi, knjigaTreca, vreme.AddDays(-1), vreme.AddDays(14), null);
        await NovaPozajmica(korisnikDrugi, knjigaCetvrta, vreme.AddDays(-20), vreme.AddDays(-6), vreme.AddDays(-8));


        var rezultat = await _servis.PrikazPozajmicaKojeKasneAsync();

        Assert.Equal(2, rezultat.Count);
        Assert.Contains(rezultat, r => r.Id == pozajmicaPrva.Id);
        Assert.Contains(rezultat, r => r.Id == pozajmicaDruga.Id);
        var prva = rezultat.Single(r => r.Id == pozajmicaPrva.Id);
        Assert.Equal("primer1", prva.ImeKorisnika);
        Assert.Equal("Rat i mi", prva.NaslovKnjige);

        var druga = rezultat.Single(r => r.Id == pozajmicaDruga.Id);
        Assert.Equal("primer2", druga.ImeKorisnika);
        Assert.Equal("Rec", druga.NaslovKnjige);
    }

    //POZAJMLJIVANJE KNJIGA
    [Fact]
    public async Task PozajmljivanjeKnjigaAsync_KadNePostojiKorisnik_VracaNijeNadjenKorisnik()
    {
        var korisnik = await NoviKorisnik("primer1", "primer1", "test1@example.com");
        var knjiga = await NovaKnjiga("Rat i mi", "Tolstoj", "", 1930, 2000);

        var pozajmica = new PozajmicaRequestDto
        {
            KnjigaId = knjiga.Id,
            KorisnikId = korisnik.Id + 1
        };
        var rezultat = await _servis.PozajmljivanjeKnjigaAsync(pozajmica);
        Assert.Equal(PozajmicaIshod.NijeNadjenKorisnik, rezultat.Ishod);
        Assert.Null(rezultat.PozajmicaDto);
        Assert.Equal(0, await _baza.Pozajmica.CountAsync());
    }
    [Fact]
    public async Task PozajmljivanjeKnjigaAsync_KadNePostojiKnjiga_VracaNijeNadjenaKnjiga()
    {
        var korisnik = await NoviKorisnik("primer1", "primer1", "test1@example.com");
        var knjiga = await NovaKnjiga("Rat i mi", "Tolstoj", "", 1930, 2000);

        var pozajmica = new PozajmicaRequestDto
        {
            KnjigaId = knjiga.Id + 1,
            KorisnikId = korisnik.Id
        };

        var rezultat = await _servis.PozajmljivanjeKnjigaAsync(pozajmica);

        Assert.Equal(PozajmicaIshod.NijeNadjenaKnjiga, rezultat.Ishod);
        Assert.Null(rezultat.PozajmicaDto);
        Assert.Equal(0, await _baza.Pozajmica.CountAsync());

    }
    [Fact]
    public async Task PozajmljivanjeKnjigaAsync_KadJeKnjigaPozajmljena_VracaKnjigaPozajmljena()
    {
        var vreme = DateTime.UtcNow;
        var korisnik = await NoviKorisnik("primer1", "primer1", "test1@example.com");
        var korisnikDrugi = await NoviKorisnik("primer2", "primer2", "test2@example.com");
        var knjiga = await NovaKnjiga("Rat i mi", "Tolstoj", "", 1930, 2000);
        var pozajmica = await NovaPozajmica(korisnik, knjiga, vreme, vreme.AddDays(14), null);
        var pregledPozajmice = new PozajmicaRequestDto
        {
            KnjigaId = knjiga.Id,
            KorisnikId = korisnikDrugi.Id
        };
        var rezultat = await _servis.PozajmljivanjeKnjigaAsync(pregledPozajmice);
        Assert.Equal(PozajmicaIshod.KnjigaPozajmljena, rezultat.Ishod);
        Assert.Null(rezultat.PozajmicaDto);

        var pozajmiceUBazi = await _baza.Pozajmica.AsNoTracking().ToListAsync();
        var jedina = Assert.Single(pozajmiceUBazi);
        Assert.Equal(pozajmica.Id, jedina.Id);
        Assert.Equal(korisnik.Id, jedina.KorisnikId);
    }
    [Fact]
    public async Task PozajmljivanjeKnjigaAsync_KnjigaJeSlobodna_VracaPozajmicaKreirana()
    {
        var korisnik = await NoviKorisnik("primer1", "primer1", "test1@example.com");
        var knjiga = await NovaKnjiga("Rat i mi", "Tolstoj", "", 1930, 2000);
        var pregledPozajmice = new PozajmicaRequestDto
        {
            KnjigaId = knjiga.Id,
            KorisnikId = korisnik.Id
        };

        var rezultat = await _servis.PozajmljivanjeKnjigaAsync(pregledPozajmice);
        Assert.Equal(PozajmicaIshod.Kreirana, rezultat.Ishod);
        Assert.NotNull(rezultat.PozajmicaDto);
        Assert.True(rezultat.PozajmicaDto.Id > 0);

        Assert.Equal(korisnik.Id, rezultat.PozajmicaDto.KorisnikId);
        Assert.Equal(knjiga.Id, rezultat.PozajmicaDto.KnjigaId);
        Assert.Equal("primer1", rezultat.PozajmicaDto.ImeKorisnika);
        Assert.Equal("Rat i mi", rezultat.PozajmicaDto.NaslovKnjige);
        Assert.Null(rezultat.PozajmicaDto.DatumVracanja);

        _baza.ChangeTracker.Clear();
        var izBaze = await _baza.Pozajmica.AsNoTracking().SingleAsync();
        Assert.Equal(rezultat.PozajmicaDto.Id, izBaze.Id);
        Assert.Equal(korisnik.Id, izBaze.KorisnikId);
        Assert.Equal(knjiga.Id, izBaze.KnjigaId);
        Assert.Null(izBaze.DatumVracanja);
    }
}
