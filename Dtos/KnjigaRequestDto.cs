using System.ComponentModel.DataAnnotations;

namespace BibliotekaIznajmljivanje.Api.Dtos;

public class KnjigaRequestDto
{
  [Required][StringLength(50)] 
  public string NaslovKnjige{get;set;}=string.Empty;

  [Required][StringLength(30)] 
  public string Autor{get;set;}=string.Empty;

  [Required][StringLength(20)] 
  public string Zanr{get;set;}=string.Empty;

  [Required][Range(1,2100)] 
  public int GodinaIzdavanja{get;set;}

  [Required][Range(100,20000)] 
  public decimal Cena{get;set;}


}
