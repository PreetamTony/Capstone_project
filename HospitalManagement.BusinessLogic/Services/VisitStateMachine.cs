using HospitalManagement.DataAccess.Exceptions;
using HospitalManagement.DataAccess.Models.Enums;

namespace HospitalManagement.BusinessLogic.Services;

public static class VisitStateMachine
{
    public static void ValidateTransition(VisitStatus current, VisitStatus next)
    {
        if (current == next) return; // Idempotent updates are fine

        bool isValid = (current, next) switch
        {
            // Allowed Transitions
            (VisitStatus.CheckedIn, VisitStatus.InConsultation) => true,
            (VisitStatus.InConsultation, VisitStatus.Completed) => true,
            (VisitStatus.CheckedIn, VisitStatus.Cancelled) => true,

            // All others invalid
            _ => false
        };

        if (!isValid)
            throw new BusinessRuleViolationException("InvalidStateTransition", 
                $"Cannot transition visit from '{current}' to '{next}'.");
    }
}
