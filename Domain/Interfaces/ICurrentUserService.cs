namespace Domain.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
    Guid CompanyId { get; }
    bool IsAdmin { get; }
    bool IsSuperAdmin { get; }
}
