namespace Smartitecture.Services
{
    public static class AppSession
    {
        public static ILLMService LlmService { get; } = new MultiModelAIService();
    }
}

