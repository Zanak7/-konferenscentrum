# Konferenscentrum Väst – API

Ett ASP.NET Core Web API för bokningshantering av konferensanläggningar, 
som hanterar kunder, lokaler, bokningar och avtal. 
Projektet använder **PostgreSQL + EF Core**, **JWT-auth**, **Swagger**, **Azure Key Vault**, **Azure Functions**, 
**Azure Communication Services** och **Azure Blob Storage**. 
API:et har även stöd för **e-postnotifieringar vid bokningsärenden** 
samt **filuppladdning av kontrakt och bilder**.

## Krav

* .NET 8 SDK
* PostgreSQL (lokalt eller moln)

## Bygga & köra

```bash
# i repo-rot
dotnet restore
dotnet build
dotnef ef database update # Körs endast om du behöver skapa tabeller i en egen databas
dotnet run
```

API nås på: `https://localhost:****`
Swagger: `/swagger`

## Autentisering

1. Registrera användare: `POST /api/auth/register`
2. Logga in: `POST /api/auth/login` → får JWT
3. Klicka **Authorize** i Swagger och ange: `Bearer <din-token>`
