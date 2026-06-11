using HospitalManagement.DataAccess.Exceptions;
using HospitalManagement.DataAccess.Models.Enums;

namespace HospitalManagement.BusinessLogic.Services;

public static class ConsultationStateMachine
{
    private static readonly Dictionary<ConsultationStatus, HashSet<ConsultationStatus>> ValidTransitions = new()
    {
        { ConsultationStatus.Draft, new HashSet<ConsultationStatus> { ConsultationStatus.InProgress, ConsultationStatus.Cancelled } },
        { ConsultationStatus.InProgress, new HashSet<ConsultationStatus> { ConsultationStatus.Completed, ConsultationStatus.Cancelled } },
        { ConsultationStatus.Completed, new HashSet<ConsultationStatus>() }, // Terminal state
        { ConsultationStatus.Cancelled, new HashSet<ConsultationStatus>() }  // Terminal state
    };

    public static void ValidateTransition(ConsultationStatus current, ConsultationStatus next)
    {
        if (!ValidTransitions.ContainsKey(current))
            throw new BusinessRuleViolationException("InvalidState", $"Unknown current state: {current}");

        if (!ValidTransitions[current].Contains(next))
            throw new BusinessRuleViolationException("InvalidTransition", $"Cannot transition from {current} to {next}");
    }
}
