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

`MEMS.sln` contains the seven projects under `src/`. The Assignment 1 MVC app has been removed
(it is in git history): do not reintroduce MVC controllers, Razor views or SQLite.

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

## GitHub Copilot usage

### Repository files
| File | Used in | Purpose |
|---|---|---|
| `.github/copilot-instructions.md` | A1, rewritten for A2 | Project-wide instructions: the seven Aspire projects, multi-tenancy rules for the API, admin and public site, election-privacy rules for the voting guide, Identity/JWT rules, validation, logging and run commands |
| `.github/skills/ponytail/SKILL.md` | A2 | "Smallest complete change" skill: reuse existing code and platform features, no unrequested abstractions or dependencies, never cut validation or security, leave a check for non-trivial logic. Third-party (MIT licence, by Dietrich Gebert, github.com/DietrichGebert/ponytail); its licence is kept beside it |
| `.github/skills/aspnet-mvc/SKILL.md` | A1 | Guidelines for MVC controllers, EF Core, validation and authorization |
| `.github/prompts/implement-phase.prompt.md` | A2 (replaces A1's `create-mvc-feature`) | Implement one phase of the implementation plan: read the plan and code first, keep the tenant and privacy rules, test every item on the phase's Check list in a real browser, log the work and design choices in the progress log, and propose a commit message for review |
| `.github/prompts/verify-partner-changes.prompt.md` | A2 | Verify a partner's pushed fixes against the handoff list without changing their code: run the stack, test each "Done when" condition with real logins and tokens, report ✅/⚠️/❌ with evidence, and raise new tickets |
| `.github/prompts/spec-audit.prompt.md` | A2 | Audit the solution against the spec and marking guide section by section, test the gaps and extra use cases, and write the remaining tasks with owner, reason and "Done when" |

### How AI assistance was used

We used the AI assistant as an agent in VS Code, guided by the files in 15.1. `copilot-instructions.md` gave it the project rules: the tenant boundaries, the privacy rules and the coding conventions. The ponytail skill kept every change as small as possible. The prompt files capture the three workflows that were used repeatedly:

- **Planning:** the spec and the Assignment 1 feedback were turned into one plan document. It holds the requirements, the deficiency table, a to-do list and a phased implementation plan, written with enough context that another model could follow it.
- **Building MEMS.Public phase by phase** (`implement-phase`):
  - Each phase was implemented, built, and tested in a real browser against the phase's Check list.
  - Each phase was then logged in the plan's progress log, with its design choices.
  - Each phase ended with a proposed commit message. Steven reviewed every change and made each commit himself.
- **Reviewing the partner's work without editing it** (`verify-partner-changes`):
  - The AppHost, API and admin portal were run and tested with real logins and JWT tokens.
  - Problems were written up as tickets (R1–R8), each with steps to reproduce, a fix and a "Done when" condition.
- **Pre-submission audits** (`spec-audit`): the solution was checked against the spec section by section. The remaining gaps were fixed or documented.
- **Documentation:**
  - the API examples in Section 9 come from captured real responses;
  - the Mermaid architecture diagram and ERD;
  - the deficiency report and the readme.
- **Debugging:** the blank Aspire dashboard and an admin portal crash, both traced to their root cause (15.5).

### Example prompts (real, quoted as typed)

| Prompt | Used for | Result |
|---|---|---|
| "ok let's start with phase 1 of the implementation plan, give me a commit message afterwards summarizing the changes so that i can review myself" | Building MEMS.Public one phase at a time | The phase was implemented and built, with a commit message to review. Became the `implement-phase` prompt |
| "and you've tested the functionality of this phase?" | Holding the assistant to evidence, not claims | Led to repeatable real-browser test scripts (layout, tap targets, contrast, privacy) that were re-run after every later phase. Became the testing section of `implement-phase` |
| "check to see if the admin portal fixes are correct, do not modify any of their code." | Reviewing the partner's admin portal | Found that no admin page was interactive (forms could not submit), plus seven more issues. All were sent as tickets R1–R8 and fixed by the partner |
| "ok check with the A2 documentation and A2 spec and see that all requirements have been fufilled. if not, write the remaining tasks out with its context in the documentation" | Final spec audit | Found that a Burnaby admin's token could still read Surrey's data through the API, the spec's own isolation example. It was then fixed with tenant-scoped reads (403). Also found the out-of-date Copilot files and readme |
| "nothing is loading" (about the Aspire dashboard) | Debugging | Traced to the Aspire 9.0 dashboard running on .NET 10, where `_framework/blazor.web.js` returns 404. Fixed by installing the .NET 8 runtime, and added to the readme as a prerequisite |

### Productivity improvements

- **Work could be resumed at any time.** The plan document kept the spec rules, the code facts and a progress log in one place, so each phase or review could start from where the last one stopped. That included a new session or a different model.
- **Testing was repeatable.** Browser test scripts were written once and re-run after every change, so regressions in layout, accessibility and privacy were caught straight away instead of by hand.
- **Partner review was precise.** Each issue came with exact steps to reproduce and a proposed fix, so fixes came back quickly and could be re-verified the same way.
- **Documentation was accurate.** API examples, status codes and seed counts were captured from the running system instead of typed by hand.
- **Changes stayed small.** The ponytail skill reused existing code (for example the API's existing tenant check for the read-isolation fix), so reviews were short.

### Problems encountered

- **A wrong diagnosis:** the blank Aspire dashboard was first put down to a limitation of the headless browser used in testing. Checking in a real browser showed it was a real bug (the dashboard running on .NET 10). Lesson: confirm in a real browser before calling something a tool quirk.
- **A fix that would have hurt the marker:** the first suggested fix for the hard-coded JWT key used user-secrets. That would have stopped a fresh clone from starting until a secret was set by hand. It was replaced with an Aspire parameter that generates the key, so the solution still runs with one command.
- **False alarms from test scripts:** one script's selector matched the admin's "Log out" form instead of the page's form, and another looked for a placeholder the input didn't have. Both were reported as script errors, not app bugs, and the tests were re-run.
- **A browser extension crashed the admin portal:** an extension (Grammarly) injected elements into the page, and Blazor Server's connection crashed. Reproducing in a clean browser proved the code was fine. The workaround is in the readme.
- **Renderer differences:** a Mermaid arrow label showed as "s18" in one previewer. The text was moved into the box, so it renders the same everywhere.
- **Prompts needed explicit boundaries.** Without "do not modify any of their code", a review could drift into editing the partner's projects. Putting the boundary in the prompt (and in `verify-partner-changes`) prevented that.

