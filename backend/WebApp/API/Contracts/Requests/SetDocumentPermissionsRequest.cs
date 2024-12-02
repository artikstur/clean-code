using System.ComponentModel.DataAnnotations;
using Core.Enums;

namespace API.Contracts.Requests;

public record SetDocumentPermissionsRequest(
    [Required] Guid UserId,
    [Required] DocumentRole DocumentRole);