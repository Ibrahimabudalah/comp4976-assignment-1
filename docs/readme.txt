Municipal Elections Management System (MEMS)
COMP 4976 - Assignment 1
BCIT - Computer Systems Technology



##PROJECT DESCRIPTION

MEMS is an ASP.NET Core MVC (.NET 10) web application
for managing municipal elections.

Administrators log in to manage municipalities,
positions, and candidates. Super Admin users manage
every municipality and every administrator account.
Municipality admin users manage only the municipality
assigned to them.

Public users can browse municipalities and positions,
search candidates by name, filter by municipality and
position, view candidate profiles, compare candidates
side by side, and build a voting guide they can print
or download as a PDF, all without logging in.

Technologies: ASP.NET Core MVC, Entity Framework Core
Code First, SQLite, ASP.NET Core Identity, Bootstrap,
QuestPDF, Azure App Service.


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
- Expanded seed data.
- Bonus features: dark mode, QuestPDF voting guide
  export, administrator and admin group management,
  candidate comparison, and mobile-first layout.


##BONUS FEATURES

1. Dark mode: a navbar toggle on public and admin
   pages. It follows the device theme by default,
   remembers the choice, and prints in light mode.
2. QuestPDF export: "Download PDF" on My Guide creates
   a PDF of the voting guide on the server, grouped by
   municipality, with candidate photos.
3. User / Role Management: Super Admin can add, edit,
   and delete administrators, reset passwords, and
   assign each one to Super Admin or a municipality
   admin group. Every municipality gets its own
   "{Name} Admin" group, which is created, renamed, and
   deleted together with the municipality.
4. Candidate comparison: tick 2 or 3 candidates on the
   Candidates page and press "Compare selected" to see
   them side by side.
5. Mobile-first enhancements: tables turn into
   labelled cards on phones, buttons are at least 44px
   tall on phones, and the navbar collapses on tablets.
   Tested at common phone widths from 320px to 932px.


##INCOMPLETE FEATURES

- Azure deployment: the application is configured for
  Azure App Service, but the team did not have
  permission to deploy it at the time of submission.


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
- Keeping admin groups in sync with municipalities.
  Identity compares role names without case, so
  municipality names must be unique regardless of case,
  and renaming a municipality renames its group so its
  admins keep access.


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
   first start. Seed data: Pitt Meadows, Maple Ridge,
   and Burnaby, 9 positions, and 14 candidates. Admin
   groups: Super Admin, Pitt Meadows Admin, Maple Ridge
   Admin, and Burnaby Admin.

4. Seeded admin users:

   Username   Group                Password
   super      Super Admin          P@$$w0rd
   pm         Pitt Meadows Admin   P@$$w0rd

5. Suggested checks:
   - As a visitor, browse Municipalities and
     Candidates, search and filter, open a profile,
     add candidates to My Guide, print the guide, and
     press "Download PDF".
   - On Candidates, tick 2 or 3 candidates and press
     "Compare selected".
   - Use the navbar toggle to switch dark mode on and
     off.
   - Log in as super: manage all municipalities,
     positions, and candidates, including logo and
     photo uploads (JPG, PNG, or WebP, 2 MB maximum).
   - As super, open Dashboard > Manage Administrators,
     add an admin for Maple Ridge, log in as that user,
     and confirm only Maple Ridge data can be changed.
   - Log in as pm: only Pitt Meadows positions and
     candidates are visible, and municipalities and
     administrators cannot be managed.
   - Resize the browser to phone width: admin tables
     become cards and nothing scrolls sideways.

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
