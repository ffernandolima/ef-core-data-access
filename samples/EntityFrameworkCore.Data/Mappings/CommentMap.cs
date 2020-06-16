using EntityFrameworkCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntityFrameworkCore.Data.Mappings
{
    public class CommentMap : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            // Primary Key
            builder.HasKey(x => x.Id);

            // Properties
            builder.Property(x => x.Id)
                   .IsRequired();

            builder.Property(x => x.PostId)
                   .IsRequired();

            builder.Property(x => x.Title)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(x => x.Content)
                   .IsRequired()
                   .HasMaxLength(500);

            // Table & Column Mappings
            builder.ToTable("Comment");
            builder.Property(x => x.Id).HasColumnName("Id");
            builder.Property(x => x.Title).HasColumnName("Title");
            builder.Property(x => x.Content).HasColumnName("Content");

            // Relationships
            builder.HasOne(x => x.Post)
                   .WithMany(x => x.Comments)
                   .HasForeignKey(x => x.PostId)
                   .IsRequired();
        }
    }
}
