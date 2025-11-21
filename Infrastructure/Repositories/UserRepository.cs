using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Resend;

namespace Infrastructure.Repositories;

public class UserRepository(
    UserManager<User> userManager, 
    IResend resend, 
    IConfiguration config, 
    IWebHostEnvironment env
) : IUserRepository
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly IResend _resend = resend;
    private readonly IConfiguration _config = config;
    private readonly IWebHostEnvironment _env = env;

    #region Identity Operations

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await _userManager.FindByEmailAsync(email);

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _userManager.FindByIdAsync(id.ToString());

    public async Task<IdentityResult> CreateAsync(User user, string password)
        => await _userManager.CreateAsync(user, password);

    public async Task<bool> CheckPasswordAsync(User user, string password)
        => await _userManager.CheckPasswordAsync(user, password);

    public async Task<string> GenerateEmailConfirmationTokenAsync(User user)
        => await _userManager.GenerateEmailConfirmationTokenAsync(user);

    public async Task<string> GeneratePasswordResetTokenAsync(User user)
        => await _userManager.GeneratePasswordResetTokenAsync(user);

    public async Task<IdentityResult> ConfirmEmailAsync(User user, string token)
        => await _userManager.ConfirmEmailAsync(user, token);

    public async Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword)
        => await _userManager.ResetPasswordAsync(user, token, newPassword);

    #endregion

    #region Email Sending

    public async Task SendConfirmationEmailAsync(User user, string confirmationLink)
    {
        var subject = "Confirm your email address";
        var body = $@"
            <p>Hi {user.UserName},</p>
            <p>Thanks for registering! Please confirm your email by clicking the link below:</p>
            <p><a href='{confirmationLink}'>Verify Email</a></p>";

        await SendMailAsync(user.Email!, subject, body);
    }

    public async Task SendPasswordResetEmailAsync(User user, string resetLink)
    {
        var subject = "Reset your password";
        var body = $@"
            <p>Hi {user.UserName},</p>
            <p>You can reset your password by clicking the link below:</p>
            <p><a href='{resetLink}'>Reset Password</a></p>";

        await SendMailAsync(user.Email!, subject, body);
    }

    private async Task SendMailAsync(string email, string subject, string body)
    {
        var message = new EmailMessage
        {
            From = _config["Resend:Email:From"]!,
            Subject = subject,
            HtmlBody = body
        };

        if (_env.IsDevelopment())
        {
            // Only send emails to yourself in dev mode
            message.To.Add(_config["Resend:Email:To"]!);
        }
        else
        { 
            // Send to actual user in production
            message.To.Add(email);
        }

        await _resend.EmailSendAsync(message);
    }

    #endregion
}
