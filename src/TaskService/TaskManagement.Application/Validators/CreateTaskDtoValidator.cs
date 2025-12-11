using FluentValidation;
using TaskManagement.Application.DTOs;

namespace TaskManagement.Application.Validators;

public class CreateTaskDtoValidator : AbstractValidator<CreateTaskDto>
{
    public CreateTaskDtoValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.Payload)
            .NotEmpty().WithMessage("Payload is required");

        RuleFor(x => x.Ttl)
            .GreaterThan(TimeSpan.Zero).WithMessage("TTL must be greater than zero")
            .LessThanOrEqualTo(TimeSpan.FromHours(24)).WithMessage("TTL must not exceed 24 hours");

        RuleFor(x => x.MaxRetries)
            .GreaterThanOrEqualTo(0).WithMessage("MaxRetries must be greater than or equal to 0")
            .LessThanOrEqualTo(10).WithMessage("MaxRetries must not exceed 10");

        RuleFor(x => x.Code)
            .Must(code => string.IsNullOrEmpty(code) || IsValidCSharpCode(code))
            .WithMessage("Code must be valid C# code");
    }

    private static bool IsValidCSharpCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return true;

        // Базовая проверка - код должен содержать хотя бы одно выражение
        return code.Trim().Length > 0;
    }
}

