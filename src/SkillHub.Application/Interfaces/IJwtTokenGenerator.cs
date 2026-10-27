using SkillHub.Domain.Entities;

namespace SkillHub.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(ApplicationUser user);
    DateTime GetExpiration();
}
