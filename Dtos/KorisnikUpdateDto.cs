using System.ComponentModel.DataAnnotations;
namespace BibliotekaIznajmljivanje.Api.Dtos;

public class KorisnikUpdateDto
{
 
    [Required]
    [StringLength(15)]
    public string Ime{get;set;}=string.Empty;

    [Required]
    [StringLength(25)]
    public string Prezime{get;set;}=string.Empty;


}
