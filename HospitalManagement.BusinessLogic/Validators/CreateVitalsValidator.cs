using FluentValidation;
using HospitalManagement.BusinessLogic.DTOs.Emr;

namespace HospitalManagement.BusinessLogic.Validators;

public class CreateVitalsValidator : AbstractValidator<CreateVitalsRequestDto>
{
    public CreateVitalsValidator()
    {
        RuleFor(x => x.HeartRate)
            .InclusiveBetween(30, 300).When(x => x.HeartRate.HasValue)
            .WithMessage("Heart rate must be between 30 and 300 bpm.");

        RuleFor(x => x.BloodPressure)
            .Matches(@"^\d{2,3}\/\d{2,3}$").When(x => !string.IsNullOrEmpty(x.BloodPressure))
            .WithMessage("Blood pressure must be in format 'Systolic/Diastolic' (e.g., 120/80).");

        RuleFor(x => x.Temperature)
            .InclusiveBetween(90.0m, 110.0m).When(x => x.Temperature.HasValue)
            .WithMessage("Temperature must be between 90.0 and 110.0 Fahrenheit.");

        RuleFor(x => x.RespiratoryRate)
            .InclusiveBetween(5, 60).When(x => x.RespiratoryRate.HasValue)
            .WithMessage("Respiratory rate must be between 5 and 60 breaths per minute.");

        RuleFor(x => x.O2Saturation)
            .InclusiveBetween(0.0m, 100.0m).When(x => x.O2Saturation.HasValue)
            .WithMessage("O2 Saturation must be between 0 and 100%.");

        RuleFor(x => x.Height)
            .GreaterThan(0).When(x => x.Height.HasValue)
            .WithMessage("Height must be greater than 0.");

        RuleFor(x => x.Weight)
            .GreaterThan(0).When(x => x.Weight.HasValue)
            .WithMessage("Weight must be greater than 0.");
    }
}
