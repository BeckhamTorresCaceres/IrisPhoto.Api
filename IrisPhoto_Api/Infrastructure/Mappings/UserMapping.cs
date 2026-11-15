using IrisPhoto_Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IrisPhoto_Api.Infrastructure.Mappings
{
    public class UserMapping : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Nombre exacto de la tabla en Postgres
            builder.ToTable("users");

            // Mapeo explícito de columnas en minúsculas para evitar conflictos de mayúsculas
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("id");

            builder.Property(e => e.Email)
                  .HasColumnName("email")
                  .IsRequired()
                  .HasMaxLength(255);

            builder.HasIndex(e => e.Email)
                  .IsUnique();

            builder.Property(e => e.PasswordHash)
                  .HasColumnName("password_hash")
                  .IsRequired();

            builder.Property(e => e.IsActive)
                  .HasColumnName("is_active")
                  .HasDefaultValue(true);

            builder.Property(e => e.CreatedAt)
                  .HasColumnName("created_at")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}
