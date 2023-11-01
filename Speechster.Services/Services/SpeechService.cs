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
    private readonly string[] _keyPhrases;

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

        _keyPhrases = new string[]
        {
            $"ok {options.Value.AssistantName}",
            $"hello {options.Value.AssistantName}",
            $"hi {options.Value.AssistantName}",
            $"{options.Value.AssistantName}"
        };

        _speechRecognizer.Recognized += RecognizedSpeechEventHandler;
    }

    private string previousWords = string.Empty;
    private bool allowResponse = true;    

    public async Task StartContinousListeningAsync() => await _speechRecognizer.StartContinuousRecognitionAsync();

    public async Task StopContinousListeningAsync() => await _speechRecognizer.StopContinuousRecognitionAsync();

    private async void RecognizedSpeechEventHandler (object? sender, SpeechRecognitionEventArgs e)
    {
        if (e.Result.Reason == ResultReason.RecognizedSpeech)
        {
            var spokenWords = e.Result.Text;

            if (CanRespond(spokenWords))
            {
                Console.WriteLine($"\n>> {spokenWords}");

                allowResponse = false;
                var answer = await _gptService.GetCompletionAsync(spokenWords);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n{answer}");
                Console.ResetColor();

                await SpeakAsync(answer);
                allowResponse = true;
            }

            previousWords = spokenWords
                .ToLower()
                .Trim()
                .Replace(",", string.Empty)
                .Replace(".", string.Empty);
        }
        //else if (e.Result.Reason == ResultReason.NoMatch)
        //{
        //    _logger.LogInformation($"NOMATCH: Speech could not be recognized.");
        //}
    }

    private bool CanRespond(string spokenWords)
    {
        var words = spokenWords.ToLower();
        var assistantName = _options.Value.AssistantName;

        return allowResponse &&
            _keyPhrases.Any(kp => kp.Equals(previousWords, StringComparison.OrdinalIgnoreCase)) ||
            (words.StartsWith(assistantName) && words.Length > assistantName.Length + 3);
    }

    private async Task SpeakAsync(string text)
    {
        using SpeechSynthesisResult result = await _speechSynthesizer.SpeakTextAsync(text);
        Console.WriteLine($"\nSpeech synthesis status: {result.Reason}");
    }

    public void Dispose()
    {
        _speechRecognizer.Dispose();
        _speechSynthesizer.Dispose();
        _audioConfig.Dispose();
    }
}
