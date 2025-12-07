using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DocuFlow.Application.Abstractions.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DocuFlow.Infrastructure.Services.AiExtraction;

public class GroqExtractionService : IAiExtractionService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GroqExtractionService> _logger;

    public GroqExtractionService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<GroqExtractionService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ExtractionServiceResult> ExtractAsync(
        ExtractionRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GroqExtractionService extracting for schema {Schema}", request.Schema);

        var apiKey = _configuration["Groq:ApiKey"]
            ?? throw new InvalidOperationException("Groq:ApiKey is not configured.");

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        var prompt = BuildPrompt(request);

        var payload = new
        {
            model = "llama-3.3-70b-versatile",
            messages = new[]
            {
                new { role = "system", content = "You are a document extraction assistant. Extract fields from documents and return only valid JSON array. No markdown, no explanation, only the JSON array." },
                new { role = "user", content = prompt }
            },
            temperature = 0.1
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(
            "https://api.groq.com/openai/v1/chat/completions", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Groq API error: {Error}", error);
            return new ExtractionServiceResult(false, new List<ExtractedFieldResult>(), error);
        }

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        return ParseResponse(responseJson);
    }

    private string BuildPrompt(ExtractionRequest request)
    {
        var instructions = request.TenantInstructions is not null
            ? $"Additional instructions: {request.TenantInstructions}"
            : string.Empty;

        return $"Extract the following fields from this {request.Schema} document.\n" +
               $"{instructions}\n\n" +
               "Return a JSON array in this exact format, no other text, no markdown:\n" +
               "[\n" +
               "  {\"fieldName\": \"FieldName\", \"fieldValue\": \"value\", \"confidenceScore\": 0.95}\n" +
               "]\n\n" +
               $"Document content:\n{request.DocumentContent}";
    }

    private ExtractionServiceResult ParseResponse(string responseJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseJson);
            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;

            // Strip markdown code fences if model wraps response
            var clean = content
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            var fields = JsonSerializer.Deserialize<List<ExtractedFieldResult>>(
                clean,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return new ExtractionServiceResult(true, fields ?? new List<ExtractedFieldResult>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse Groq response");
            return new ExtractionServiceResult(false, new List<ExtractedFieldResult>(), ex.Message);
        }
    }
}