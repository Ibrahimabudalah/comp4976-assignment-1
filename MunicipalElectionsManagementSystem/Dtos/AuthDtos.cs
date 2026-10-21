using System.ComponentModel.DataAnnotations;

namespace MunicipalElectionsManagementSystem.Dtos;

public record RegisterRequest(
    [Required, StringLength(100)] string Name,
    [Required, EmailAddress, StringLength(254)] string Email,
    [Required, MinLength(8)] string Password);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record TokenResponse(string Token);

public record UserResponse(string Name, string Email);
