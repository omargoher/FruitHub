using FruitHub.ApplicationCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FruitHub.Infrastructure.Persistence.Configurations;

public class UserFavoriteEntityTypeConfiguration : IEntityTypeConfiguration<UserFavorite>
{
    public void Configure(EntityTypeBuilder<UserFavorite> builder)
    {
        builder
            .HasKey(uf => new { uf.UserId, uf.ProductId });

        builder
            .HasOne(uf => uf.User)
            .WithMany(u => u.FavoriteList)
            .HasForeignKey(uf => uf.UserId)
            .IsRequired();
        
        builder
            .HasOne(uf => uf.Product)
            .WithMany()
            .HasForeignKey(uf => uf.ProductId)
            .IsRequired();
    }
}