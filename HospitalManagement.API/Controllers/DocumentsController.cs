using HospitalManagement.BusinessLogic.DTOs.Patient;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Presentation.Controllers;

[ApiController]
[Route("api/documents")]
[Authorize]
[Produces("application/json")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(DocumentResponseDto), 201)]
    public async Task<IActionResult> CreateDocument([FromBody] CreateDocumentRequestDto request, CancellationToken ct)
    {
        var result = await _documentService.CreateDocumentAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, new { success = true, data = result });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DocumentResponseDto), 200)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _documentService.GetDocumentByIdAsync(id, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("patient/{patientId:guid}")]
    [ProducesResponseType(typeof(List<DocumentResponseDto>), 200)]
    public async Task<IActionResult> GetByPatientId(Guid patientId, CancellationToken ct)
    {
        var result = await _documentService.GetDocumentsByPatientIdAsync(patientId, ct);
        return Ok(new { success = true, data = result });
    }
}
