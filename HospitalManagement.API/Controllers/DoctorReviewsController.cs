using HospitalManagement.BusinessLogic.DTOs.Doctor;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Presentation.Controllers;

[ApiController]
[Route("api/doctors")]
[Authorize]
[Produces("application/json")]
public class DoctorReviewsController : ControllerBase
{
    private readonly IDoctorReviewService _reviewService;

    public DoctorReviewsController(IDoctorReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpPost("{doctorId:guid}/reviews")]
    [ProducesResponseType(typeof(DoctorReviewResponseDto), 201)]
    public async Task<IActionResult> CreateReview(Guid doctorId, [FromBody] CreateDoctorReviewRequestDto request, CancellationToken ct)
    {
        if (doctorId != request.DoctorId)
            return BadRequest(new { success = false, message = "Doctor ID mismatch." });

        var result = await _reviewService.CreateReviewAsync(request, ct);
        return CreatedAtAction(nameof(GetReviewsByDoctor), new { doctorId = doctorId }, new { success = true, data = result });
    }

    [HttpGet("{doctorId:guid}/reviews")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<DoctorReviewResponseDto>), 200)]
    public async Task<IActionResult> GetReviewsByDoctor(Guid doctorId, CancellationToken ct)
    {
        var result = await _reviewService.GetReviewsByDoctorIdAsync(doctorId, ct);
        return Ok(new { success = true, data = result });
    }
}
