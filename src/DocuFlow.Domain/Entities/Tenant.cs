using DocuFlow.Domain.Common;

namespace DocuFlow.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string Plan { get; private set; } = "Free";
    public bool IsActive { get; private set; }

    private readonly List<User> _users = new();
    private readonly List<Document> _documents = new();

    public IReadOnlyCollection<User> Users => _users.AsReadOnly();
    public IReadOnlyCollection<Document> Documents => _documents.AsReadOnly();

    private Tenant() { }

    public static Tenant Create(string name, string slug, string plan = "Free")
    {
        return new Tenant
        {
            Name = name,
            Slug = slug.ToLower().Trim(),
            Plan = plan,
            IsActive = true
        };
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
    public void UpdatePlan(string plan) => Plan = plan;
}