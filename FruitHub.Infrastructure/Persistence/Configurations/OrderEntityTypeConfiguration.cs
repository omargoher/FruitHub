using FruitHub.ApplicationCore.Enums.Order;
using FruitHub.ApplicationCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FruitHub.Infrastructure.Persistence.Configurations;

public class OrderEntityTypeConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id)
            .ValueGeneratedOnAdd();

        builder.Property(o => o.CustomerFullName)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(o => o.CustomerAddress)
            .HasMaxLength(255)
            .IsRequired();
        
        builder.Property(o => o.CustomerCity)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(o => o.CustomerDepartment)
            .IsRequired();
        
        builder.Property(o => o.CustomerPhoneNumber)
            .HasMaxLength(50)
            .IsRequired();
        
        builder.Property(o => o.SubPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
        
        builder.Property(o => o.TotalPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
        
        builder.Property(o => o.ShippingFees)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
        
        builder.Property(o => o.OrderStatus)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(o => o.OrderStatus)
            .HasDefaultValue(OrderStatus.Pending);
        
        builder.HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .IsRequired();
    }
}