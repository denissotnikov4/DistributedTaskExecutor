using FluentValidation;
using TaskService.Client.Models.Requests;

namespace TaskService.Logic.Validators;

public class TaskCreateRequestValidator : AbstractValidator<TaskCreateRequest>
{
    public TaskCreateRequestValidator()
    {
        this.RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Description is required.")
            .MaximumLength(500)
            .WithMessage("Description must not exceed 500 characters.");

        this.RuleFor(x => x.InputData)
            .NotEmpty()
            .WithMessage("Payload is required.");

        this.RuleFor(x => x.Ttl)
            .GreaterThan(TimeSpan.Zero)
            .WithMessage("TTL must be greater than zero.")
            .LessThanOrEqualTo(TimeSpan.FromHours(24))
            .WithMessage("TTL must not exceed 24 hours.");

        this.RuleFor(x => x.Code)
            .Must(code => !string.IsNullOrEmpty(code))
            .WithMessage("Code should be specified.");
    }
}