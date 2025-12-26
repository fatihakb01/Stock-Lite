namespace Domain.Interfaces;

public interface ICurrentUserService
{
    Guid Id { get; }
    Guid CompanyId { get; }
    bool IsAdmin { get; }
    bool IsSuperAdmin { get; }
}
