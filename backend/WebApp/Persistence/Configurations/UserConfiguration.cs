using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Entities;

namespace Persistence.Configurations;

public class UserConfiguration: IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.HasKey(u => u.Id);

        builder
            .HasMany(u => u.Roles)
            .WithMany(r => r.Users)
            .UsingEntity<UserRoleEntity>(
                l => l.HasOne<RoleEntity>()
                    .WithMany()
                    .HasForeignKey(r => r.RoleId),
                r => r.HasOne<UserEntity>()
                    .WithMany()
                    .HasForeignKey(u => u.UserId));

        builder.HasMany(u => u.PersonalDocuments)
            .WithOne(d => d.Author) 
            .HasForeignKey(d => d.AuthorId)
            .OnDelete(DeleteBehavior.Cascade); 

        builder.HasMany(u => u.AllowedToEditDocuments)
            .WithMany(d => d.AllowedToEditUsers);
        
        builder.HasMany(u => u.AllowedToReadDocuments)
            .WithMany(d => d.AllowedToReadUsers);
    }
}