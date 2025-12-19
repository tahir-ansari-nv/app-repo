namespace TimeSheetPortal.Core.Interfaces;

public interface IMFAService
{
    Task<string> GenerateMFACodeAsync(Guid userId);
    Task<bool> ValidateMFACodeAsync(Guid userId, string code);
}
