namespace Speechster.Services.Models.Settings;

public class AzureOpenAISettings
{
    public string Key { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string Deployment { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public int MaxCompletionTokens { get; set; }
    public int MaxModelTokens { get; set; }
}
