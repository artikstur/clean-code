using System.ComponentModel.DataAnnotations;

namespace API.Contracts.Requests;

public record LoginUserRequest(
    [Required] string Email,
    [Required] string Password);