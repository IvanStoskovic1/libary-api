# Library API

REST API for managing a library: books, members, and book loans. Built with ASP.NET Core minimal APIs as a learning project, with a focus on clean layering and real business rules rather than plain CRUD.

## Tech stack

- ASP.NET Core 10 (minimal API)
- Entity Framework Core with SQLite (code-first migrations)
- Dependency Injection and a service layer
- DTOs for requests and responses
- Validation with DataAnnotations

## Features

- Full CRUD for books and users
- Book search (title or author) and pagination
- Borrowing and returning books with business rules:
  - a book cannot be borrowed twice at the same time
  - a loan cannot be returned twice
  - 404 for a missing user or book, 409 for conflicts
- Loan history per user and a list of overdue loans
- Unique, normalized (trimmed, lowercase) user emails
- Consistent HTTP status codes (200, 201, 204, 400, 404, 409)

## Project structure

| Folder | Responsibility |
|---|---|
| `EndPoints` | HTTP layer: routes and status codes |
| `Services` | Business logic and database access |
| `Data` | `DbContext` and model configuration |
| `Models` | Entities (`Knjiga`, `Korisnik`, `Pozajmica`) |
| `Dtos` | Request/response objects |
| `Extensions` | Service registration helpers |
| `Migrations` | EF Core migrations |

Endpoints only translate HTTP to service calls and results back to HTTP. All rules live in the services.

## Getting started

Prerequisites:

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- EF Core CLI tool: `dotnet tool install --global dotnet-ef`

Run the project:

```bash
git clone https://github.com/IvanStoskovic1/biblioteka-api.git
cd biblioteka-api
dotnet ef database update
dotnet run
```

The SQLite database file (`biblioteka.db`) is created automatically by the migration. Example requests are in `zahtevi.http` (works with the VS Code REST Client extension or Visual Studio).

## API endpoints

### Books (`/knjige`)

| Method | Path | Description |
|---|---|---|
| GET | `/knjige?search=&page=&pageSize=` | Paged list with optional search |
| GET | `/knjige/{id}` | Get one book |
| POST | `/knjige` | Create a book |
| PUT | `/knjige/{id}` | Update a book |
| DELETE | `/knjige/{id}` | Delete a book |

### Users (`/korisnici`)

| Method | Path | Description |
|---|---|---|
| GET | `/korisnici` | List users |
| GET | `/korisnici/{id}` | Get one user |
| POST | `/korisnici` | Create a user (409 if email exists) |
| PUT | `/korisnici/{id}` | Update first and last name |
| DELETE | `/korisnici/{id}` | Delete a user |

### Loans (`/pozajmice`)

| Method | Path | Description |
|---|---|---|
| POST | `/pozajmice` | Borrow a book (`korisnikId`, `knjigaId`) |
| PUT | `/pozajmice/{id}/vracanje` | Return a book |
| GET | `/pozajmice` | List all loans |
| GET | `/pozajmice/{id}` | Get one loan |
| GET | `/pozajmice/korisnik/{id}` | All loans of one user |
| GET | `/pozajmice/kasni` | Overdue, not yet returned loans |

Example request body for borrowing a book:

```json
{ "korisnikId": 1, "knjigaId": 2 }
```

The loan period defaults to 14 days.

