namespace TimeSheetPortal.Core.Interfaces;

public interface IPasswordHashingService
{
    (string Hash, string Salt) HashPassword(string password);
    bool VerifyPassword(string password, string hash, string salt);
}
