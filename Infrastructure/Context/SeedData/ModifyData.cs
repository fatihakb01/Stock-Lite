using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context.SeedData;

public static class ModifyData
{
    public static async Task RunAsync(DataContext context)
    {
        var users = await context.Users.ToListAsync();
        if (users.Count == 0)
            return;

        var passwordHasher = new PasswordHasher<User>();
        string newPassword = "Password123!";

        foreach (var user in users)
        {
            // 1. Normalize email + username
            user.NormalizedEmail = user.Email?.ToUpperInvariant();
            user.NormalizedUserName = user.UserName?.ToUpperInvariant();

            // 2. Set a SecurityStamp if missing
            if (string.IsNullOrEmpty(user.SecurityStamp))
                user.SecurityStamp = Guid.NewGuid().ToString();

            // 3. Set ConcurrencyStamp if missing
            if (string.IsNullOrEmpty(user.ConcurrencyStamp))
                user.ConcurrencyStamp = Guid.NewGuid().ToString();

            // 4. Make sure email is confirmed by default
            user.EmailConfirmed = true;
            user.IsActive = true;

            // 5. Hash a new password
            user.PasswordHash = passwordHasher.HashPassword(user, newPassword);
        }

        await context.SaveChangesAsync();
    }
}
