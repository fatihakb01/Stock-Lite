using Domain.Enums;

namespace Application.Entities.Core.Users.DTOs
{
    public class CurrentUserDto
    {
        public required Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public UserRole Role { get; set; }
    }
}
