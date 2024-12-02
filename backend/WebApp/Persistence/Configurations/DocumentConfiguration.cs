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

       builder.HasMany(d => d.AllowedToEditUsers)
           .WithMany(u => u.AllowedToEditDocuments);
       
       builder.HasMany(d => d.AllowedToReadUsers)
           .WithMany(u => u.AllowedToReadDocuments);
    }
}