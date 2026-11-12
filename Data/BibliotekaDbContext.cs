using BibliotekaIznajmljivanje.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotekaIznajmljivanje.Api.Data;

public class BibliotekaDbContext:DbContext
{

    public BibliotekaDbContext(DbContextOptions<BibliotekaDbContext> options) :base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Korisnik>().HasIndex(k=>k.Email).IsUnique();
    }
    public DbSet<Knjiga> Knjige=>Set<Knjiga>();
    public DbSet<Korisnik> Korisnici=>Set<Korisnik>();

    public DbSet<Pozajmica> Pozajmica=>Set<Pozajmica>();


}
