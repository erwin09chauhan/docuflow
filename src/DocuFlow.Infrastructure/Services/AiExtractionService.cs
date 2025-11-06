using System.Net.Http.Json;
using System.Text.Json;
using DocuFlow.Application.Abstractions.Services;
using DocuFlow.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace DocuFlow.Infrastructure.Services;

public class AiExtractionService : IAiExtractionService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AiExtractionService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
        _httpClient.DefaultRequestHeaders.Add(
            "Authorization", $"Bearer {_configuration["OpenAi:ApiKey"]}");
    }

    public async Task<ExtractionServiceResult> ExtractAsync(
        ExtractionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var prompt = BuildPrompt(request);

            var payload = new
            {
                model = "gpt-4o",
                messages = new[]
                {
                    new { role = "system", content = "You are a document extraction assistant. Extract structured fields from documents and return them as JSON only." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.1
            };

            var response = await _httpClient.PostAsJsonAsync(
                "chat/completions", payload, cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<JsonElement>(
                cancellationToken: cancellationToken);

            var content = result
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "[]";

            var fields = JsonSerializer.Deserialize<List<ExtractedFieldResult>>(content)
                ?? new List<ExtractedFieldResult>();

            return new ExtractionServiceResult(true, fields);
        }
        catch (Exception ex)
        {
            return new ExtractionServiceResult(false, new List<ExtractedFieldResult>(), ex.Message);
        }
    }

    private static string BuildPrompt(ExtractionRequest request)
    {
        var schemaInstructions = request.Schema switch
        {
            ExtractionSchema.Contract => "Extract: parties involved, contract date, expiry date, value, key obligations.",
            ExtractionSchema.Invoice => "Extract: invoice number, vendor, date, due date, line items, total amount.",
            ExtractionSchema.Report => "Extract: title, author, date, summary, key findings.",
            ExtractionSchema.Auto => "Extract all meaningful structured fields you can identify.",
            _ => "Extract all meaningful structured fields."
        };

        var additionalInstructions = request.TenantInstructions != null
            ? $"Additional instructions: {request.TenantInstructions}"
            : string.Empty;

        return $"{schemaInstructions}\n{additionalInstructions}\n\n" +
               "Return ONLY a JSON array in this format:\n" +
               "[{\"fieldName\": \"...\", \"fieldValue\": \"...\", \"confidenceScore\": 0.95}]\n\n" +
               $"Document content:\n{request.DocumentContent}";
    }
}