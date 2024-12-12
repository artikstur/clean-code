using System.ComponentModel.DataAnnotations;

namespace API.Contracts.Requests;

public record MdPushRequest(
    [Required] string NewContent,
    [Required] Guid DocumentId);