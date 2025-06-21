using GroqApiLibrary;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

class Program
{
    static List<JObject> myHistoryChat = new List<JObject>();

    static void Main(string[] args)
    {
        var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
        string groqAiKey = config.GetSection("GROQ_API_KEY").Value;
        GroqApiLibrary.GroqApiClient groqApi = new GroqApiLibrary.GroqApiClient(groqAiKey);
        RunChatLoop(groqApi).Wait();
    }

    static async Task RunChatLoop (GroqApiClient groqApi)
    {
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Você: ");
            Console.ForegroundColor = ConsoleColor.White;
            string userInput = Console.ReadLine();

            myHistoryChat.Add(new JObject
            {
                ["role"] = "user",
                ["content"] = userInput
            });

            int maxMessagesSize = 8;
            int messagesToRemoveCount = Math.Max(0, myHistoryChat.Count - maxMessagesSize);
            myHistoryChat.RemoveRange(0, messagesToRemoveCount);

            JObject response = GenerateAIResponce(groqApi).Result;
            string? aiResponse = response?["choices"]?[0]?["message"]?["content"]?.ToString();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(aiResponse);
            Console.ForegroundColor = ConsoleColor.White;

            myHistoryChat.Add(new JObject
            {
                ["role"] = "assistant",
                ["content"] = aiResponse
            });
        }
    }

    static async Task<JObject> GenerateAIResponce(GroqApiClient anApi)
    {
        JArray totalChatJArray = new JArray();

        foreach (var chat in myHistoryChat)
        {
            totalChatJArray.Add(chat);
        }
        JObject request = new JObject
        {
            ["model"] = "llama-3.1-8b-instant",
            ["messages"] = totalChatJArray
        };
        try
        {
            var result = await anApi.CreateChatCompletionAsync(request);
            return result;
        }
        catch (HttpRequestException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Erro de comunicação com a API: {ex.Message}");
            Console.ForegroundColor = ConsoleColor.White;
            return new JObject();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Ocorreu um erro inesperado: {ex.Message}");
            Console.ForegroundColor = ConsoleColor.White;
            return new JObject();
        }
    }
}