using Microsoft.AspNetCore.Identity;
using SkillHub.Application.DTOs.Auth;
using SkillHub.Application.Interfaces;
using SkillHub.Domain.Constants;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Exceptions;

namespace SkillHub.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(UserManager<ApplicationUser> userManager, IJwtTokenGenerator jwtTokenGenerator)
    {
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new BusinessRuleException("Email cannot be empty.");

        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new BusinessRuleException("Full name cannot be empty.");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new BusinessRuleException("Password cannot be empty.");

        var existingUser = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (existingUser != null)
            throw new BusinessRuleException($"A user with email '{request.Email}' already exists.");

        var user = new ApplicationUser
        {
            UserName = request.Email.Trim(),
            Email = request.Email.Trim(),
            FullName = request.FullName.Trim(),
            Role = UserRoles.Participant,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new BusinessRuleException($"Registration failed: {errors}");
        }

        await _userManager.AddToRoleAsync(user, UserRoles.Participant);

        var token = _jwtTokenGenerator.GenerateToken(user);
        var expiration = _jwtTokenGenerator.GetExpiration();

        return new AuthResponseDto(token, user.Email, user.FullName, user.Role, expiration);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            throw new BusinessRuleException("Email and password are required.");

        var user = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (user == null)
            throw new BusinessRuleException("Invalid credentials.");

        var isValidPassword = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isValidPassword)
            throw new BusinessRuleException("Invalid credentials.");

        var token = _jwtTokenGenerator.GenerateToken(user);
        var expiration = _jwtTokenGenerator.GetExpiration();

        return new AuthResponseDto(token, user.Email!, user.FullName, user.Role, expiration);
    }
}
