---
name: aspnet-mvc
description: Guidelines for developing ASP.NET Core MVC features in MEMS.
---

# ASP.NET Core MVC Development Skill

Use this skill when implementing controllers, models,
Razor views, validation, and database operations.

## Development Process

1. Identify the feature requirements.
2. Create or update the relevant model.
3. Configure relationships using EF Core.
4. Create ViewModels when needed.
5. Implement controller actions.
6. Create Razor views using Bootstrap.
7. Add input validation.
8. Add authentication and authorization checks.
9. Test the feature before committing.

## Controller Guidelines

- Keep actions focused on one responsibility.
- Use dependency injection.
- Use async EF Core methods.
- Return NotFound for missing records.
- Verify user permissions before changing data.
- Use POST requests for changes to data.
- Use antiforgery validation for form submissions.

## Database Guidelines

- Use EF Core Code First.
- Create migrations when the model changes.
- Avoid raw SQL unless necessary.
- Define clear entity relationships.
- Seed the required assignment data.

## UI Guidelines

- Use Razor Views.
- Use Bootstrap components.
- Support mobile screens.
- Keep layouts consistent.
- Support light and dark themes.

## Testing

Verify that:

- Valid inputs are accepted.
- Invalid inputs display validation messages.
- Anonymous users cannot access admin pages.
- Municipality admins cannot modify other municipalities.
- Super Admin users can manage all municipalities.
- Public pages work without authentication.