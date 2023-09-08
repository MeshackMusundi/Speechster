namespace Speechster.Services.Contracts;

public interface ISpeechService
{
    Task StartContinousListeningAsync();
    Task StopContinousListeningAsync();
}
