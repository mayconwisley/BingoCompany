using BingoCompany.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BingoCompany.Infrastructure.Persistence.Configurations;

public sealed class CompanyUserConfiguration : IEntityTypeConfiguration<CompanyUser>
{
    public void Configure(EntityTypeBuilder<CompanyUser> builder)
    {
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => item.Email).IsUnique();
        builder.Property(item => item.Email).HasMaxLength(320);
        builder.Property(item => item.Name).HasMaxLength(160);
        builder.HasOne<Company>().WithMany().HasForeignKey(item => item.CompanyId);
    }
}
