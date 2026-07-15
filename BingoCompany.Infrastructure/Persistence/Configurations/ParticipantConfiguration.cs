using BingoCompany.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BingoCompany.Infrastructure.Persistence.Configurations;

public sealed class ParticipantConfiguration : IEntityTypeConfiguration<Participant>
{
	public void Configure(EntityTypeBuilder<Participant> builder)
	{
		builder.HasKey(item => item.Id);
		builder.HasIndex(item => item.EventId);
		builder.HasIndex(item => item.ParticipantAccountId);
		builder.HasOne<ParticipantAccount>().WithMany().HasForeignKey(item => item.ParticipantAccountId).OnDelete(DeleteBehavior.SetNull);
	}
}
