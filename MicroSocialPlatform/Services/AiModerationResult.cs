namespace MicroSocialPlatform.Services
{
    public class AiModerationResult
    {
        public bool IsAppropriate { get; set; }
        public double Confidence { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
