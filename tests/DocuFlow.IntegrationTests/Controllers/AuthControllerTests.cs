using System.Net;
using System.Net.Http.Json;
using DocuFlow.Domain.Entities;
using DocuFlow.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace DocuFlow.IntegrationTests.Controllers;

public class AuthControllerTests : IClassFixture<DocuFlowWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly DocuFlowWebApplicationFactory _factory;

    public AuthControllerTests(DocuFlowWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task SeedTenantAsync(string slug)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (!db.Tenants.Any(t => t.Slug == slug))
        {
            var tenant = Tenant.Create(slug, slug);
            db.Tenants.Add(tenant);
            await db.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task Register_ValidRequest_Returns200()
    {
        // Arrange
        await SeedTenantAsync("test-tenant");

        var request = new
        {
            email = "john@test.com",
            password = "Password123!",
            firstName = "John",
            lastName = "Doe",
            tenantSlug = "test-tenant"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, because: body);
    }

    [Fact]
    public async Task Login_InvalidCredentials_Returns401()
    {
        // Arrange
        var request = new
        {
            email = "nobody@test.com",
            password = "WrongPassword!",
            tenantSlug = "test-tenant"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetDocuments_Unauthenticated_Returns401()
    {
        // Act
        var response = await _client.GetAsync("/api/documents");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}