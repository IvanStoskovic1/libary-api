namespace BibliotekaIznajmljivanje.Api.Dtos;

public record  PozajmicaDto(
    int Id,
    int KorisnikId,
    string ImeKorisnika,
    int KnjigaId,
    string NaslovKnjige,
    DateTime DatumPreuzimanja,
    DateTime RokVracanja,
    DateTime? DatumVracanja
    );

