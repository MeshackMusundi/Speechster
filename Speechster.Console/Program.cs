using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Speechster.Services;
using Speechster.Services.Contracts;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddUserSecrets<Program>()
    .Build();

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSpeechsterServices(configuration);
builder.Services.AddLogging();

using IHost host = builder.Build();

var speechService = host.Services.GetService<ISpeechService>();

Console.WriteLine("Start speaking...");

await speechService!.StartContinousListeningAsync();

Console.ReadKey();

await speechService.StopContinousListeningAsync();