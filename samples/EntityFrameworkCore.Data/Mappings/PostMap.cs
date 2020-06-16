using EntityFrameworkCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntityFrameworkCore.Data.Mappings
{
    public class PostMap : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            // Primary Key
            builder.HasKey(x => x.Id);

            // Properties
            builder.Property(x => x.Id)
                   .IsRequired();

            builder.Property(x => x.BlogId)
                   .IsRequired();

            builder.Property(x => x.Title)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(x => x.Content)
                   .IsRequired()
                   .HasMaxLength(500);

            // Table & Column Mappings
            builder.ToTable("Post");
            builder.Property(x => x.Id).HasColumnName("Id");
            builder.Property(x => x.Title).HasColumnName("Title");
            builder.Property(x => x.Content).HasColumnName("Content");

            // Relationships
            builder.HasOne(x => x.Blog)
                   .WithMany(x => x.Posts)
                   .HasForeignKey(x => x.BlogId)
                   .IsRequired();
        }
    }
}
