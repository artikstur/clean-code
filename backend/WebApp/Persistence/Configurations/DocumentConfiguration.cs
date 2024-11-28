using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Entities;

namespace Persistence.Configurations;

public class DocumentConfiguration: IEntityTypeConfiguration<DocumentEntity>
{
    public void Configure(EntityTypeBuilder<DocumentEntity> builder)
    {
        builder.HasKey(d => d.DocumentId);

       builder.HasOne(d => d.Author) 
           .WithMany(u => u.PersonalDocuments) 
           .HasForeignKey(d => d.AuthorId) 
           .OnDelete(DeleteBehavior.Cascade);

       builder.HasMany(d => d.AllowedUsers)
           .WithMany(u => u.AllowedDocuments);
    }
}