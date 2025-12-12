using FluentValidation;
using TimesheetApi.DTOs;

namespace TimesheetApi.Validators;

public class CreateOrUpdateTimesheetRequestValidator : AbstractValidator<CreateOrUpdateTimesheetRequest>
{
    public CreateOrUpdateTimesheetRequestValidator()
    {
        RuleFor(x => x.WeekStartDate)
            .NotEmpty().WithMessage("Week start date is required")
            .Must(BeMonday).WithMessage("Week start date must be a Monday");

        RuleFor(x => x.Entries)
            .NotNull().WithMessage("Entries collection is required");

        RuleForEach(x => x.Entries)
            .SetValidator(new TimesheetEntryDtoValidator());

        RuleFor(x => x.Entries)
            .Must(entries => entries.Sum(e => e.Hours) <= 100)
            .WithMessage("Total weekly hours must not exceed 100")
            .When(x => x.Entries != null && x.Entries.Any());

        RuleFor(x => x)
            .Must(request => AllEntriesWithinWeek(request))
            .WithMessage("All entry dates must be within the specified week (Monday to Sunday)")
            .When(x => x.Entries != null && x.Entries.Any());
    }

    private bool BeMonday(DateTime date)
    {
        return date.DayOfWeek == DayOfWeek.Monday;
    }

    private bool AllEntriesWithinWeek(CreateOrUpdateTimesheetRequest request)
    {
        var weekStart = request.WeekStartDate.Date;
        var weekEnd = weekStart.AddDays(6);

        return request.Entries.All(e => e.Date.Date >= weekStart && e.Date.Date <= weekEnd);
    }
}
