using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BibliotekaIznajmljivanje.Api.Dtos;

public record  KnjigaDto(int Id,string NaslovKnjige,string Autor,string Zanr,int GodinaIzdavanja,decimal Cena);
