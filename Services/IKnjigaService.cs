using BibliotekaIznajmljivanje.Api.Data;
using BibliotekaIznajmljivanje.Api.Dtos;

namespace BibliotekaIznajmljivanje.Api.Services;

public interface IKnjigaService
{
  Task<KnjigaDto?> DajKnjiguPoIdAsync(int id);

  Task<KnjigaDto> DodajKnjiguAsync(KnjigaRequestDto createDto);
  Task<KnjigaPrikazStavkiDto> PrikaziSveKnjigeAsync(string? search, int page, int pageSize);
  Task<bool> IzmenaKnjigaPoIdAsync(int id, KnjigaRequestDto izmeniDto);

  Task<bool> IzbrisiKnjiguPoIdAsync(int id);  
    
}
