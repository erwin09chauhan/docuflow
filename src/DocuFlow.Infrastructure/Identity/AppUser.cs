using DocuFlow.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace DocuFlow.Infrastructure.Identity;

public class AppUser : IdentityUser
{
    public Guid TenantId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Viewer;
}