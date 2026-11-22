using BibliotekaIznajmljivanje.Api.Dtos;

namespace BibliotekaIznajmljivanje.Api.Services;

public interface IPozajmicaService
{
    Task<PozajmicaIshod> VracanjeKnjigeAsync(int id);
    Task<List<PozajmicaDto>> PrikazSvihPozajmicaAsync();
    Task<PozajmicaDto?> PrikazJednePozajmicePoIdAsync(int id);

    Task<List<PozajmicaDto>?> PrikazSvihPozajmicaJednogKorisnikaAsync(int id);
    Task<List<PozajmicaDto>> PrikazPozajmicaKojeKasneAsync();
    Task<PozajmicaRezultat> PozajmljivanjeKnjigaAsync(PozajmicaRequestDto noviDto);

}
