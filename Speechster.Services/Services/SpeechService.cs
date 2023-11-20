using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Speechster.Services.Contracts;
using Speechster.Services.Models.Settings;

namespace Speechster.Services.Services;

public sealed class SpeechService : ISpeechService, IDisposable
{    
    private readonly AudioConfig _audioConfig;
    private readonly SpeechRecognizer _speechRecognizer;
    private readonly SpeechSynthesizer _speechSynthesizer;
    private readonly IGptService _gptService;
    private readonly ILogger<SpeechService> _logger;
    private readonly IOptions<SpeechServiceSettings> _options;
    private readonly string assistantName;
    private readonly string[] _keyPhrases;

    private bool isSpeaking;

    public SpeechService(
        AudioConfig audioConfig,
        SpeechRecognizer speechRecognizer,
        SpeechSynthesizer speechSynthesizer,
        IGptService gptService,
        ILogger<SpeechService> logger,
        IOptions<SpeechServiceSettings> options)
    {
        _audioConfig = audioConfig;
        _speechRecognizer = speechRecognizer;
        _speechSynthesizer = speechSynthesizer;

        _gptService = gptService;
        _logger = logger;
        _options = options;
        assistantName = options.Value.AssistantName;

        _keyPhrases = new string[]
        {
            $"ok {assistantName}",
            $"hello {assistantName}",
            $"hi {assistantName}",
            $"{assistantName}"
        };

        _speechRecognizer.Recognized += RecognizedSpeechEventHandler;
        _speechSynthesizer.SynthesisCompleted += SynthesisCompletedEventHandler;
    }

    public async Task StartContinousListeningAsync() => await _speechRecognizer.StartContinuousRecognitionAsync();

    public async Task StopContinousListeningAsync() => await _speechRecognizer.StopContinuousRecognitionAsync();

    private void SynthesisCompletedEventHandler(object? sender, SpeechSynthesisEventArgs e) => isSpeaking = false;

    private async void RecognizedSpeechEventHandler (object? sender, SpeechRecognitionEventArgs e)
    {
        if (e.Result.Reason == ResultReason.RecognizedSpeech)
        {
            var spokenWords = e.Result.Text;
            var words = spokenWords
                .ToLower()
                .Trim()
                .Replace(",", string.Empty)
                .Replace(".", string.Empty);

            if (words.StartsWith("stop " + assistantName))
            {
                await _speechSynthesizer.StopSpeakingAsync();
                isSpeaking = false;
            }
            else if (_keyPhrases.Any(phrase => words.StartsWith(phrase)) && !isSpeaking)
            { 
                isSpeaking = true;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n> {spokenWords}");
                var answer = await _gptService.GetCompletionAsync(spokenWords);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n{answer}");
                Console.ResetColor();

                await SpeakAsync(answer);
            }
        }
    }

    private async Task SpeakAsync(string text)
    {
        using SpeechSynthesisResult result = await _speechSynthesizer.StartSpeakingTextAsync(text);
        Console.WriteLine($"\nSpeech synthesis status: {result.Reason}");
    }

    public void Dispose()
    {
        _speechRecognizer.Dispose();
        _speechSynthesizer.Dispose();
        _audioConfig.Dispose();
    }
}
