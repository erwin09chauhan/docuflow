using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DocuFlow.Application.Abstractions.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DocuFlow.Infrastructure.Services.AiExtraction;

public class AzureOpenAiExtractionService : IAiExtractionService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AzureOpenAiExtractionService> _logger;

    public AzureOpenAiExtractionService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<AzureOpenAiExtractionService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ExtractionServiceResult> ExtractAsync(
        ExtractionRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("AzureOpenAiExtractionService extracting for schema {Schema}", request.Schema);

        var apiKey = _configuration["AzureOpenAi:ApiKey"]
            ?? throw new InvalidOperationException("AzureOpenAi:ApiKey is not configured.");

        var endpoint = _configuration["AzureOpenAi:Endpoint"]
            ?? throw new InvalidOperationException("AzureOpenAi:Endpoint is not configured.");

        var deploymentName = _configuration["AzureOpenAi:DeploymentName"]
            ?? throw new InvalidOperationException("AzureOpenAi:DeploymentName is not configured.");

        var apiVersion = _configuration["AzureOpenAi:ApiVersion"] ?? "2024-02-01";

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("api-key", apiKey);

        var prompt = BuildPrompt(request);

        var payload = new
        {
            messages = new[]
            {
                new { role = "system", content = "You are a document extraction assistant. Extract fields from documents and return only valid JSON." },
                new { role = "user", content = prompt }
            },
            temperature = 0.1
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var url = $"{endpoint}/openai/deployments/{deploymentName}/chat/completions?api-version={apiVersion}";
        var response = await _httpClient.PostAsync(url, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
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
               "Return a JSON array in this exact format, no other text:\n" +
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

            var fields = JsonSerializer.Deserialize<List<ExtractedFieldResult>>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return new ExtractionServiceResult(true, fields ?? new List<ExtractedFieldResult>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse Azure OpenAI response");
            return new ExtractionServiceResult(false, new List<ExtractedFieldResult>(), ex.Message);
        }
    }
}