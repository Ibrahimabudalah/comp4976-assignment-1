# GitHub Copilot Instructions

## Project Overview

This project is a Municipal Elections Management System (MEMS)
developed for COMP 4976 at BCIT.

The application allows administrators to manage municipalities,
elected positions, and candidates. Public users can browse
candidates and create printable voting guides.

## Required Technologies

- ASP.NET Core MVC (.NET 10)
- C#
- Entity Framework Core Code First
- SQLite
- ASP.NET Core Identity
- Razor Views
- Bootstrap
- Azure App Service

## Coding Guidelines

- Follow the MVC architecture.
- Keep controllers organized and easy to understand.
- Use PascalCase for classes, methods, and properties.
- Use camelCase for local variables.
- Use asynchronous EF Core database operations.
- Use Data Annotations for model validation.
- Use ViewModels when forms require additional fields.
- Keep the application simple and maintainable.

## Authentication and Authorization

- Use ASP.NET Core Identity for authentication.
- Super Admin users can manage all municipalities.
- Municipality admins can only manage their assigned municipality.
- Public visitors do not need to log in.
- Protect admin controllers and actions with authorization.
- Verify municipality access on the server for every data operation.

## Database

- Use SQLite and EF Core Code First migrations.
- Store the development connection string in
  appsettings.Development.json.
- Never commit appsettings.Development.json.
- Seed municipalities, positions, candidates, users, and roles.

## User Interface

- Use Bootstrap for responsive layouts.
- Support light mode and dark mode.
- Support printable voting guides and PDF export.

## Security

- Validate user input on both client and server.
- Validate uploaded images by file type and size.
- Use antiforgery protection on modifying form actions.
- Never trust municipality or position IDs from the browser.
- Avoid hardcoding credentials or secrets.