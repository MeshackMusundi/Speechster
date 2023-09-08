namespace Speechster.Services.Contracts;

public interface IGptService
{
    Task<string> GetCompletionAsync(string input);
}
