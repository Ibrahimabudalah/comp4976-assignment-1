Municipal Elections Management System (MEMS)
COMP 4976 - Assignment 2
BCIT - Computer Systems Technology



##PROJECT DESCRIPTION

MEMS is a multi-tenant, cloud-native .NET 10 system
for managing municipal elections, orchestrated with
.NET Aspire. Each municipality is a tenant, identified
by a URL segment (its slug): /burnaby, /surrey,
/richmond. A new municipality needs no new
application: it is just a new tenant created in the
admin portal.

- Admin portal (Blazor Server): one portal shared by
  every tenant. Super Admin manages all municipalities,
  positions, candidates and administrators. A
  municipality admin manages only their own
  municipality's positions and candidates.
- Public website (Blazor WebAssembly): a separate
  experience per tenant. Visitors browse
  municipalities, positions and candidates, search by
  name, filter by municipality and position, view
  profiles and photos, compare candidates, and build a
  printable personal voting guide, all without logging
  in.
- Web API (ASP.NET Core): all public data goes through
  it. It is tenant-scoped (/api/{slug}/...), uses JWT
  authentication, and is documented with OpenAPI and
  Scalar.
- Election privacy: voting guide selections stay in
  the browser's session storage. They are never sent
  to the API, never stored in SQL Server, and are
  cleared when the browser session ends.

Projects (MEMS.sln, all under src/):
  MEMS.AppHost          .NET Aspire orchestration
  MEMS.ServiceDefaults  telemetry, health checks,
                        service discovery
  MEMS.Admin            Blazor Server admin portal
  MEMS.Public           Blazor WebAssembly public site
  MEMS.Api              ASP.NET Core Web API
  MEMS.Data             EF Core, migrations,
                        repositories, seeding
  MEMS.Shared           DTOs and shared constants

Technologies: .NET 10, .NET Aspire, Blazor Server,
Blazor WebAssembly, ASP.NET Core Web API, Entity
Framework Core Code First, SQL Server, ASP.NET Core
Identity, JWT bearer, OpenTelemetry, Bootstrap 5,
Blazored.SessionStorage.

The full write-up (architecture diagram, ERD, API
documentation, deficiency resolution report, Copilot
usage, screenshots) is in the separate documentation
file submitted with this project.


##STUDENT CONTRIBUTIONS

Ibrahim Abdulah
- .NET Aspire solution: AppHost orchestration of SQL
  Server, API, admin portal and public site;
  ServiceDefaults (OpenTelemetry, health checks).
- SQL Server data layer: EF Core entities, migrations,
  repositories, and seed data.
- MEMS.Api: tenant-scoped endpoints for
  municipalities, positions and candidates; JWT login;
  write authorization per tenant; global exception
  handling; OpenAPI and Scalar.
- MEMS.Admin: Blazor Server conversion of every
  Assignment 1 admin feature (dashboard, tenant,
  position, candidate and user management, logo and
  photo uploads), with tenant scoping, account lockout
  and access-denied handling.
- Assignment 1 deficiency fixes in the admin portal
  and API: checked role results, initial admin when
  creating a municipality, tenant-only admin views.

Steven Doan
- MEMS.Public: Blazor WebAssembly public site with
  tenant resolution from the URL, per-tenant branding,
  municipality, position and candidate directories,
  search and filters, candidate profiles and
  comparison.
- Personal voting guide in session storage, with the
  seat limit per position, one municipality per guide,
  and a printable / save-as-PDF layout. Fixes the three
  Assignment 1 voting guide deductions.
- Election privacy design: no ballot data on the
  server, no selections in any API request.
- Dark mode, mobile-first layout, accessibility
  checks, and friendly error handling on the public
  site.
- Testing and review of the full stack (handoff and
  issue lists for the admin portal and API), plus the
  JWT signing-key fix and tenant-scoped API reads.
- Documentation: system documentation, deficiency
  resolution report, Copilot instructions, and this
  readme.


##BONUS FEATURES

1. Dark mode: a navbar toggle on the public site. It
   follows the device theme by default, remembers the
   choice, and always prints in light mode.
2. Candidate comparison: tick 2 or 3 candidates on the
   Candidates page and press "Compare selected" to
   see them side by side. It runs in the browser, so
   no picks are sent to the server.
3. Per-tenant branding: each municipality's public site
   has its own accent colour (generated from its slug),
   logo and header. New tenants get one automatically.
4. User / role management: Super Admin can add, edit
   and delete administrators, assign each one to a
   municipality, and create a municipality's first
   administrator while creating the municipality.
