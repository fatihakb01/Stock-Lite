using Domain.Entities.Core;

namespace Domain.Interfaces;

public interface ITokenService
{
    string CreateToken(User user);
}
