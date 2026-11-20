# GitHub Copilot Instructions

## Project Overview

This project is the Municipal Elections Management System (MEMS), built for COMP 4976
Assignment 2 at BCIT. It is a **multi-tenant** system: every municipality is a tenant,
identified by a URL slug (`/burnaby`, `/surrey`, `/richmond`).

- **Administrators** manage municipalities, positions, candidates and admin users in one shared
  Blazor Server portal.
- **Public visitors** browse each municipality's candidates on a Blazor WebAssembly site, and
  build a personal voting guide that never leaves their browser.

## Solution Structure

`MEMS.sln` contains the seven projects under `src/`. The `MunicipalElectionsManagementSystem/`
folder is the old Assignment 1 MVC app: do not change it or copy its patterns.

| Project | Role |
|---|---|
| `MEMS.AppHost` | .NET Aspire orchestration: SQL Server, API, Admin, Public |
| `MEMS.ServiceDefaults` | OpenTelemetry, health checks, service discovery, resilience |
| `MEMS.Admin` | Blazor Server admin portal (interactive server render mode) |
| `MEMS.Public` | Blazor WebAssembly public site (no database access) |
| `MEMS.Api` | ASP.NET Core Web API, JWT auth, OpenAPI and Scalar |
| `MEMS.Data` | EF Core `ApplicationDbContext`, entities, migrations, repositories, seeding |
| `MEMS.Shared` | DTOs with validation attributes, `AdminRoles` constants |

## Required Technologies

- .NET 10, C#
- .NET Aspire (AppHost and ServiceDefaults)
- Blazor Server (admin) and Blazor WebAssembly (public)
- ASP.NET Core Web API
- SQL Server with Entity Framework Core Code First. **SQLite is not allowed.**
- ASP.NET Core Identity (cookies in the admin portal, JWT bearer in the API)
- Bootstrap 5

## Multi-Tenancy Rules

- Every Position, Candidate and admin user belongs to exactly one Municipality
  (`MunicipalityId`). Tenant slugs are unique, lowercase and match `^[a-z0-9-]+$`.
- Filter by tenant **inside the EF query** (`Where(c => c.Municipality.TenantSlug == slug)`),
  never after loading all rows.
- **API:**
  - Tenant routes are `api/{tenantSlug}/...`.
  - A record from another tenant returns 404.
  - Every write checks `CanWriteToTenantAsync`, and every read checks `CanReadTenantAsync`.
  - A signed-in Municipality Admin gets 403 for any other tenant. Anonymous reads stay open
    for the public site.
- **Admin portal:**
  - Get the tenant from the signed-in user through `TenantAuthorizationService`, never from
    the URL or a form field.
  - Municipality and user management are Super Admin only.
- **Public site:**
  - Take the tenant from the first URL segment (`TenantShell`). Only call
    `/api/{active slug}/...`.
  - Slugs match case-insensitively. An unknown slug shows "Municipality not found".
- A new municipality must work with no code change. Never hard-code tenant names or IDs.

## Election Privacy Rules (do not break)

- Voting guide selections live **only** in browser `sessionStorage` (`Blazored.SessionStorage`,
  key `mems-guide:{slug}`).
- Never add a table, entity, endpoint, log line or telemetry attribute that stores or sends
  ballot selections.
- `VotingGuideService` must never depend on `HttpClient` or `MemsApiClient`.
- Guide and compare pages load the tenant's full candidate list and filter in the browser, so
  no request reveals what a voter picked.
- A voter can pick at most `SeatsAvailable` candidates per position, from one municipality only.

## Authentication and Authorization

- Roles:
  - `AdminRoles.SuperAdmin` has full access.
  - Municipality Admins have `AdminRoles.MunicipalityAdmin` plus `AdminRoles.ForMunicipality(name)`.
- Check every `IdentityResult` from `UserManager` and `RoleManager`. Show or log failures;
  never ignore them.
- The JWT signing key comes from the AppHost's `jwt-key` secret parameter. Never put keys,
  passwords or connection strings in source.
- The public site has no login.

## Coding Guidelines

- PascalCase for types, methods and properties; camelCase for locals.
- Use async EF Core calls. Data access goes through the `MEMS.Data` repositories.
- Put request and response shapes in `MEMS.Shared/Dtos`, with Data Annotations (`[Required]`,
  `[StringLength]`, `[EmailAddress]`, `[Url]`, `[Range]`). The same DTO validates the Blazor
  form and the API.
- Blazor forms use `EditForm`, `DataAnnotationsValidator` and `ValidationMessage`.
- Admin pages run interactively. Pages that need `HttpContext` (login, error) use
  `[ExcludeFromInteractiveRouting]`.
- Keep it simple: no new packages or abstractions unless a requirement needs them.

## Database

- SQL Server is provided by Aspire (`memsdb`). Never hard-code a connection string.
- After changing an entity, add a migration in `MEMS.Data`:
  `dotnet ef migrations add <Name> --project src/MEMS.Data --startup-project src/MEMS.Api`.
- Seed data lives in `MEMS.Data/Seeding/DbInitializer.cs`:
  - Burnaby, Surrey, Richmond and Pitt Meadows;
  - several positions per tenant;
  - at least 20 candidates;
  - users Super, super, Burnabyadmin, Surreyadmin, Richmondadmin and pm.
- Use `DeleteBehavior.Restrict` on tenant foreign keys.

## Logging and Error Handling

- Use structured `ILogger` templates (`"Failed login for {UserName}"`), not string
  concatenation.
- Never show `ex.Message` or stack traces to users. Log the exception and show a generic
  message:
  - API: `GlobalExceptionMiddleware` returns `ProblemDetails`.
  - Admin: catches errors in each page and shows a generic message.
  - Public: `ErrorBoundary` in `MainLayout`.

## User Interface

- Bootstrap 5, responsive and mobile-first; tap targets of at least 44px.
- Keep navigation consistent. Support light and dark mode (`data-bs-theme`).
- The public site shows each tenant's logo, name and accent colour.
- The printable guide shows the municipality, election date, candidate names, position names
  and print date. Printing is always in light mode.

## Security

- Validate on the client (Blazor), the API (`[ApiController]` model validation) and the
  database (entity constraints, unique indexes).
- Image uploads: `.jpg`, `.jpeg`, `.png`, `.webp` only, 2 MB maximum, saved under a GUID name.
- Never trust municipality, position or candidate IDs from the browser. Re-check the tenant
  on the server.

## Running

`dotnet run --project src/MEMS.AppHost --launch-profile http` (requires Docker).

| Resource | Address |
|---|---|
| Dashboard | `:15000` |
| API | `:5027` (Scalar at `/scalar/v1`) |
| Admin | `:5002` |
| Public | `:5089` |
