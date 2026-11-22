using BibliotekaIznajmljivanje.Api.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BibliotekaIznajmljivanje.Tests;

public  class TestBaza
{
    public static BibliotekaDbContext MemorijskaBaza()
    {
        var konekcija=new SqliteConnection("DataSource=:memory:");
        konekcija.Open();

        var opcija=new DbContextOptionsBuilder<BibliotekaDbContext>().UseSqlite(konekcija).Options;

        var baza=new BibliotekaDbContext(opcija);
        baza.Database.EnsureCreated();
        return baza;
   
    }
}
