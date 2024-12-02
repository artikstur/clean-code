using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Persistence.Entities;

public class UserEntity
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public ICollection<RoleEntity> Roles { get; set; } = new List<RoleEntity>();
    public ICollection<DocumentEntity> AllowedToEditDocuments { get; set; } = new List<DocumentEntity>();
    public ICollection<DocumentEntity> AllowedToReadDocuments { get; set; } = new List<DocumentEntity>();
    public ICollection<DocumentEntity> PersonalDocuments { get; set; } = new List<DocumentEntity>();
}