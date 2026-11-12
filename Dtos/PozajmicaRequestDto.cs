using System.ComponentModel.DataAnnotations;

namespace BibliotekaIznajmljivanje.Api.Dtos;

public class PozajmicaRequestDto
{
    [Range(1,int.MaxValue)]
    public int KnjigaId{get;set;}
    
    [Range(1,int.MaxValue)]
    public int KorisnikId{get;set;}
    

}
