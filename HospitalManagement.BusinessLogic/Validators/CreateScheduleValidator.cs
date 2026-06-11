using FluentValidation;
using HospitalManagement.BusinessLogic.DTOs.Schedule;

namespace HospitalManagement.BusinessLogic.Validators;

public class CreateScheduleValidator : AbstractValidator<CreateScheduleRequestDto>
{
    public CreateScheduleValidator()
    {
        RuleForEach(x => x.DaysOfWeek)
            .InclusiveBetween(0, 6).WithMessage("Day of week must be between 0 (Sunday) and 6 (Saturday).");

        RuleFor(x => x.StartTime)
            .LessThan(x => x.EndTime).WithMessage("Start time must be before end time.");

        RuleFor(x => x.ValidFrom)
            .LessThan(x => x.ValidTo).WithMessage("Valid from date must be before valid to date.");
    }
}
