namespace BibliotekaIznajmljivanje.Api.Dtos;

public record  KnjigaPrikazStavkiDto(List<KnjigaDto> Stavke, int Page,int PageSize,int UkupnoStavki);

