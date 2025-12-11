using Amazon.Runtime;
using Amazon.S3;
using DocuFlow.Application.Abstractions.Repositories;
using DocuFlow.Application.Abstractions.Services;
using DocuFlow.Infrastructure.Identity;
using DocuFlow.Infrastructure.Persistence;
using DocuFlow.Infrastructure.Persistence.Repositories;
using DocuFlow.Infrastructure.Services;
using DocuFlow.Infrastructure.Services.AiExtraction;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DocuFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });

        // Identity
        services.AddIdentity<AppUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.User.RequireUniqueEmail = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        // R2 File Storage (S3-compatible)
        var r2AccountId = configuration["R2:AccountId"];
        var r2AccessKey = configuration["R2:AccessKeyId"];
        var r2SecretKey = configuration["R2:SecretAccessKey"];

        services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(
            new BasicAWSCredentials(r2AccessKey, r2SecretKey),
            new AmazonS3Config
            {
                ServiceURL = $"https://{r2AccountId}.r2.cloudflarestorage.com",
                ForcePathStyle = true
            }));

        // Repositories
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IExtractionJobRepository, ExtractionJobRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        // Services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ICurrentTenantService, CurrentTenantService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IDocumentProcessingService, DocumentProcessingService>();
        services.AddScoped<IBackgroundJobService, BackgroundJobService>();
        services.AddHttpClient<IWebhookService, WebhookService>();

        // AI Extraction — provider selected via config
        var aiProvider = configuration["AiProvider"] ?? "Mock";

        switch (aiProvider)
        {
            case "OpenAI":
                services.AddHttpClient<IAiExtractionService, OpenAiExtractionService>();
                break;
            case "AzureOpenAI":
                services.AddHttpClient<IAiExtractionService, AzureOpenAiExtractionService>();
                break;
            case "Groq":
                services.AddHttpClient<IAiExtractionService, GroqExtractionService>();
                break;
            default:
                services.AddScoped<IAiExtractionService, MockAiExtractionService>();
                break;
        }

        // Hangfire
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(options =>
                options.UseNpgsqlConnection(
                    configuration.GetConnectionString("DefaultConnection"))));

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = 5;
            options.Queues = new[] { "critical", "default" };
        });

        // HTTP Context
        services.AddHttpContextAccessor();

        return services;
    }
}