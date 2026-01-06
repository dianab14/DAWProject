namespace MicroSocialPlatform.Services
{
    public interface IAiModerationService
    {
        Task<AiModerationResult> AnalyzeAsync(string text);
    }
}
