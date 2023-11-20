using Microsoft.Extensions.Options;
using Speechster.Services.Models.Settings;
using Speechster.Services.Contracts;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using Speechster.Services.Utilities;

namespace Speechster.Services.Services;

public sealed class GptService : IGptService
{
    private readonly ILogger<GptService> _logger;
    private readonly IOptionsMonitor<AzureOpenAISettings> _options;
    private readonly IChatCompletion? _chatGPT;
    private readonly OpenAIChatHistory? chatHistory;

    private int tokensCount;

    public GptService(
        ILogger<GptService> logger,
        IOptionsMonitor<AzureOpenAISettings> options,
        IChatCompletion? chatGPT)
    {
        _logger = logger;
        _options = options;
        _chatGPT = chatGPT;

        tokensCount += _options.CurrentValue.Instructions.TokensCount(options.CurrentValue.Model);
        chatHistory = (OpenAIChatHistory)_chatGPT!.CreateNewChat(options.CurrentValue.Instructions);
    }

    public async Task<string> GetCompletionAsync(string question)
    {
        try
        {
            Guard.IsNotNullOrEmpty(question);

            TokensLimitCheck(question);
            chatHistory!.AddUserMessage(question);
            
            string completion = await _chatGPT!.GenerateMessageAsync(chatHistory);

            chatHistory!.AddAssistantMessage(completion);
            tokensCount += completion.TokensCount(_options.CurrentValue.Model);

            return completion;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Completion error : {exceptionMessage}", ex.Message);
            throw;
        }
    }

    private void TokensLimitCheck(string question)
    {
        var settings = _options.CurrentValue;
        var tokensSum = tokensCount + question.TokensCount(settings.Model);

        while (tokensSum > settings.MaxModelTokens && chatHistory?.Count > 2)
        {
            int targetIndex = 1;
            tokensCount -= chatHistory[targetIndex].Content.TokensCount(settings.Model);
            chatHistory.RemoveAt(targetIndex);
        }

        tokensCount = tokensSum;
    }
}
