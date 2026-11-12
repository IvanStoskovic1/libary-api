using BibliotekaIznajmljivanje.Api.Data;

namespace BibliotekaIznajmljivanje.Api.Models;

public class Pozajmica
{
    public int Id{get;set;}
    public int KorisnikId{get;set;}
    public Korisnik Korisnik{get;set;}=null!;
    public int KnjigaId{get;set;}
    public Knjiga Knjiga{get;set;}=null!;
    public DateTime DatumPreuzimanja{get;set;}=DateTime.UtcNow;
    public DateTime RokVracanja { get; set; }=DateTime.UtcNow.AddDays(14);
    public DateTime? DatumVracanja { get; set;}
}
