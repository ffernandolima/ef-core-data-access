using EntityFrameworkCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntityFrameworkCore.Data.Mappings
{
    public class BlogMap : IEntityTypeConfiguration<Blog>
    {
        public void Configure(EntityTypeBuilder<Blog> builder)
        {
            // Primary Key
            builder.HasKey(x => x.Id);

            // Properties
            builder.Property(x => x.Id)
                   .IsRequired();

            builder.Property(x => x.Url)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(x => x.Title)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(x => x.TypeId)
                   .IsRequired();

            // Table & Column Mappings
            builder.ToTable("Blog");
            builder.Property(x => x.Id).HasColumnName("Id");
            builder.Property(x => x.Url).HasColumnName("Url");
            builder.Property(x => x.Title).HasColumnName("Title");
            builder.Property(x => x.TypeId).HasColumnName("TypeId");

            // Relationships
            builder.HasOne(x => x.Type)
                   .WithMany()
                   .HasForeignKey(x => x.TypeId);
        }
    }
}
