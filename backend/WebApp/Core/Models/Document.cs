namespace Core.Models;

public class Document
{
    public Guid DocumentId { get; set; }
    public Guid AuthorId { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastModified { get; set; }

    private Document(Guid documentId, Guid authorId, string name, DateTime createdAt, DateTime lastModified)
    {
        DocumentId = documentId;
        AuthorId = authorId;
        Name = name;
        CreatedAt = createdAt;
        LastModified = lastModified;
    }

    public static Document Create(Guid documentId, Guid authorId, string name, DateTime createdAt, DateTime lastModified)
    {
        return new Document(documentId, authorId, name, createdAt, lastModified);
    }
}