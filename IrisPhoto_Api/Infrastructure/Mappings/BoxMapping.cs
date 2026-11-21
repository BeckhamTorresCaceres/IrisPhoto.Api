using IrisPhoto_Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IrisPhoto_Api.Infrastructure.Mappings
{
    public class BoxMapping : IEntityTypeConfiguration<Box>
    {
        public void Configure(EntityTypeBuilder<Box> builder)
        {
            builder.ToTable("boxes");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("id");

            builder.Property(e => e.UserId)
                  .HasColumnName("user_id")
                  .IsRequired();

            builder.Property(e => e.Name)
                  .HasColumnName("name");

            builder.Property(e => e.PasswordType)
                .HasColumnName("password_type")
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(e => e.Password)
                  .HasColumnName("password")
                  .IsRequired();
            
            builder.Property(e => e.IsActive)
                  .HasColumnName("is_active")
                  .HasDefaultValue(true);

        }
    }
}
