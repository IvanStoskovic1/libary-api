using BibliotekaIznajmljivanje.Api.EndPoints;
using BibliotekaIznajmljivanje.Api.Extensions;
using BibliotekaIznajmljivanje.Api.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddBibliotekaDataBase(builder.Configuration);
builder.Services.AddScoped<IKnjigaService, KnjigaService>();
builder.Services.AddScoped<IKorisnikService,KorisnikService>();
builder.Services.AddScoped<IPozajmicaService,PozajmicaService>();
builder.Services.AddValidation();

builder.Services.AddOpenApi();



builder.Services.AddProblemDetails();  
var app = builder.Build();
app.UseExceptionHandler(); 


app.MapKnjige();
app.MapKorisnik();
app.MapPozajmica();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.Run();


