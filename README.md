# Konferenscentrum Väst – API

Ett ASP.NET Core Web API för bokningshantering av konferensanläggningar (kunder, lokaler, bokningar och avtal).
Projektet använder **PostgreSQL + EF Core**, **JWT-auth**, **Swagger**, **Azure Key Vault** och **Azure Blob Storage**.

## Krav

* .NET 8 SDK
* PostgreSQL (lokalt eller moln)
* Azure CLI (för lokala Key Vault-credentials)
* (Valfritt i prod) Managed Identity för åtkomst till Key Vault

## Konfiguration

Appen läser hemligheter från **Azure Key Vault** (prod) eller **User Secrets/appsettings** (dev).

Nödvändiga nycklar:

* `DbConnectionString`
* `BlobConnectionString` eller `StorageConnectionString`
* `JwtSettings:Secret` (minst 256 bitar), `JwtSettings:Issuer`, `JwtSettings:Audience`

Lokalt:

```bash
az login
# lägg hemligheter i User Secrets eller appsettings.Development.json
```

## Bygga & köra

```bash
# i repo-rot
dotnet restore
dotnet build
dotnet ef database update   # skapar tabeller (inkl. Identity) mot DbConnectionString
dotnet run
```

API nås på: `https://localhost:****`
Swagger: `/swagger`

## Autentisering

1. Registrera användare: `POST /api/auth/register`
2. Logga in: `POST /api/auth/login` → får JWT
3. Klicka **Authorize** i Swagger och ange: `Bearer <din-token>`

## Deploy (översikt)

* Publicera: `dotnet publish -c Release -o ./output`
* (CI/CD) Zippa output och deploya till din App Service/Container
* Säkerställ Key Vault-access (Managed Identity) i produktion
