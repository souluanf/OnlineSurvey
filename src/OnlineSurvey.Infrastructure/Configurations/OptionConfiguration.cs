using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineSurvey.Domain.Entities;

namespace OnlineSurvey.Infrastructure.Configurations;

public class OptionConfiguration : IEntityTypeConfiguration<Option>
{
    public void Configure(EntityTypeBuilder<Option> builder)
    {
        builder.ToTable("Options");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Text)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.Order)
            .IsRequired();

        builder.HasIndex(o => o.QuestionId);
        builder.HasIndex(o => new { o.QuestionId, o.Order });
    }
}
