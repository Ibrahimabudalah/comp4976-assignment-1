Municipal Elections Management System (MEMS)
COMP 4976 - Assignment 1
BCIT - Computer Systems Technology



##PROJECT DESCRIPTION

MEMS is an ASP.NET Core MVC (.NET 10) web application
for managing municipal elections.

Administrators log in to manage municipalities,
positions, and candidates. Super Admin users manage
every municipality. Municipality admin users manage
only the municipality assigned to them.

Public users can browse municipalities and positions,
search candidates by name, filter by municipality and
position, view candidate profiles, and build a
printable voting guide without logging in.

Technologies: ASP.NET Core MVC, Entity Framework Core
Code First, SQLite, ASP.NET Core Identity, Bootstrap,
Azure App Service.


##STUDENT CONTRIBUTIONS

Ibrahim Abdulah
- Project setup, EF Core models, and initial migration.
- ASP.NET Core Identity, login, roles, and seeding.
- Admin area: dashboard and CRUD for municipalities,
  positions, and candidates.
- Image upload service and municipality-scoped
  authorization for admin users.

Steven Doan
- Configuration fixes: connection string and seed
  passwords in appsettings.Development.json.
- Public website: municipality pages, candidate
  search, filters, and profile pages.
- Session-based voting guide with printable layout.
- Navigation, colour scheme, and responsive layout.
- Azure deployment configuration (persistent database
  and upload folders).


##INCOMPLETE FEATURES

- Azure deployment: the application is configured for
  Azure App Service, but the team did not have
  permission to deploy it at the time of submission.
- Municipality admin groups: only the "Pitt Meadows
  Admin" group exists. The role name is fixed in code,
  so a new municipality does not get its own admin
  group automatically.
- No bonus features were implemented.


##MAJOR CHALLENGES

- Restricting municipality admins to their own data.
  Every admin query and form post checks the user's
  assigned municipality on the server, so changing an
  ID in the browser does not give access to other data.
- Keeping secrets out of source control while still
  letting the app seed users. The connection string and
  seed passwords live in appsettings.Development.json,
  which is git-ignored.
- Planning storage for Azure. App Service replaces
  wwwroot on each deploy, so the database and uploaded
  images are configured to live under /home instead.


##TESTING INSTRUCTIONS

1. Install the .NET 10 SDK.

2. appsettings.Development.json is not in the
   repository. Create it in the
   MunicipalElectionsManagementSystem folder with:

   {
     "ConnectionStrings": {
       "DefaultConnection": "Data Source=mems.db"
     },
     "SeedUsers": {
       "SuperPassword": "P@$$w0rd",
       "PmPassword": "P@$$w0rd"
     }
   }

3. Run the application from the
   MunicipalElectionsManagementSystem folder:

   dotnet run

   The database is created, migrated, and seeded on
   first start. Seed data: Pitt Meadows and Maple Ridge,
   5 positions, and 3 candidates.

4. Seeded admin users:

   Username   Group                Password
   super      Super Admin          P@$$w0rd
   pm         Pitt Meadows Admin   P@$$w0rd

5. Suggested checks:
   - As a visitor, browse Municipalities and
     Candidates, search and filter, open a profile,
     add candidates to My Guide, and print the guide.
   - Log in as super: manage all municipalities,
     positions, and candidates, including logo and
     photo uploads (JPG, PNG, or WebP, 2 MB maximum).
   - Log in as pm: only Pitt Meadows positions and
     candidates are visible, and municipalities cannot
     be created.

6. To reset the data, stop the app, delete mems.db,
   and run it again.


##AZURE DEPLOYMENT URL

Not deployed. See INCOMPLETE FEATURES.

Required App Service settings when deployed:
ConnectionStrings__DefaultConnection = Data Source=/home/data/mems.db
Uploads__Path = /home/uploads
SeedUsers__SuperPassword = P@$$w0rd
SeedUsers__PmPassword = P@$$w0rd


##GITHUB REPOSITORY

https://github.com/Ibrahimabudalah/comp4976-assignment-1
