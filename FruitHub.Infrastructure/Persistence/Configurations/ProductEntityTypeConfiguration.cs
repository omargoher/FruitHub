using FruitHub.ApplicationCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FruitHub.Infrastructure.Persistence.Configurations;

public class ProductEntityTypeConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .ValueGeneratedOnAdd();

        builder.Property(p => p.Name)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(p => p.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
        
        builder.Property(p => p.Calories)
            .IsRequired();
        
        builder.Property(p => p.Description)
            .HasMaxLength(500)
            .IsRequired();
        
        builder.Property(p => p.Organic)
            .HasDefaultValue(true)
            .IsRequired();
        
        builder.Property(p => p.ExpirationPeriodByDays)
            .IsRequired();
        
        builder.Property(p => p.ImageUrl)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(p => p.Stock)
            .IsRequired();

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .IsRequired();
        
        builder.HasOne(p => p.Admin)
            .WithMany(a => a.Products)
            .HasForeignKey(p => p.AdminId)
            .IsRequired();
    }
}