using System.Data;
using System.Text;
using System.Text.Json;
using HospitalManagement.BusinessLogic.DTOs.AdminBot;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HospitalManagement.BusinessLogic.Services;

public class AdminBotService : IAdminBotService
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<AdminBotService> _logger;

    public AdminBotService(AppDbContext context, HttpClient httpClient, IConfiguration config, ILogger<AdminBotService> logger)
    {
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

    public async Task<AdminBotResponseDto> QueryDatabaseAsync(string question, CancellationToken ct = default)
    {
        // 1. Database Schema context
        string schema = @"
Table Appointments (
    Id UUID PRIMARY KEY,
    PatientId UUID,
    DoctorId UUID,
    AppointmentTime TIMESTAMP,
    Status VARCHAR ('Scheduled', 'Confirmed', 'InProgress', 'Completed', 'Cancelled', 'NoShow', 'CheckedIn')
);
Table Patients (
    Id UUID PRIMARY KEY,
    FirstName VARCHAR,
    LastName VARCHAR,
    Gender VARCHAR ('Male', 'Female', 'Other')
);
Table Doctors (
    Id UUID PRIMARY KEY,
    FirstName VARCHAR,
    LastName VARCHAR,
    DepartmentId UUID,
    ConsultationFee DECIMAL
);
Table Departments (
    Id UUID PRIMARY KEY,
    Name VARCHAR
);
";

        // 2. Generate SQL via Groq
        string sqlPrompt = $@"
You are an elite Senior PostgreSQL Database Administrator with decades of enterprise experience. Your role is to write highly optimized, robust, and accurate SQL queries to answer the user's questions based on the provided schema.

Given the following database schema:
{schema}

Write a PostgreSQL query to answer this user's question: ""{question}""

CRITICAL RULES:
1. If the question is conversational, general, or does NOT require database information (e.g., ""hello"", ""what is your name"", ""how are you""), you MUST return exactly the string ""NO_SQL"".
2. Otherwise, ONLY return the raw SQL query. Do not wrap it in markdown block quotes (```sql) or provide any conversational text.
3. The query MUST begin with SELECT. Under no circumstances are destructive operations (UPDATE, DELETE, DROP, INSERT, ALTER) allowed.
4. Be as exact and efficient as possible. Use joins properly when spanning multiple tables.
5. IMPORTANT: PostgreSQL is case-sensitive with our schema. You MUST wrap ALL table names and column names in double quotes. Example: SELECT ""Id"", ""FirstName"" FROM ""Doctors"";
6. POSTGRESQL SPECIFIC RULES & EXAMPLES:
   - Multiple Queries: If the user asks for multiple distinct things (e.g. ""how many"" AND ""a list""), you MAY return multiple queries separated by semicolons.
     * Example: SELECT COUNT(""Id"") FROM ""Departments""; SELECT ""Id"", ""Name"" FROM ""Departments"";
   - Date/Time: Use CURRENT_DATE or NOW() instead of GETDATE() or SYSDATE.
   - String Concatenation: Use || instead of +. Example: ""FirstName"" || ' ' || ""LastName""
   - String Matching: Use ILIKE for case-insensitive searches instead of LIKE. Example: ""Name"" ILIKE '%cardio%'
   - Limiting Results: Use LIMIT instead of TOP. Example: SELECT * FROM ""Appointments"" LIMIT 5;
7. Handle potential nulls or complex queries gracefully, leveraging your senior DBA expertise.
";

        string sqlQuery = await CallGroqAsync(sqlPrompt, ct);
        sqlQuery = sqlQuery.Replace("```sql", "").Replace("```", "").Trim();

        string queryResults = string.Empty;

        if (sqlQuery == "NO_SQL")
        {
            queryResults = "No database query was needed for this conversational question.";
        }
        else if (!sqlQuery.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
        {
            return new AdminBotResponseDto
            {
                Answer = "Security violation: I can only execute SELECT queries.",
                ExecutedSql = sqlQuery
            };
        }
        else
        {
            // 3. Execute SQL safely
            try
            {
                using var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText = sqlQuery;
                if (command.Connection?.State != System.Data.ConnectionState.Open)
                {
                    await command.Connection!.OpenAsync(ct);
                }

                using var reader = await command.ExecuteReaderAsync(ct);
                
                var allResultSets = new List<List<Dictionary<string, object>>>();
                
                do
                {
                    var dataList = new List<Dictionary<string, object>>();
                    while (await reader.ReadAsync(ct))
                    {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row[reader.GetName(i)] = reader.GetValue(i);
                        }
                        dataList.Add(row);
                    }
                    allResultSets.Add(dataList);
                } while (await reader.NextResultAsync(ct));

                if (allResultSets.Count == 1)
                {
                    queryResults = JsonSerializer.Serialize(allResultSets[0]);
                }
                else
                {
                    queryResults = JsonSerializer.Serialize(allResultSets);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing LLM generated SQL: {Sql}", sqlQuery);
                queryResults = $"SQL Execution Error: {ex.Message}";
            }
        }

        // 4. Formulate final response
        string answerPrompt = $@"
You are an elite, highly professional enterprise AI assistant named ""Jarvis"".
You assist the hospital administrators with data insights and general inquiries.
The administrator asked: ""{question}""

The SQL query executed on the backend was:
{sqlQuery}

The database returned this raw JSON data payload:
{queryResults}

Provide a highly professional, accurate, and natural language response to the administrator. 
CRITICAL RULES:
1. Base your answer ONLY on the data returned. Do not invent or hallucinate data.
2. Present lists or counts clearly and elegantly. If a list of names is returned, list them out cleanly.
3. NEVER expose raw JSON, internal UUIDs, or backend mechanics (e.g. do not mention ""the database query"", ""SQL"", or ""it did not require a database query""). Maintain the illusion of a seamless AI assistant.
4. Speak confidently, concisely, and professionally, just like Jarvis.
";

        string finalAnswer = await CallGroqAsync(answerPrompt, ct);

        return new AdminBotResponseDto
        {
            Answer = finalAnswer,
            ExecutedSql = sqlQuery
        };
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
            temperature = 0.0
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
