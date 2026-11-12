using BibliotekaIznajmljivanje.Api.Dtos;

namespace BibliotekaIznajmljivanje.Api.Services;

public interface IKorisnikService
{
    Task<List<KorisnikDto>> PrikazSvihKorisnikaAsync();
    Task<KorisnikDto?> PrikazKorisnikaPoIdAsync(int id);
    Task<KorisnikDto?> KreiranjeKorisnikaAsync(KorisnikRequestDto noviDto);
    Task<bool> IzmenaKorisnikaPoIdAsync(int id,KorisnikUpdateDto izmenjenKorisnik);
    Task<bool> BrisanjeKorisnikaPoIdAsync(int id);

}
