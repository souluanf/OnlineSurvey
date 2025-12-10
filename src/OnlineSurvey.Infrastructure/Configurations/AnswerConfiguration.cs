using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineSurvey.Domain.Entities;

namespace OnlineSurvey.Infrastructure.Configurations;

public class AnswerConfiguration : IEntityTypeConfiguration<Answer>
{
    public void Configure(EntityTypeBuilder<Answer> builder)
    {
        builder.ToTable("Answers");

        builder.HasKey(a => a.Id);

        builder.HasIndex(a => a.ResponseId);
        builder.HasIndex(a => a.QuestionId);
        builder.HasIndex(a => a.SelectedOptionId);
        builder.HasIndex(a => new { a.ResponseId, a.QuestionId }).IsUnique();
    }
}
