using HospitalManagement.BusinessLogic.DTOs.Emr;
using HospitalManagement.DataAccess.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HospitalManagement.DataAccess.Repositories;
using HospitalManagement.DataAccess.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Presentation.Controllers;

public partial class EmrController
{
    private async Task<Guid> GetPatientIdFromCurrentUserAsync([FromServices] IUnitOfWork uow)
    {
        var userIdString = User.FindFirst(AppConstants.Jwt.ClaimUserId)?.Value 
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
        if (!Guid.TryParse(userIdString, out var userId))
            throw new System.UnauthorizedAccessException("User ID not found in token.");

        var patient = await uow.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null)
            throw new NotFoundException("Patient profile not found for the current user.");

        return patient.Id;
    }

    private async Task EnsurePatientOwnershipAsync(Guid requestedPatientId, IUnitOfWork uow)
    {
        if (User.IsInRole(AppConstants.Roles.Patient))
        {
            var myPatientId = await GetPatientIdFromCurrentUserAsync(uow);
            if (myPatientId != requestedPatientId)
            {
                throw new HospitalManagement.DataAccess.Exceptions.BusinessRuleViolationException("Forbidden", "You are not authorized to view this patient's records.");
            }
        }
    }

    [HttpGet("me")]
    [Authorize(Roles = AppConstants.Roles.Patient)]
    public async Task<ActionResult<FullEmrResponseDto>> GetMyEmr([FromServices] IUnitOfWork uow, CancellationToken ct)
    {
        var patientId = await GetPatientIdFromCurrentUserAsync(uow);
        var result = await _emrService.GetFullEmrAsync(patientId, ct);
        return Ok(result);
    }

    // -- Summaries & Timelines --

    [HttpGet("patient/{patientId:guid}/summary")]
    [Authorize(Roles = $"{AppConstants.Roles.Doctor},{AppConstants.Roles.Patient}")]
    public async Task<ActionResult<EmrSummaryDto>> GetEmrSummary(Guid patientId, [FromServices] IUnitOfWork uow, CancellationToken ct)
    {
        await EnsurePatientOwnershipAsync(patientId, uow);
        var result = await _emrService.GetEmrSummaryAsync(patientId, ct);
        return Ok(result);
    }

    [HttpGet("patient/{patientId:guid}/timeline")]
    [Authorize(Roles = $"{AppConstants.Roles.Doctor},{AppConstants.Roles.Patient}")]
    public async Task<ActionResult<IEnumerable<HospitalManagement.BusinessLogic.DTOs.Patient.PatientTimelineItemDto>>> GetClinicalTimeline(Guid patientId, [FromServices] IUnitOfWork uow, CancellationToken ct)
    {
        await EnsurePatientOwnershipAsync(patientId, uow);
        var result = await _emrService.GetClinicalTimelineAsync(patientId, ct);
        return Ok(result);
    }

    [HttpGet("patient/{patientId:guid}/vitals/latest")]
    [Authorize(Roles = $"{AppConstants.Roles.Doctor},{AppConstants.Roles.Patient}")]
    public async Task<ActionResult<VitalsDto>> GetLatestVitals(Guid patientId, [FromServices] IUnitOfWork uow, CancellationToken ct)
    {
        await EnsurePatientOwnershipAsync(patientId, uow);
        var result = await _emrService.GetLatestVitalsAsync(patientId, ct);
        if (result == null) return NotFound("No vitals recorded for this patient.");
        return Ok(result);
    }

    [HttpGet("patient/{patientId:guid}/diagnoses")]
    [Authorize(Roles = $"{AppConstants.Roles.Doctor},{AppConstants.Roles.Patient}")]
    public async Task<ActionResult<IEnumerable<DiagnosisHistoryDto>>> GetDiagnosisHistory(Guid patientId, [FromServices] IUnitOfWork uow, CancellationToken ct)
    {
        await EnsurePatientOwnershipAsync(patientId, uow);
        var result = await _emrService.GetDiagnosisHistoryAsync(patientId, ct);
        return Ok(result);
    }

    // -- Emergency Info --

    [HttpGet("patient/{patientId:guid}/emergency-info")]
    [Authorize(Roles = $"{AppConstants.Roles.Doctor},{AppConstants.Roles.Receptionist},{AppConstants.Roles.Patient}")]
    public async Task<ActionResult<EmergencyInfoDto>> GetEmergencyInfo(Guid patientId, [FromServices] IUnitOfWork uow, CancellationToken ct)
    {
        await EnsurePatientOwnershipAsync(patientId, uow);
        var result = await _emrService.GetEmergencyInfoAsync(patientId, ct);
        return Ok(result);
    }

    [HttpPut("patient/{patientId:guid}/emergency-info")]
    [Authorize(Roles = $"{AppConstants.Roles.Doctor},{AppConstants.Roles.Receptionist},{AppConstants.Roles.Patient}")]
    public async Task<ActionResult<EmergencyInfoDto>> UpdateEmergencyInfo(Guid patientId, [FromBody] UpdateEmergencyInfoRequestDto request, [FromServices] IUnitOfWork uow, CancellationToken ct)
    {
        await EnsurePatientOwnershipAsync(patientId, uow);
        var result = await _emrService.UpdateEmergencyInfoAsync(patientId, request, ct);
        return Ok(result);
    }

    // -- Immunizations --

    [HttpGet("patient/{patientId:guid}/immunizations")]
    [Authorize(Roles = $"{AppConstants.Roles.Doctor},{AppConstants.Roles.Patient}")]
    public async Task<ActionResult<IEnumerable<ImmunizationDto>>> GetImmunizations(Guid patientId, [FromServices] IUnitOfWork uow, CancellationToken ct)
    {
        await EnsurePatientOwnershipAsync(patientId, uow);
        var result = await _emrService.GetImmunizationsAsync(patientId, ct);
        return Ok(result);
    }

    [HttpPost("patient/{patientId:guid}/immunizations")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    public async Task<ActionResult<ImmunizationDto>> AddImmunization(Guid patientId, [FromBody] CreateImmunizationRequestDto request, CancellationToken ct)
    {
        var result = await _emrService.AddImmunizationAsync(patientId, request, ct);
        return CreatedAtAction(nameof(GetImmunizations), new { patientId }, result);
    }

    [HttpPut("patient/{patientId:guid}/immunizations/{id:guid}")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    public async Task<IActionResult> UpdateImmunization(Guid patientId, Guid id, [FromBody] UpdateImmunizationRequestDto request, CancellationToken ct)
    {
        await _emrService.UpdateImmunizationAsync(patientId, id, request, ct);
        return NoContent();
    }

    [HttpDelete("patient/{patientId:guid}/immunizations/{id:guid}")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    public async Task<IActionResult> DeleteImmunization(Guid patientId, Guid id, CancellationToken ct)
    {
        await _emrService.DeleteImmunizationAsync(patientId, id, ct);
        return NoContent();
    }

    // -- Allergies & History Updates/Deletes --

    [HttpPut("patient/{patientId:guid}/allergies/{id:guid}")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    public async Task<IActionResult> UpdateAllergy(Guid patientId, Guid id, [FromBody] UpdateAllergyRequestDto request, CancellationToken ct)
    {
        await _emrService.UpdateAllergyAsync(patientId, id, request, ct);
        return NoContent();
    }

    [HttpDelete("patient/{patientId:guid}/allergies/{id:guid}")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    public async Task<IActionResult> DeleteAllergy(Guid patientId, Guid id, CancellationToken ct)
    {
        await _emrService.DeleteAllergyAsync(patientId, id, ct);
        return NoContent();
    }

    [HttpPut("patient/{patientId:guid}/history/{id:guid}")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    public async Task<IActionResult> UpdateMedicalHistory(Guid patientId, Guid id, [FromBody] UpdateMedicalHistoryRequestDto request, CancellationToken ct)
    {
        await _emrService.UpdateMedicalHistoryAsync(patientId, id, request, ct);
        return NoContent();
    }

    [HttpDelete("patient/{patientId:guid}/history/{id:guid}")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    public async Task<IActionResult> DeleteMedicalHistory(Guid patientId, Guid id, CancellationToken ct)
    {
        await _emrService.DeleteMedicalHistoryAsync(patientId, id, ct);
        return NoContent();
    }

    // -- Documents --

    [HttpGet("patient/{patientId:guid}/documents")]
    [Authorize(Roles = $"{AppConstants.Roles.Doctor},{AppConstants.Roles.Patient}")]
    public async Task<ActionResult<IEnumerable<EmrDocumentDto>>> GetDocuments(Guid patientId, [FromServices] IUnitOfWork uow, CancellationToken ct)
    {
        await EnsurePatientOwnershipAsync(patientId, uow);
        var result = await _emrService.GetDocumentsAsync(patientId, ct);
        return Ok(result);
    }

    [HttpPost("patient/{patientId:guid}/documents")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    public async Task<ActionResult<EmrDocumentDto>> UploadDocument(Guid patientId, [FromForm] UploadEmrDocumentRequestDto request, CancellationToken ct)
    {
        var userIdString = User.FindFirst(AppConstants.Jwt.ClaimUserId)?.Value 
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Guid.TryParse(userIdString, out var uploaderId);

        var result = await _emrService.UploadDocumentAsync(patientId, request, uploaderId, ct);
        return CreatedAtAction(nameof(GetDocuments), new { patientId }, result);
    }

    [HttpDelete("patient/{patientId:guid}/documents/{id:guid}")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    public async Task<IActionResult> DeleteDocument(Guid patientId, Guid id, CancellationToken ct)
    {
        await _emrService.DeleteDocumentAsync(patientId, id, ct);
        return NoContent();
    }

    // -- Audit --

    [HttpGet("patient/{patientId:guid}/audit")]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    public async Task<ActionResult<IEnumerable<EmrAuditDto>>> GetAuditLog(Guid patientId, CancellationToken ct)
    {
        var result = await _emrService.GetAuditLogAsync(patientId, ct);
        return Ok(result);
    }
}
