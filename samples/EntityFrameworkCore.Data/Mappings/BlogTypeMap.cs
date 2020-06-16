using EntityFrameworkCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntityFrameworkCore.Data.Mappings
{
    public class BlogTypeMap : IEntityTypeConfiguration<BlogType>
    {
        public void Configure(EntityTypeBuilder<BlogType> builder)
        {
            // Primary Key
            builder.HasKey(x => x.Id);

            // Properties
            builder.Property(x => x.Id)
                   .IsRequired();

            builder.Property(x => x.Description)
                   .IsRequired()
                   .HasMaxLength(500);

            // Table & Column Mappings
            builder.ToTable("BlogType");
            builder.Property(x => x.Id).HasColumnName("Id");
            builder.Property(x => x.Description).HasColumnName("Description");

            // Relationships
        }
    }
}
