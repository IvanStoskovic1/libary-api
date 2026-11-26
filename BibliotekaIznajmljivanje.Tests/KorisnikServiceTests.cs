using BibliotekaIznajmljivanje.Api.Data;
using BibliotekaIznajmljivanje.Api.Dtos;
using BibliotekaIznajmljivanje.Api.Models;
using BibliotekaIznajmljivanje.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace BibliotekaIznajmljivanje.Tests;

public class KorisnikServiceTests : IDisposable
{
    private readonly BibliotekaDbContext _baza;
    private readonly KorisnikService _servis;

    public KorisnikServiceTests()
    {
        _baza = TestBaza.MemorijskaBaza();
        _servis = new KorisnikService(_baza);
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
    , DateTime? datumVracanja)
    {
        var novaPozajmica = new Pozajmica
        {
            KorisnikId = korisnik.Id,
            KnjigaId = knjiga.Id,
            DatumVracanja = datumVracanja
        };
        _baza.Pozajmica.Add(novaPozajmica);
        await _baza.SaveChangesAsync();
        return novaPozajmica;
    }

    //PRIKAZ KORISNIKA
    [Fact]
    public async Task PrikazSvihKorisnikaAsync_KadaJeBazaPrazna_VracaPraznuListu()
    {
        var rezultat = await _servis.PrikazSvihKorisnikaAsync();
        Assert.Empty(rezultat);
    }
    [Fact]
    public async Task PrikazSvihKorisnikaAsync_KadaPostojeKnjige_VracaListuKnjiga()
    {
        var prviKorisnik =await  NoviKorisnik("prvi", "prvic", "test@example.com");
        var drugiKorisnik =await NoviKorisnik("drugi", "drugic", "test1@example.com");

        var rezultat = await _servis.PrikazSvihKorisnikaAsync();
        Assert.NotEmpty(rezultat);
        Assert.Equal(2, rezultat.Count);
        var pregledPrvog = rezultat.Find(r => r.id == prviKorisnik.Id);
        Assert.NotNull(pregledPrvog);
        Assert.Equal("prvi", pregledPrvog.Ime);
        Assert.Equal("prvic", pregledPrvog.Prezime);
        Assert.Equal("test@example.com", pregledPrvog.Email);
        var pregledDrugog = rezultat.Find(r => r.id == drugiKorisnik.Id);
        Assert.NotNull(pregledDrugog);
        Assert.Equal("drugi", pregledDrugog.Ime);
        Assert.Equal("drugic", pregledDrugog.Prezime);
        Assert.Equal("test1@example.com", pregledDrugog.Email);
    }

    //PRIKAZ KORISNIKA PO ID
    [Fact]
    public async Task PrikazKorisnikaPoIdAsync_KadaNePostojiKorisnik_VracaNull()
    {
        var prviKorisnik = await NoviKorisnik("prvi", "prvic", "test@example.com");
        var rezultat = await _servis.PrikazKorisnikaPoIdAsync(prviKorisnik.Id + 1);
        Assert.Null(rezultat);
    }

    [Fact]
    public async Task PrikazKorisnikaPoIdAsync_KadaPostojiKorisnik_VracaKorisnika()
    {
        var prviKorisnik = await NoviKorisnik("prvi", "prvic", "test@example.com");
        var rezultat = await _servis.PrikazKorisnikaPoIdAsync(prviKorisnik.Id);
        Assert.NotNull(rezultat);
        Assert.Equal("prvi", rezultat.Ime);
        Assert.Equal("prvic", rezultat.Prezime);
        Assert.Equal("test@example.com", rezultat.Email);
    }

    //KREIRANJE KORISNIKA
    [Fact]
    public async Task KreiranjeKorisnikaAsync_EmailVecPostoji_VracaNull()
    {
        var prviKorisnik = await NoviKorisnik("prvi", "prvic", "test@example.com");
        var korisnikZaKreiranje = new KorisnikRequestDto
        {
            Ime = "Pera",
            Prezime = "Peric",
            Email = prviKorisnik.Email
        };
        var rezultat = await _servis.KreiranjeKorisnikaAsync(korisnikZaKreiranje);
        Assert.Null(rezultat);
        _baza.ChangeTracker.Clear();
        var pregledBaze = await _baza.Korisnici.AsNoTracking().SingleAsync();
        Assert.Equal(prviKorisnik.Id, pregledBaze.Id);
        Assert.Equal("prvi", pregledBaze.Ime);
        Assert.Equal("prvic", pregledBaze.Prezime);
    }
    [Fact]
    public async Task KreiranjeKorisnikaAsync_KreiraSeKorisnik_VracaKreiranogKorisnika()
    {
        var prviKorisnik = await NoviKorisnik("prvi", "prvic", "test@example.com");

        var korisnikaZaKreiranje = new KorisnikRequestDto
        {
            Ime = "drugi",
            Prezime = "drugic",
            Email = "test1@gmail.com"
        };
        var rezultat = await _servis.KreiranjeKorisnikaAsync(korisnikaZaKreiranje);
        Assert.NotNull(rezultat);
        Assert.Equal("drugi", rezultat.Ime);
        Assert.Equal("drugic", rezultat.Prezime);
        Assert.Equal("test1@gmail.com", rezultat.Email);
        _baza.ChangeTracker.Clear();
        var pregledBazeKolicina = await _baza.Korisnici.CountAsync();
        Assert.Equal(2, pregledBazeKolicina);
        var pregledBaze=await _baza.Korisnici.ToListAsync();
        Assert.Contains(pregledBaze, r => r.Id == rezultat.id);
        Assert.Contains(pregledBaze, r => r.Id == prviKorisnik.Id);

    }

