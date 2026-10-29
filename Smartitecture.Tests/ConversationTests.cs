using System.Runtime.CompilerServices;
using Smartitecture.Services;
using Smartitecture.Services.Core;
using Smartitecture.Services.Interfaces;
using Xunit;

namespace Smartitecture.Tests;

public sealed class ConversationTests
{
    [Fact]
    public async Task MultiModelAIService_GetStreamingResponseAsync_ShouldRestoreHistoryWithoutDuplicatingCurrentPrompt()
    {
        LLMRequest? captured = null;
        var provider = new TestProvider((request, _) =>
        {
            captured = request;
            return Task.FromResult(new LLMResponse { Content = "An answer" });
        });
        var service = new MultiModelAIService(backendProvider: provider);
        service.SetConversationHistory("saved", new[]
        {
            new ConversationMessage { Role = "user", Content = "Check performance" },
            new ConversationMessage { Role = "assistant", Content = "Memory usage is 90%." },
            new ConversationMessage { Role = "system", Content = "Internal status" }
        });

        await service.GetStreamingResponseAsync("What does that mean?", _ => { }, "saved");

        Assert.NotNull(captured);
        Assert.Equal("What does that mean?", captured.UserMessage);
        Assert.Equal(2, captured.History.Count);
        Assert.Equal("Memory usage is 90%.", captured.History[1].Content);
        Assert.Equal(4, (await service.GetConversationHistoryAsync("saved")).Count);
    }

    [Fact]
    public async Task MultiModelAIService_GetStreamingResponseAsync_ShouldCancelWithoutCallingFallback()
    {
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var fallbackCalls = 0;
        var provider = new TestProvider(async (_, token) =>
        {
            started.SetResult();
            await Task.Delay(Timeout.Infinite, token);
            return new LLMResponse();
        });
        var fallback = new TestProvider((_, _) =>
        {
            fallbackCalls++;
            return Task.FromResult(new LLMResponse { Content = "Unexpected fallback" });
        });
        var service = new MultiModelAIService(provider, fallback);
        using var cancellation = new CancellationTokenSource();

        var response = service.GetStreamingResponseAsync("Hello", _ => { }, "cancelled", cancellation.Token);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => response);
        Assert.Equal(0, fallbackCalls);
        Assert.DoesNotContain(await service.GetConversationHistoryAsync("cancelled"), m => m.Role == "assistant");
    }

    [Fact]
    public async Task MultiModelAIService_SetConversationHistory_ShouldCopyMessagesAndKeepConversationsSeparate()
    {
        var service = new MultiModelAIService();
        var original = new ConversationMessage { Role = "assistant", Content = "Previous answer" };
        service.SetConversationHistory("old", new[] { original });
        original.Content = "Changed externally";

        Assert.Equal("Previous answer", (await service.GetConversationHistoryAsync("old"))[0].Content);
        Assert.Empty(await service.GetConversationHistoryAsync("new"));
    }

    private sealed class TestProvider(Func<LLMRequest, CancellationToken, Task<LLMResponse>> respond) : ILLMProvider
    {
        public string Name => "Test provider";
        public bool IsConfigured => true;
        public Task<LLMResponse> GetResponseAsync(LLMRequest request, CancellationToken cancellationToken)
            => respond(request, cancellationToken);

        public async IAsyncEnumerable<string> StreamResponseAsync(LLMRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            yield return (await respond(request, cancellationToken)).Content;
        }
    }
}
