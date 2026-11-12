namespace BibliotekaIznajmljivanje.Api.Data;

public class Knjiga
{
    public int Id{get;set;}
    public string NaslovKnjige{get;set;}="";
    public string Autor{get;set;}="";
    public string Zanr{get;set;}="";
    public int GodinaIzdavanja{get;set;}
    public decimal Cena{get;set;}

    
}
