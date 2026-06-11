using System.Text;
using System.Text.Json;
using HospitalManagement.BusinessLogic.DTOs.DoctorBot;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Context;
using HospitalManagement.DataAccess.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;

namespace HospitalManagement.BusinessLogic.Services;

public class DoctorBotService : IDoctorBotService
{
    private readonly IEmrService _emrService;
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<DoctorBotService> _logger;

    public DoctorBotService(IEmrService emrService, AppDbContext context, HttpClient httpClient, IConfiguration config, ILogger<DoctorBotService> logger)
    {
        _emrService = emrService;
        _context = context;
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
        
        _httpClient.BaseAddress = new Uri("https://api.groq.com/openai/v1/");
        var apiKey = _config["GROQ_API_KEY"];
        if (!string.IsNullOrEmpty(apiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        }
    }

    public async Task<DoctorBotResponseDto> QueryEmrAsync(Guid patientId, string question, CancellationToken ct = default)
    {
        var emrData = await _emrService.GetFullEmrAsync(patientId, ct);
        string emrJson = JsonSerializer.Serialize(emrData, new JsonSerializerOptions { WriteIndented = true });

        string prompt = $@"
You are a helpful medical AI assistant for doctors.
The doctor is asking a question about a patient's EMR (Electronic Medical Record).

Patient's EMR JSON Data:
{emrJson}

Doctor's Question: ""{question}""

Based ONLY on the EMR data provided above, please answer the doctor's question or provide the summary they requested. Be concise, professional, and clinically accurate. Do not mention the JSON structure, just use the information.
";

        string answer = await CallGroqAsync(prompt, ct);

        return new DoctorBotResponseDto { Answer = answer };
    }

    public async Task<DoctorBotResponseDto> QueryDocumentAsync(Guid documentId, string question, CancellationToken ct = default)
    {
        var document = await _context.Documents.FirstOrDefaultAsync(d => d.Id == documentId, ct)
            ?? throw new NotFoundException("Document", documentId);

        if (string.IsNullOrEmpty(document.FileUrl))
            return new DoctorBotResponseDto { Answer = "This document does not contain a valid file." };

        string documentText = "";

        try
        {
            if (document.FileUrl.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                documentText = await ExtractTextFromPdfAsync(document.FileUrl, ct);
            }
            else if (document.FileUrl.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            {
                documentText = await ReadTextFileAsync(document.FileUrl, ct);
            }
            else
            {
                return new DoctorBotResponseDto { Answer = "Unsupported file format. I can only read .pdf and .txt files. Images or scans are not supported." };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading document file.");
            return new DoctorBotResponseDto { Answer = $"Failed to read document: {ex.Message}" };
        }

        string prompt = $@"
You are a helpful medical AI assistant for doctors.
The doctor is asking a question about a specific uploaded document.

Document Name: {document.FileName}
Document Type: {document.DocumentType}
Document Content:
{documentText}

Doctor's Question: ""{question}""

Based ONLY on the document content provided above, please answer the doctor's question or provide the summary they requested. Be concise, professional, and clinically accurate.
";

        string answer = await CallGroqAsync(prompt, ct);

        return new DoctorBotResponseDto { Answer = answer };
    }

    private async Task<string> ExtractTextFromPdfAsync(string fileUrl, CancellationToken ct)
    {
        byte[] pdfBytes;
        if (fileUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            // Note: Use a separate general HttpClient for downloading if needed, but we can reuse the base address temporarily or just create a new one.
            using var client = new HttpClient();
            pdfBytes = await client.GetByteArrayAsync(fileUrl, ct);
        }
        else
        {
            // Assume it's a local file path
            if (!File.Exists(fileUrl)) throw new FileNotFoundException($"PDF file not found at {fileUrl}");
            pdfBytes = await File.ReadAllBytesAsync(fileUrl, ct);
        }

        using var document = PdfDocument.Open(pdfBytes);
        var sb = new StringBuilder();
        foreach (var page in document.GetPages())
        {
            sb.AppendLine(page.Text);
        }
        return sb.ToString();
    }

    private async Task<string> ReadTextFileAsync(string fileUrl, CancellationToken ct)
    {
        if (fileUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            using var client = new HttpClient();
            return await client.GetStringAsync(fileUrl, ct);
        }
        
        if (!File.Exists(fileUrl)) throw new FileNotFoundException($"Text file not found at {fileUrl}");
        return await File.ReadAllTextAsync(fileUrl, ct);
    }

    private async Task<string> CallGroqAsync(string prompt, CancellationToken ct)
    {
        var payload = new
        {
            model = "llama-3.3-70b-versatile",
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            temperature = 0.2
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("chat/completions", content, ct);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(responseString);
        var message = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return message ?? string.Empty;
    }
}
