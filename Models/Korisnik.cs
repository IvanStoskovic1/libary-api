namespace BibliotekaIznajmljivanje.Api.Models;

public class Korisnik
{
    public int Id{get;set;}
    public string Ime{get;set;}="";
    public string Prezime{get;set;}="";
    public string Email{get;set;}="";
    public DateTime DatumRegistracije{get;set;}=DateTime.UtcNow;
}
