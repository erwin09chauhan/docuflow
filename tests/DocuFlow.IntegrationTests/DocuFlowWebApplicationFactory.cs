using DocuFlow.Infrastructure.Persistence;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DocuFlow.IntegrationTests;

public class TestAppDbContext : AppDbContext
{
    public TestAppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Ignore<DocuFlow.Domain.Common.DomainEvent>();
    }
}

public class DocuFlowWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();

            // Remove hosted services that might connect to postgres
            var hostedServices = services
                .Where(d => d.ServiceType.FullName?.Contains("IHostedService") == true)
                .ToList();
            foreach (var hs in hostedServices)
                services.Remove(hs);

            // Remove Hangfire services
            var hangfireServices = services
                .Where(d => d.ServiceType.FullName?.Contains("Hangfire") == true)
                .ToList();
            foreach (var hs in hangfireServices)
                services.Remove(hs);

            // Add Hangfire with in-memory storage for tests
            services.AddHangfire(config => config.UseInMemoryStorage());

            // Register in-memory DbContext
            services.AddScoped<AppDbContext>(sp =>
            {
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options;
                return new TestAppDbContext(options);
            });

            // Ensure DB is created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
    }
}