using System.Security.Claims;

namespace TimeSheetPortal.Core.Interfaces;

public interface ITokenService
{
    string GenerateJwtToken(Guid userId, string username, string email);
    ClaimsPrincipal? ValidateToken(string token);
}
