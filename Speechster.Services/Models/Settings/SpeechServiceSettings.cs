namespace Speechster.Services.Models.Settings;

public class SpeechServiceSettings
{
    public string Key { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string Language { get; set; } = "en-US";
    public string VoiceName { get; set; } = "en-US-GuyNeural";
    public string AssistantName { get; set; } = string.Empty;
}
