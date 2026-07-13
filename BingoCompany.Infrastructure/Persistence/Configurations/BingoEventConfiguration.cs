using BingoCompany.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BingoCompany.Infrastructure.Persistence.Configurations;

public sealed class BingoEventConfiguration : IEntityTypeConfiguration<BingoEvent>
{
    public void Configure(EntityTypeBuilder<BingoEvent> builder)
    {
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => item.PublicCode).IsUnique();
        builder.HasIndex(item => item.CompanyId);
        builder.HasOne<Company>().WithMany().HasForeignKey(item => item.CompanyId);
        builder.HasMany(item => item.Rounds).WithOne().HasForeignKey(item => item.EventId);
        builder.HasMany(item => item.Cards).WithOne().HasForeignKey(item => item.EventId);
        builder.HasMany(item => item.Participants).WithOne().HasForeignKey(item => item.EventId);
    }
}