    //IZMENA KORISNIKA PO ID
    [Fact]
    public async Task IzmenaKorisnikaPoIdAsync_NePostojiKorisnik_VracaFalse()
    {
        var prviKorisnik = await NoviKorisnik("prvi", "prvic", "test@example.com");
        var izmenjenKorisnik = new KorisnikUpdateDto
        {
            Ime = "NovoIme",
            Prezime = "NovoPrezime"
        };
        var rezultat = await _servis.IzmenaKorisnikaPoIdAsync(prviKorisnik.Id + 1, izmenjenKorisnik);
        Assert.False(rezultat);
        _baza.ChangeTracker.Clear();
        var pregledBaze = await _baza.Korisnici.SingleAsync();
        Assert.Equal("prvi", pregledBaze.Ime);
        Assert.Equal("prvic", pregledBaze.Prezime);
    }
    [Fact]
    public async Task IzmenaKorisnikaPoIdAsync_KorisnikIzmenjen_VracaTrue()
    {
        var prviKorisnik = await NoviKorisnik("prvi", "prvic", "test@example.com");
        var drugiKorisnik = await NoviKorisnik("drugi", "drugic", "test1@example.com");
        var izmenjenKorisnik = new KorisnikUpdateDto
        {
            Ime = "NovoIme",
            Prezime = "NovoPrezime"
        };
        var rezultat = await _servis.IzmenaKorisnikaPoIdAsync(prviKorisnik.Id, izmenjenKorisnik);
        Assert.True(rezultat);
        _baza.ChangeTracker.Clear();
        var pregledBaze = await _baza.Korisnici.FirstAsync(r => r.Id == prviKorisnik.Id);
        Assert.Equal("NovoIme", pregledBaze.Ime);
        Assert.Equal("NovoPrezime", pregledBaze.Prezime);
    }

    //BRISANJE KORISNIKA PO ID
    [Fact]
    public async Task BrisanjeKorisnikaPoIdAsync_NePostojiKorisnik_VracaNijeNadjeno()
    {
        var prviKorisnik = await NoviKorisnik("prvi", "prvic", "test@example.com");
        var rezultat = await _servis.BrisanjeKorisnikaPoIdAsync(prviKorisnik.Id + 1);
        Assert.Equal(BrisanjeIshod.NijeNadjena, rezultat);
        _baza.ChangeTracker.Clear();
        var pregledBazeKolicina = await _baza.Korisnici.CountAsync();
        Assert.Equal(1, pregledBazeKolicina);
        Assert.Contains(_baza.Korisnici, r => r.Id == prviKorisnik.Id);
    }
    [Fact]
    public async Task BrisanjeKorisnikaPoIdAsync_KorisnikImaPozajmicuNull_VracaImaPozajmicu()
    {
        var prviKorisnik = await NoviKorisnik("prvi", "prvic", "test@example.com");
        var knjiga = await NovaKnjiga("primer", "primerAutor", "primerZanr", 2010, 1000);
        var pozajmica = await NovaPozajmica(prviKorisnik, knjiga, null);
        var rezultat = await _servis.BrisanjeKorisnikaPoIdAsync(prviKorisnik.Id);
        Assert.Equal(BrisanjeIshod.ImaPozajmicu, rezultat);
        Assert.Null(pozajmica.DatumVracanja);
        _baza.ChangeTracker.Clear();
        var pregledBazeKolicina = await _baza.Korisnici.CountAsync();
        Assert.Equal(1, pregledBazeKolicina);
        Assert.Contains(_baza.Korisnici, r => r.Id == prviKorisnik.Id);
    }
    [Fact]
    public async Task BrisanjeKorisnikaPoIdAsync_KorisnikImaPozajmicuDatumVracanja_VracaImaPozajmicu()
    {
        var prviKorisnik = await NoviKorisnik("prvi", "prvic", "test@example.com");
        var knjiga = await NovaKnjiga("primer", "primerAutor", "primerZanr", 2010, 1000);
        var pozajmica = await NovaPozajmica(prviKorisnik, knjiga, DateTime.UtcNow.AddDays(5));
        var rezultat = await _servis.BrisanjeKorisnikaPoIdAsync(prviKorisnik.Id);
        Assert.Equal(BrisanjeIshod.ImaPozajmicu, rezultat);
        Assert.NotNull(pozajmica.DatumVracanja);
        _baza.ChangeTracker.Clear();
        var pregledBazeKolicina = await _baza.Korisnici.CountAsync();
        Assert.Equal(1, pregledBazeKolicina);
        Assert.Contains(_baza.Korisnici, r => r.Id == prviKorisnik.Id);
    }
    [Fact]
    public async Task BrisanjeKorisnikaPoIdAsync_KorisnikNemaPozajmicu_VracaUspesnoBrisanje()
    {
        var prviKorisnik = await NoviKorisnik("prvi", "prvic", "test@example.com");
        var drugiKorisnik = await NoviKorisnik("drugi", "drugic", "test1@example.com");
        var rezultat=await _servis.BrisanjeKorisnikaPoIdAsync(prviKorisnik.Id);
        Assert.Equal(BrisanjeIshod.Uspeh,rezultat);
        _baza.ChangeTracker.Clear();
        var pregledBazeKolicina = await _baza.Korisnici.CountAsync();
        Assert.Equal(1, pregledBazeKolicina);
        Assert.Contains(_baza.Korisnici, r => r.Id == drugiKorisnik.Id);
        var pregledBaze=await _baza.Korisnici.SingleAsync();
        Assert.Equal("drugi",pregledBaze.Ime);
        Assert.Equal("drugic",pregledBaze.Prezime);
    }


}
