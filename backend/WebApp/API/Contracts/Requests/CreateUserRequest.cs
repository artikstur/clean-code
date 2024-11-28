using System.ComponentModel.DataAnnotations;

namespace API.Contracts.Requests;

public record CreateUserRequest(
    [Required] string UserName, 
    [Required] string Email,
    [Required] string Password);