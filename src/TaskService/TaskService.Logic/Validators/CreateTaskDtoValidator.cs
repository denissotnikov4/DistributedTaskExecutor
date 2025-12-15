using FluentValidation;
using TaskService.Client.Models.Requests;

namespace TaskService.Logic.Validators;

public class CreateTaskDtoValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskDtoValidator()
    {
        this.RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

        this.RuleFor(x => x.Payload)
            .NotEmpty().WithMessage("Payload is required");

        this.RuleFor(x => x.Ttl)
            .GreaterThan(TimeSpan.Zero).WithMessage("TTL must be greater than zero")
            .LessThanOrEqualTo(TimeSpan.FromHours(24)).WithMessage("TTL must not exceed 24 hours");

        this.RuleFor(x => x.MaxRetries)
            .GreaterThanOrEqualTo(0).WithMessage("MaxRetries must be greater than or equal to 0")
            .LessThanOrEqualTo(10).WithMessage("MaxRetries must not exceed 10");

        this.RuleFor(x => x.Code)
            .Must(code => string.IsNullOrEmpty(code) || IsValidCSharpCode(code))
            .WithMessage("Code must be valid C# code");
    }

    private static bool IsValidCSharpCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return true;
        }

        return code.Trim().Length > 0;
    }
}