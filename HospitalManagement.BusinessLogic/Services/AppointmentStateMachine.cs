using HospitalManagement.DataAccess.Exceptions;
using HospitalManagement.DataAccess.Models.Enums;

namespace HospitalManagement.BusinessLogic.Services;

/// <summary>
/// Domain service enforcing strict state machine transitions for Appointments.
/// </summary>
public static class AppointmentStateMachine
{
    public static void ValidateTransition(AppointmentStatus currentState, AppointmentStatus newState)
    {
        if (currentState == newState)
            return;

        bool isValid = (currentState, newState) switch
        {
            // From Scheduled (Booked)
            (AppointmentStatus.Scheduled, AppointmentStatus.Confirmed) => true,
            (AppointmentStatus.Scheduled, AppointmentStatus.Cancelled) => true,
            (AppointmentStatus.Scheduled, AppointmentStatus.CheckedIn) => true,

            // From Confirmed
            (AppointmentStatus.Confirmed, AppointmentStatus.CheckedIn) => true,
            (AppointmentStatus.Confirmed, AppointmentStatus.Cancelled) => true,
            (AppointmentStatus.Confirmed, AppointmentStatus.NoShow) => true,

            // From CheckedIn
            (AppointmentStatus.CheckedIn, AppointmentStatus.InProgress) => true,
            (AppointmentStatus.CheckedIn, AppointmentStatus.Cancelled) => true,

            // From InProgress
            (AppointmentStatus.InProgress, AppointmentStatus.Completed) => true,
            // (AppointmentStatus.InProgress, AppointmentStatus.PendingPayment) => true, // If we use it during completion

            // From PendingPayment
            (AppointmentStatus.PendingPayment, AppointmentStatus.Completed) => true,

            _ => false
        };

        if (!isValid)
        {
            throw new BusinessRuleViolationException(
                "InvalidAppointmentTransition", 
                $"Cannot transition appointment from '{currentState}' to '{newState}'.");
        }
    }
}
