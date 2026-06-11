using System;
using System.Collections.Generic;
using HospitalManagement.DataAccess.Models.Enums;

namespace HospitalManagement.BusinessLogic.DTOs.Appointment;

public class AvailableSlotDto
{
    public Guid DoctorId { get; set; }
    public int SlotDurationMinutes { get; set; }
    public List<DateTime> AvailableSlots { get; set; } = new();
}

public class AppointmentSummaryDto
{
    public Guid Id { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string DoctorSpecialization { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public DateTime AppointmentTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
}

public class AppointmentDetailsDto : AppointmentSummaryDto
{
    public string Reason { get; set; } = string.Empty;
    public string? SymptomsJson { get; set; }
    public string? Notes { get; set; }
    public int? QueueNumber { get; set; }
    public string? ConsultationRoom { get; set; }
    public bool IsTeleConsultation { get; set; }
    public string? MeetingUrl { get; set; }
    public string? MeetingProvider { get; set; }
    
    public DateTime? CheckedInAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CancellationReason { get; set; }
}

public class AppointmentFilterDto
{
    public AppointmentStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public AppointmentType? Type { get; set; }
    public AppointmentPriority? Priority { get; set; }
    public Guid? DoctorId { get; set; }
    public Guid? PatientId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Sorting { get; set; } // e.g., "date_desc", "date_asc"
}
