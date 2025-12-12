using FluentValidation;
using TimesheetApi.DTOs;

namespace TimesheetApi.Validators;

public class TimesheetEntryDtoValidator : AbstractValidator<TimesheetEntryDto>
{
    public TimesheetEntryDtoValidator()
    {
        RuleFor(x => x.ProjectCode)
            .NotEmpty().WithMessage("Project code is required")
            .MaximumLength(100).WithMessage("Project code must not exceed 100 characters");

        RuleFor(x => x.TaskDescription)
            .MaximumLength(500).WithMessage("Task description must not exceed 500 characters");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required");

        RuleFor(x => x.Hours)
            .GreaterThanOrEqualTo(0).WithMessage("Hours must be greater than or equal to 0")
            .LessThanOrEqualTo(24).WithMessage("Hours must be less than or equal to 24 per entry");
    }
}
