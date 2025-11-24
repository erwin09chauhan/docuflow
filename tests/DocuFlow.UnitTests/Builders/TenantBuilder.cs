using DocuFlow.Domain.Entities;

namespace DocuFlow.UnitTests.Builders;

public class TenantBuilder
{
    private string _name = "Test Tenant";
    private string _slug = "test-tenant";
    private string _plan = "Free";

    public TenantBuilder WithName(string name) { _name = name; return this; }
    public TenantBuilder WithSlug(string slug) { _slug = slug; return this; }
    public TenantBuilder WithPlan(string plan) { _plan = plan; return this; }

    public Tenant Build() => Tenant.Create(_name, _slug, _plan);
}