using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Speechster.Services.Contracts;
using Speechster.Services.Models.Settings;
using Speechster.Services.Services;

namespace Speechster.Services;

public static class SpeechsterServicesRegistration
{
    public static IServiceCollection AddSpeechsterServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<SpeechServiceSettings>(configuration.GetSection(nameof(SpeechServiceSettings)));
        services.Configure<AzureOpenAISettings>(configuration.GetSection(nameof(AzureOpenAISettings)));

        var speechServiceSettings = configuration.GetSection(nameof(SpeechServiceSettings)).Get<SpeechServiceSettings>();
        var azureOpenAISettings = configuration.GetSection(nameof(AzureOpenAISettings)).Get<AzureOpenAISettings>();

        var speechConfig = SpeechConfig.FromSubscription(speechServiceSettings!.Key, speechServiceSettings.Region);
        speechConfig.SpeechRecognitionLanguage = speechServiceSettings.Language;
        speechConfig.SpeechSynthesisVoiceName = speechServiceSettings.VoiceName;

        var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
        var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);
        var speechSynthesizer = new SpeechSynthesizer(speechConfig);

        var kernelBuilder = new KernelBuilder()
            .WithAzureOpenAIChatCompletionService(
            azureOpenAISettings!.Deployment,
            azureOpenAISettings!.Endpoint,
            azureOpenAISettings!.Key);

        var kernel = kernelBuilder.Build();

        var chatGPT = kernel.GetService<IChatCompletion>();

        services.AddSingleton(c => chatGPT);
        services.AddSingleton<IGptService, GptService>();

        services.AddSingleton(audioConfig);
        services.AddSingleton(speechRecognizer);
        services.AddSingleton(speechSynthesizer);
        services.AddSingleton<ISpeechService, SpeechService>();

        return services;
    }
}
