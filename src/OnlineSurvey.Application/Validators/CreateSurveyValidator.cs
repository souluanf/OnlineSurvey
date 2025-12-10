using FluentValidation;
using OnlineSurvey.Application.DTOs;

namespace OnlineSurvey.Application.Validators;

public class CreateSurveyValidator : AbstractValidator<CreateSurveyRequest>
{
    public CreateSurveyValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(x => x.Questions)
            .NotEmpty().WithMessage("At least one question is required.")
            .Must(q => q.Count <= 50).WithMessage("Survey cannot have more than 50 questions.");

        RuleForEach(x => x.Questions).SetValidator(new CreateQuestionValidator());
    }
}

public class CreateQuestionValidator : AbstractValidator<CreateQuestionRequest>
{
    public CreateQuestionValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Question text is required.")
            .MaximumLength(500).WithMessage("Question text cannot exceed 500 characters.");

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0).WithMessage("Order cannot be negative.");

        RuleFor(x => x.Options)
            .NotEmpty().WithMessage("At least 2 options are required.")
            .Must(o => o.Count >= 2).WithMessage("At least 2 options are required.")
            .Must(o => o.Count <= 10).WithMessage("Question cannot have more than 10 options.");

        RuleForEach(x => x.Options).SetValidator(new CreateOptionValidator());
    }
}

public class CreateOptionValidator : AbstractValidator<CreateOptionRequest>
{
    public CreateOptionValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Option text is required.")
            .MaximumLength(200).WithMessage("Option text cannot exceed 200 characters.");

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0).WithMessage("Order cannot be negative.");
    }
}
