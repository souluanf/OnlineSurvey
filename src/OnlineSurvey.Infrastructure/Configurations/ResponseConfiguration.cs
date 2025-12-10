using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineSurvey.Domain.Entities;

namespace OnlineSurvey.Infrastructure.Configurations;

public class ResponseConfiguration : IEntityTypeConfiguration<Response>
{
    public void Configure(EntityTypeBuilder<Response> builder)
    {
        builder.ToTable("Responses");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.ParticipantId)
            .HasMaxLength(100);

        builder.Property(r => r.IpAddress)
            .HasMaxLength(45);

        builder.Property(r => r.SubmittedAt)
            .IsRequired();

        builder.HasMany(r => r.Answers)
            .WithOne()
            .HasForeignKey(a => a.ResponseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.SurveyId);
        builder.HasIndex(r => r.ParticipantId);
        builder.HasIndex(r => new { r.SurveyId, r.ParticipantId });
        builder.HasIndex(r => r.SubmittedAt);
    }
}
