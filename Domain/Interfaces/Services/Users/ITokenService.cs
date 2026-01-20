using Domain.Entities.Core;

namespace Domain.Interfaces.Services.Users;

public interface ITokenService
{
    string CreateToken(User user);
}
