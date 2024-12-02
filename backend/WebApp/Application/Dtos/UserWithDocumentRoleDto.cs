using Core.Enums;

namespace Application.Dtos;

public class UserWithDocumentRoleDto(string userName, Guid userId, DocumentRole documentRole)
{
    public string UserName { get; set; } = userName;
    public Guid UserId { get; set; } = userId;
    public DocumentRole DocumentRole { get; set; } = documentRole;
}