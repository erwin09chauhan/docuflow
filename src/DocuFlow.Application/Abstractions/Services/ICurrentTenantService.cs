namespace DocuFlow.Application.Abstractions.Services;

public interface ICurrentTenantService
{
    Guid? TenantId { get; }
    void SetTenant(Guid tenantId);
}