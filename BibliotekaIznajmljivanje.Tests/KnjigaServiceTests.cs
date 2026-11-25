using BibliotekaIznajmljivanje.Api.Data;
using BibliotekaIznajmljivanje.Api.Dtos;
using BibliotekaIznajmljivanje.Api.Models;
using BibliotekaIznajmljivanje.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace BibliotekaIznajmljivanje.Tests;

public class KnjigaServiceTests : IDisposable
{
    private readonly BibliotekaDbContext _baza;
    private readonly KnjigaService _servis;

    public KnjigaServiceTests()
    {
        _baza = TestBaza.MemorijskaBaza();
        _servis = new KnjigaService(_baza);
    }
    public void Dispose()
    {
        var konekcija = _baza.Database.GetDbConnection();
        _baza.Dispose();
        konekcija.Dispose();
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
    private async Task<Pozajmica> NovaPozajmica(Korisnik korisnik, Knjiga knjiga
    ,  DateTime? datumVracanja)
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

    //PRIKAZ KNJIGE PO ID
    [Fact]
    public async Task DajKnjiguPoIdAsync_KadNePostojiKnjiga_VracaNull()
    {
        var knjiga = await NovaKnjiga("primerNaslov", "primerAutor", "primerZanr", 2000, 1000);
        var rezultat = await _servis.DajKnjiguPoIdAsync(knjiga.Id + 1);
        Assert.Null(rezultat);
    }
    [Fact]
    public async Task DajKnjiguPoIdAsync_KadPostojiKnjiga_VracaKnjigu()
    {
        var knjiga = await NovaKnjiga("primerNaslov", "primerAutor", "primerZanr", 2000, 1000);
        var rezultat = await _servis.DajKnjiguPoIdAsync(knjiga.Id);
        Assert.NotNull(rezultat);

        Assert.Equal(knjiga.Autor,rezultat.Autor);
        Assert.Equal(knjiga.NaslovKnjige,rezultat.NaslovKnjige);
    }

    //DODAVANJE KNJIGE
    [Fact]
    public async Task DodajKnjiguAsync_DodajeSeKnjiga_VracaDodatuKnjigu()
    {
        var knjigaZaDodavanje = new KnjigaRequestDto
        {
            NaslovKnjige = "primerNaslov",
            Autor = "primerAutor",
            Zanr = "primerZanr",
            GodinaIzdavanja = 2000,
            Cena = 1000
        };
        var rezultat = await _servis.DodajKnjiguAsync(knjigaZaDodavanje);
        Assert.NotNull(rezultat);
        Assert.True(rezultat.Id > 0);
        Assert.Equal(knjigaZaDodavanje.NaslovKnjige, rezultat.NaslovKnjige);
        Assert.Equal(knjigaZaDodavanje.Autor, rezultat.Autor);
        Assert.Equal(knjigaZaDodavanje.Zanr, rezultat.Zanr);
        Assert.Equal(knjigaZaDodavanje.GodinaIzdavanja, rezultat.GodinaIzdavanja);
        Assert.Equal(knjigaZaDodavanje.Cena, rezultat.Cena);
    }

    [Fact]
    public async Task DodajKnjiguAsync_KadJeZahtevIspravan_DodajeKnjiguUBazu()
    {
        var knjigaZaDodavanje = new KnjigaRequestDto
        {
            NaslovKnjige = "primerNaslov",
            Autor = "primerAutor",
            Zanr = "primerZanr",
            GodinaIzdavanja = 2000,
            Cena = 1000
        };
        var rezultat = await _servis.DodajKnjiguAsync(knjigaZaDodavanje);
        _baza.ChangeTracker.Clear();
        var pregledBaze = await _baza.Knjige.AsNoTracking().SingleAsync();
        Assert.Equal(rezultat.Id, pregledBaze.Id);
        Assert.Equal("primerNaslov", pregledBaze.NaslovKnjige);
        Assert.Equal("primerAutor", pregledBaze.Autor);
        Assert.Equal("primerZanr", pregledBaze.Zanr);
        Assert.Equal(2000, pregledBaze.GodinaIzdavanja);
        Assert.Equal(1000, pregledBaze.Cena);
    }

    //PRIKAZ SVIH KNJIGA
    [Fact]
    public async Task PrikaziSveKnjigeAsync_KadJeBazaPrazna_VracaPraznuListu()
    {
        var rezultat = await _servis.PrikaziSveKnjigeAsync(null);
        Assert.NotNull(rezultat);
        Assert.Empty(rezultat.Stavke);
        Assert.Equal(1, rezultat.Page);
        Assert.Equal(10, rezultat.PageSize);

    }

    [Fact]
    public async Task PrikaziSveKnjigeAsync_KadPostojeKnjige_VracaListuKnjiga()
    {
        var knjigaPrva = await NovaKnjiga("primerNaslov", "primerAutor", "primerZanr", 2000, 1000);
        var knjigaDruga = await NovaKnjiga("primerNaslovDva", "primerAutorDva", "primerZanrDva", 1900, 1500);
        var knjigaTreca = await NovaKnjiga("primerNaslovTri", "primerAutorTri", "primerZanrTri", 2010, 2100);
        var rezultat = await _servis.PrikaziSveKnjigeAsync(null);
        Assert.Equal(3, rezultat.Stavke.Count);
        Assert.Equal(3, rezultat.UkupnoStavki);
        Assert.Equal(1, rezultat.Page);
        Assert.Equal(10, rezultat.PageSize);
        Assert.Contains(rezultat.Stavke, r => r.Id == knjigaPrva.Id);
        Assert.Contains(rezultat.Stavke, r => r.Id == knjigaDruga.Id);
        Assert.Contains(rezultat.Stavke, r => r.Id == knjigaTreca.Id);

        var pregledPrve = rezultat.Stavke.Find(p => p.Id == knjigaPrva.Id);

        Assert.NotNull(pregledPrve);

        Assert.Equal(knjigaPrva.NaslovKnjige, pregledPrve.NaslovKnjige);
        Assert.Equal(knjigaPrva.Autor, pregledPrve.Autor);
        Assert.Equal(knjigaPrva.Zanr, pregledPrve.Zanr);
        Assert.Equal(knjigaPrva.GodinaIzdavanja, pregledPrve.GodinaIzdavanja);
        Assert.Equal(knjigaPrva.Cena, pregledPrve.Cena);

        var pregledDruge = rezultat.Stavke.Find(p => p.Id == knjigaDruga.Id);
        Assert.NotNull(pregledDruge);
        Assert.Equal(knjigaDruga.NaslovKnjige, pregledDruge.NaslovKnjige);
        Assert.Equal(knjigaDruga.Autor, pregledDruge.Autor);
        Assert.Equal(knjigaDruga.Zanr, pregledDruge.Zanr);
        Assert.Equal(knjigaDruga.GodinaIzdavanja, pregledDruge.GodinaIzdavanja);
        Assert.Equal(knjigaDruga.Cena, pregledDruge.Cena);
    }
    [Fact]
    public async Task PrikaziSveKnjigeAsync_KadKnjigeSadrzeNaslov_VracaListuKnjiga()
    {
        var knjigaPrva = await NovaKnjiga("primerNaslov", "primerAutor", "primerZanr", 2000, 1000);
        var knjigaDruga = await NovaKnjiga("primerNaslovDva", "primerAutorDva", "primerZanrDva", 1900, 1500);
        var knjigaTreca = await NovaKnjiga("tri", "primerAutorTri", "primerZanrTri", 2010, 2100);
        string search = "Naslov";
        var rezultat = await _servis.PrikaziSveKnjigeAsync(search);

        Assert.Equal(2, rezultat.UkupnoStavki);
        Assert.Equal(2, rezultat.Stavke.Count);
        Assert.Equal(1, rezultat.Page);
        Assert.Equal(10, rezultat.PageSize);
        Assert.Contains(rezultat.Stavke, s => s.Id == knjigaPrva.Id);
        Assert.Contains(rezultat.Stavke, s => s.Id == knjigaDruga.Id);
        Assert.DoesNotContain(rezultat.Stavke, s => s.Id == knjigaTreca.Id);
    }
    [Fact]
    public async Task PrikaziSveKnjigeAsync_KadKnjigeSadrzeAutor_VracaListuKnjiga()
    {
        var knjigaPrva = await NovaKnjiga("primerNaslov", "primerAutor", "primerZanr", 2000, 1000);
        var knjigaDruga = await NovaKnjiga("primerNaslovDva", "primerAutorDva", "primerZanrDva", 1900, 1500);
        var knjigaTreca = await NovaKnjiga("tri", "Tri", "primerZanrTri", 2010, 2100);
        string search = "Autor";
        var rezultat = await _servis.PrikaziSveKnjigeAsync(search);

        Assert.Equal(2, rezultat.UkupnoStavki);
        Assert.Equal(2, rezultat.Stavke.Count);
        Assert.Equal(1, rezultat.Page);
        Assert.Equal(10, rezultat.PageSize);
        Assert.Contains(rezultat.Stavke, s => s.Id == knjigaPrva.Id);
        Assert.Contains(rezultat.Stavke, s => s.Id == knjigaDruga.Id);
        Assert.DoesNotContain(rezultat.Stavke, s => s.Id == knjigaTreca.Id);
    }

    [Fact]
    public async Task PrikaziSveKnjigeAsync_KadNiKnjigaNiAutorNeSadrze_VracaPraznuListu()
    {
        await NovaKnjiga("primerNaslov", "primerAutor", "primerZanr", 2000, 1000);
        await NovaKnjiga("primerNaslovDva", "primerAutorDva", "primerZanrDva", 1900, 1500);
        await NovaKnjiga("primerNaslovTri", "primerAutorTri", "primerZanrTri", 2010, 2100);
        string search = "Nema";
        var rezultat = await _servis.PrikaziSveKnjigeAsync(search);
        Assert.Empty(rezultat.Stavke);
        Assert.Equal(0, rezultat.UkupnoStavki);
        Assert.Equal(1, rezultat.Page);
        Assert.Equal(10, rezultat.PageSize);
    }

    [Fact]
    public async Task PrikaziSveKnjigeAsync_ProveraRadaPaginacije_VracaDruguStranu()
    {
        var prvaKnjiga = await NovaKnjiga("primerNaslov", "primerAutor", "primerZanr", 2000, 1000);
        var drugaKnjiga = await NovaKnjiga("primerNaslovDva", "primerAutorDva", "primerZanrDva", 1900, 1500);
        var trecaKnjigaPrikaz = await NovaKnjiga("primerNaslovTri", "primerAutorTri", "primerZanrTri", 2010, 2100);
        var cetvrtaKnjigaPrikaz = await NovaKnjiga("primerNaslovCetiri", "primerAutorCetiri", "primerZanrCetiri", 2000, 1000);
        var petaKnjiga = await NovaKnjiga("primerNaslovPet", "primerAutorPet", "primerZanrPet", 1900, 1500);
        int page = 2;
        int pageSize = 2;
        var rezultat = await _servis.PrikaziSveKnjigeAsync(null, page, pageSize);

        Assert.Equal(page, rezultat.Page);
        Assert.Equal(pageSize, rezultat.PageSize);
        Assert.Equal(2, rezultat.Stavke.Count);
        Assert.Equal(5, rezultat.UkupnoStavki);
        Assert.Contains(rezultat.Stavke, r => r.Id == trecaKnjigaPrikaz.Id);
        Assert.Contains(rezultat.Stavke, r => r.Id == cetvrtaKnjigaPrikaz.Id);
        Assert.DoesNotContain(rezultat.Stavke, r => r.Id == prvaKnjiga.Id);
        Assert.DoesNotContain(rezultat.Stavke, r => r.Id == drugaKnjiga.Id);
        Assert.DoesNotContain(rezultat.Stavke, r => r.Id == petaKnjiga.Id);
    }

    [Fact]
    public async Task PrikaziSveKnjigeAsync_PageIPageSizeIspodOkvira_VracaNajvise10Stavki()
    {
        for (int i = 1; i <= 11; i++)
        {
            await NovaKnjiga($"Naslov{i}", $"Autor{i}", $"Roman", 1999, 1000);
        }
        int page = 0;
        int pageSize = 0;
        var rezultat = await _servis.PrikaziSveKnjigeAsync(null, page, pageSize);
        Assert.Equal(1, rezultat.Page);
        Assert.Equal(10, rezultat.PageSize);
        Assert.Equal(11, rezultat.UkupnoStavki);
        Assert.Equal(10, rezultat.Stavke.Count);
        Assert.Contains(rezultat.Stavke, r => r.NaslovKnjige == "Naslov1");
        Assert.DoesNotContain(rezultat.Stavke, r => r.NaslovKnjige == "Naslov11");
    }
    [Fact]
    public async Task PrikaziSveKnjigeAsync_PageSizeIznadOkvira_VracaNajvise50Stavki()
    {
        for (int i = 1; i <= 51; i++)
        {
            await NovaKnjiga($"Naslov{i}", $"Autor{i}", $"Roman", 1999, 1000);
        }
        int pageSize = 51;
        var rezultat = await _servis.PrikaziSveKnjigeAsync(null, 1, pageSize);
        Assert.Equal(1, rezultat.Page);
        Assert.Equal(50, rezultat.PageSize);
        Assert.Equal(51, rezultat.UkupnoStavki);
        Assert.Equal(50, rezultat.Stavke.Count);
        Assert.Contains(rezultat.Stavke, r => r.NaslovKnjige == "Naslov1");
        Assert.DoesNotContain(rezultat.Stavke, r => r.NaslovKnjige == "Naslov51");
    }
    [Fact]
    public async Task PrikaziSveKnjigeAsync_ProveraSearchIPaginacije_VracaOdgovarajucuListuSearch()
    {
        var prvaKnjigaPrikaz = await NovaKnjiga("primerNaslov", "primerAutor", "primerZanr", 2000, 1000);
        var drugaKnjigaPrikaz = await NovaKnjiga("primerNaslovDva", "primerAutorDva", "primerZanrDva", 1900, 1500);
        var trecaKnjigaPrikaz = await NovaKnjiga("primerNaslovTri", "primerAutorTri", "primerZanrTri", 2010, 2100);
        var cetvrtaKnjigaPrikaz = await NovaKnjiga("primerNaslovCetiri", "primerAutorCetiri", "primerZanrCetiri", 2000, 1000);
        var petaKnjiga = await NovaKnjiga("primerNaslovPet", "primerAutorPet", "primerZanrPet", 1900, 1500);

        int pageSize = 2;
        string search = "Naslov";
        var rezultat = await _servis.PrikaziSveKnjigeAsync(search, page: 1, pageSize);
        Assert.Equal(1, rezultat.Page);
        Assert.Equal(pageSize, rezultat.PageSize);
        Assert.Equal(2, rezultat.Stavke.Count);
        Assert.Equal(5, rezultat.UkupnoStavki);
        Assert.Contains(rezultat.Stavke, r => r.Id == prvaKnjigaPrikaz.Id);
        Assert.Contains(rezultat.Stavke, r => r.Id == drugaKnjigaPrikaz.Id);
    }

    //IZMENA KNJIGE PO ID
    [Fact]
    public async Task IzmenaKnjigaPoIdAsync_KadaNePostojiId_VracaFalse()
    {
        var prvaKnjigaPrikaz = await NovaKnjiga("primerNaslov", "primerAutor", "primerZanr", 2000, 1500);

        var dto = new KnjigaRequestDto
        {
            NaslovKnjige = "NoviNaslov",
            Autor = "NoviAutor",
            Zanr = "NoviZanr",
            GodinaIzdavanja = 1900,
            Cena = 1000
        };
        var rezultat = await _servis.IzmenaKnjigaPoIdAsync(prvaKnjigaPrikaz.Id + 1, dto);

        Assert.False(rezultat);
        _baza.ChangeTracker.Clear();
        var pregledBaze = await _baza.Knjige.AsNoTracking().SingleAsync();

        Assert.Equal("primerNaslov", pregledBaze.NaslovKnjige);
        Assert.Equal("primerAutor", pregledBaze.Autor);
        Assert.Equal("primerZanr", pregledBaze.Zanr);
        Assert.Equal(2000, pregledBaze.GodinaIzdavanja);
        Assert.Equal(1500, pregledBaze.Cena);

    }
    [Fact]
    public async Task IzmenaKnjigaPoIdAsync_KadSeKnjigaIzmeni_VracaTrue()
    {
        var prvaKnjigaPrikaz = await NovaKnjiga("primerNaslov", "primerAutor", "primerZanr", 2000, 1000);

        var dto = new KnjigaRequestDto
        {
            NaslovKnjige = "NoviNaslov",
            Autor = "NoviAutor",
            Zanr = "NoviZanr",
            GodinaIzdavanja = 1900,
            Cena = 1500
        };
        var rezultat = await _servis.IzmenaKnjigaPoIdAsync(prvaKnjigaPrikaz.Id, dto);
        Assert.True(rezultat);
        _baza.ChangeTracker.Clear();
        var pregledBaze = await _baza.Knjige.AsNoTracking().SingleAsync();

        Assert.Equal("NoviNaslov", pregledBaze.NaslovKnjige);
        Assert.Equal("NoviAutor", pregledBaze.Autor);
        Assert.Equal("NoviZanr", pregledBaze.Zanr);
        Assert.Equal(1900, pregledBaze.GodinaIzdavanja);
        Assert.Equal(1500, pregledBaze.Cena);
    }

    //IZBRISI KNJIGU PO ID
    [Fact]
    public async Task IzbrisiKnjiguPoIdAsync_NijePronadjenaKnjiga_VracaNijeNadjena()
    {
        var prvaKnjigaPrikaz = await NovaKnjiga("primerNaslov", "primerAutor", "primerZanr", 2000, 1000);
        var rezultat = await _servis.IzbrisiKnjiguPoIdAsync(prvaKnjigaPrikaz.Id + 1);
        Assert.Equal(BrisanjeIshod.NijeNadjena, rezultat);

        _baza.ChangeTracker.Clear();
        var pregledBaze = await _baza.Knjige.AsNoTracking().SingleAsync();
        Assert.Equal("primerNaslov", pregledBaze.NaslovKnjige);
        Assert.Equal("primerAutor", pregledBaze.Autor);
        Assert.Equal("primerZanr", pregledBaze.Zanr);
        Assert.Equal(2000, pregledBaze.GodinaIzdavanja);
        Assert.Equal(1000, pregledBaze.Cena);
    }
    [Fact]
    public async Task IzbrisiKnjiguPoIdAsync_KadJeKnjigaPozajmljenaNull_VracaImaPozajmicu()
    {
        var prvaKnjigaPrikaz = await NovaKnjiga("primerNaslov", "primerAutor", "primerZanr", 2000, 1000);
        var korisnik = await NoviKorisnik("Test","TestP","test@example.com");
        var pozajmica=await NovaPozajmica(korisnik,prvaKnjigaPrikaz,null);
   

        var rezultat = await _servis.IzbrisiKnjiguPoIdAsync(prvaKnjigaPrikaz.Id);
        Assert.Equal(BrisanjeIshod.ImaPozajmicu, rezultat);
        _baza.ChangeTracker.Clear();
        Assert.True(await _baza.Knjige.AsNoTracking().AnyAsync(k => k.Id == prvaKnjigaPrikaz.Id));
    }

    [Fact]
    public async Task IzbrisiKnjiguPoIdAsync_KadJeKnjigaPozajmljenaDatumVracanja_VracaImaPozajmicu()
    {
        var prvaKnjigaPrikaz = await NovaKnjiga("primerNaslov", "primerAutor", "primerZanr", 2000, 1000);
         var korisnik = await NoviKorisnik("Test","TestP","test@example.com");
        var pozajmica=await NovaPozajmica(korisnik,prvaKnjigaPrikaz,DateTime.UtcNow.AddDays(5));
        var rezultat = await _servis.IzbrisiKnjiguPoIdAsync(prvaKnjigaPrikaz.Id);
        Assert.Equal(BrisanjeIshod.ImaPozajmicu, rezultat);
        _baza.ChangeTracker.Clear();
        Assert.True(await _baza.Knjige.AsNoTracking().AnyAsync(k => k.Id == prvaKnjigaPrikaz.Id));
    }
    [Fact]
    public async Task IzbrisiKnjiguPoIdAsync_KadKnjigaNemaPozajmicu_VracaUspeh()
    {
         var prvaKnjigaPrikaz = await NovaKnjiga("primerNaslov", "primerAutor", "primerZanr", 2000, 1000);
         var drugaKnjigaPrikaz = await NovaKnjiga("primerNaslovDva", "primerAutorDva", "primerZanrDva", 1900, 1500);
         var rezultat=await _servis.IzbrisiKnjiguPoIdAsync(prvaKnjigaPrikaz.Id);
         Assert.Equal(BrisanjeIshod.Uspeh,rezultat);
        _baza.ChangeTracker.Clear();
        Assert.True(await _baza.Knjige.AsNoTracking().AnyAsync(k=>k.Id==drugaKnjigaPrikaz.Id));
        Assert.False(await _baza.Knjige.AsNoTracking().AnyAsync(k=>k.Id==prvaKnjigaPrikaz.Id));
    }
}