5. Mobile-first enhancements: tables turn into
   labelled cards on phones, tap targets are at least
   44px, and nothing scrolls sideways from 320px wide.

Note: the Assignment 1 server-side QuestPDF export was
replaced by the browser's "Print / Save as PDF".
Building the PDF on the server would mean sending the
voter's selections to the backend, which the
election-privacy requirements forbid.


##INCOMPLETE FEATURES

- N/A. Two deliberate design choices are explained in
  the documentation:
  - The WebAssembly public site runs in the browser,
    so it can't read Aspire's service-discovery
    environment variables. Its API address is set in
    MEMS.Public/wwwroot/appsettings.json, matching the
    port the AppHost starts the API on.
  - The admin portal runs as its own application on
    its own port (http://localhost:5002) rather than
    under a /admin path.


##MAJOR CHALLENGES

- Tenant isolation in every layer. The public site only
  requests its own tenant; the API checks the token's
  tenant on every write and read (403 on a mismatch);
  the admin portal gets the tenant from the signed-in
  user, never from the URL; and EF queries filter by
  tenant in the database.
- Election privacy. The voting guide service has no
  access to the HTTP client, and the guide and compare
  pages load the tenant's whole candidate list and
  filter in the browser, so no request reveals what a
  voter picked.
- The spec conflict between "local storage" and
  "Session Storage". We used session storage, because
  local storage would keep ballot data after the
  browser closes.
- Running .NET Aspire 9 on .NET 10, and making the
  Blazor Server admin interactive while the login and
  error pages stay server-rendered (they need
  HttpContext).
- Keeping secrets out of source control. The SQL
  password and the JWT signing key are Aspire secret
  parameters, and the JWT key is generated on each run
  instead of being written in the code.


##TESTING INSTRUCTIONS

1. Install the .NET 10 SDK and Docker Desktop. Start
   Docker.

2. From the repository root, run:

   dotnet run --project src/MEMS.AppHost --launch-profile http

   No configuration files or secrets are needed. On
   first start, SQL Server is started in Docker, and
   the database is created by EF Core migrations and
   seeded: Burnaby, Surrey, Richmond and Pitt Meadows,
   12 positions, and 25 candidates.

3. Open:

   Aspire dashboard  http://localhost:15000
                     (use the login link printed in
                     the console)
   Public site       http://localhost:5089
   Admin portal      http://localhost:5002
   API (Scalar)      http://localhost:5027/scalar/v1

4. Seeded admin users (all passwords: P@$$w0rd):

   Username        Role                Tenant
   Super / super   Super Admin         All
   Burnabyadmin    Municipality Admin  Burnaby
   Surreyadmin     Municipality Admin  Surrey
   Richmondadmin   Municipality Admin  Richmond
   pm              Municipality Admin  Pitt Meadows

5. Suggested checks:
   - Public: open /burnaby and /surrey and compare
     their look. Browse positions and candidates,
     search by name, filter by position and
     municipality, and open a profile.
   - Public: tick 2 or 3 candidates and press "Compare
     selected".
   - Voting guide: add candidates and open My Guide.
     Try to add a second mayor: it is blocked with the
     reason. Print the guide or save it as a PDF.
     Close the tab and open the site again: the guide
     is empty.
   - Public: try /vancouver (municipality not found),
     and a Surrey candidate ID under /burnaby/candidates
     (not found).
   - Admin as Super: dashboard totals for all tenants.
     Create a municipality with a slug and an initial
     administrator, then log in as that administrator.
   - Admin as Burnabyadmin: only Burnaby positions and
     candidates are visible. Municipalities and Users
     show Access Denied. Upload a candidate photo and
     see it on the public site.
   - API in Scalar: POST /api/auth/login as
     Burnabyadmin, then use the token.
     GET /api/burnaby/candidates returns 200;
     GET /api/surrey/candidates returns 403;
     POST /api/surrey/positions returns 403.
   - Aspire dashboard: resources are healthy, and
     structured logs, traces and metrics appear for the
     api and admin.
   - Resize the browser to phone width: nothing scrolls
     sideways.

6. If SQL Server login fails on startup, an old SQL
   data volume from an earlier build is using a
   different password. Delete the
   mems.apphost-...-sqlserver-data volume in Docker
   Desktop and run again.

7. API tokens last 8 hours. They are signed with a key
   that is generated each time the AppHost starts, so
   log in again after a restart.


##GITHUB REPOSITORY

https://github.com/Ibrahimabudalah/comp4976-assignment-1/tree/assignment-2
