using FluentValidation;
using TimesheetApi.DTOs;

namespace TimesheetApi.Validators;

public class RejectTimesheetRequestValidator : AbstractValidator<RejectTimesheetRequest>
{
    public RejectTimesheetRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Rejection reason is required")
            .MinimumLength(5).WithMessage("Rejection reason must be at least 5 characters");
    }
}
