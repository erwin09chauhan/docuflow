using DocuFlow.Domain.Common;
using DocuFlow.Domain.Enums;

namespace DocuFlow.Domain.Entities;

public class User : AuditableEntity
{
    public Guid TenantId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    public string FullName => $"{FirstName} {LastName}";

    private User() { }

    public static User Create(Guid tenantId, string email, string passwordHash,
        string firstName, string lastName, UserRole role = UserRole.Viewer)
    {
        return new User
        {
            TenantId = tenantId,
            Email = email.ToLower().Trim(),
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName,
            Role = role,
            IsActive = true
        };
    }

    public void UpdateRole(UserRole role) => Role = role;
    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
    public void RecordLogin() => LastLoginAt = DateTime.UtcNow;
    public void UpdatePassword(string passwordHash) => PasswordHash = passwordHash;
}