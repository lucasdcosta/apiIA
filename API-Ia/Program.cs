using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using System.Text;

namespace ConsoleApp.AzureAI
{
    public class TemperatureSettings
    {
        public double Temperature { get; set; } = 0.7;
    }

    public class Program
    {
        private static string AzureOpenAIEndpoint = "";
        private static string AzureOpenAIKey = "";
        private static string DeploymentName = "gpt-35-turbo";
        private static string PromptFilePath => Path.Combine(Directory.GetCurrentDirectory(), "prompt.txt");
        private static string TemperatureFilePath => Path.Combine(Directory.GetCurrentDirectory(), "temperature.json");

        public static async Task Main(string[] args)
        {
            IConfiguration configuration;
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            AzureOpenAIEndpoint = configuration["AzureOpenAI:Endpoint"] ?? "";
            AzureOpenAIKey = configuration["AzureOpenAI:Key"] ?? "";
            DeploymentName = configuration["AzureOpenAI:DeploymentName"] ?? "";

            Console.OutputEncoding = Encoding.UTF8;
            ConsoleColor defaultColor = Console.ForegroundColor;

            DisplayHeader(defaultColor);

            try
            {
                AzureOpenAIClient client = new AzureOpenAIClient(
                    new Uri(AzureOpenAIEndpoint),
                    new AzureKeyCredential(AzureOpenAIKey));

                ChatClient chatClient = client.GetChatClient(DeploymentName);

                bool continueChat = true;
                var chatHistory = new List<ChatMessage>();

                while (continueChat)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\nEntre sua mensagem (digite 'sair' para sair, 'nova' para começar nova conversa, 'prompt' para recarregar o prompt, 'temp' para recarregar a temperatura):");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("> ");

                    string userInput = Console.ReadLine() ?? "";

                    if (string.Equals(userInput, "sair", StringComparison.OrdinalIgnoreCase))
                    {
                        continueChat = false;
                        continue;
                    }

                    if (string.Equals(userInput, "nova", StringComparison.OrdinalIgnoreCase))
                    {
                        chatHistory.Clear();
                        Console.WriteLine("Nova conversa iniciada.");
                        continue;
                    }

                    if (string.Equals(userInput, "prompt", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("Recarregando arquivo de prompt...");
                        continue;
                    }

                    if (string.Equals(userInput, "temp", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("Recarregando configurações de temperatura...");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(userInput))
                    {
                        continue;
                    }

                    try
                    {
                        Console.WriteLine("\nEnviando mensagem para o Azure OpenAI...");
                        var temperatureSettings = ReadTemperatureSettings();

                        var chatCompletionOptions = new ChatCompletionOptions
                        {
                            Temperature = (float)temperatureSettings.Temperature,
                        };

                        chatHistory.RemoveAll(msg => msg is SystemChatMessage);

                        string prompt = ReadPromptFile();
                        chatHistory.Insert(0, new SystemChatMessage(prompt));

                        chatHistory.Add(new UserChatMessage(userInput));

                        var response = await chatClient.CompleteChatAsync(chatHistory, chatCompletionOptions);

                        if (response.Value.Content.Count > 0)
                        {
                            string responseMessage = response.Value.Content[0].Text;

                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine("\nResposta do Azure OpenAI:");
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine(responseMessage);
                            Console.ForegroundColor = defaultColor;

                            chatHistory.Add(new AssistantChatMessage(responseMessage));
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Nenhuma resposta recebida.");
                            Console.ForegroundColor = defaultColor;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Erro: {ex.Message}");
                        Console.ForegroundColor = defaultColor;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Erro ao inicializar o cliente Azure OpenAI: {ex.Message}");
                Console.ForegroundColor = defaultColor;
            }
        }
        private static string ReadPromptFile()
        {
                return File.ReadAllText(PromptFilePath);
        }
        private static TemperatureSettings ReadTemperatureSettings()
        {
            string temperatureJson = File.ReadAllText(TemperatureFilePath);
            var settings = System.Text.Json.JsonSerializer.Deserialize<TemperatureSettings>(temperatureJson);
            return settings!;
        }
        private static void DisplayHeader(ConsoleColor defaultColor)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("===========================================");
            Console.WriteLine("    Azure Workshop");
            Console.WriteLine("===========================================");
            Console.ForegroundColor = defaultColor;
        }
    }
}