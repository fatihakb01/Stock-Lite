using Domain.Entities.Core;
using Microsoft.AspNetCore.Identity;

namespace Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IdentityResult> CreateAsync(User user, string password);
    Task<bool> CheckPasswordAsync(User user, string password);
    Task<string> GenerateEmailConfirmationTokenAsync(User user);
    Task<string> GeneratePasswordResetTokenAsync(User user);
    Task<IdentityResult> ConfirmEmailAsync(User user, string token);
    Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword);
    Task SendConfirmationEmailAsync(User user, string confirmationLink);
    Task SendPasswordResetEmailAsync(User user, string resetLink);
}
