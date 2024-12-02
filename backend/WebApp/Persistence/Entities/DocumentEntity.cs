namespace Persistence.Entities;

public class DocumentEntity
{
    public Guid DocumentId { get; set; }
    public Guid AuthorId { get; set; }
    public UserEntity Author { get; set; }
    public ICollection<UserEntity> AllowedToEditUsers { get; set; }
    public ICollection<UserEntity> AllowedToReadUsers { get; set; }
    public string Name { get; set; }
    public DateTime LastModifiedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}