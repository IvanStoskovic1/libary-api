using BibliotekaIznajmljivanje.Api.Models;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

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
        modelBuilder.Entity<Pozajmica>().HasOne(p=>p.Knjiga).WithMany().HasForeignKey(p=>p.KnjigaId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Pozajmica>().HasOne(p=>p.Korisnik).WithMany().HasForeignKey(p=>p.KorisnikId).OnDelete(DeleteBehavior.Restrict);

    }

    public DbSet<Knjiga> Knjige=>Set<Knjiga>();
    public DbSet<Korisnik> Korisnici=>Set<Korisnik>();

    public DbSet<Pozajmica> Pozajmica=>Set<Pozajmica>();


}
