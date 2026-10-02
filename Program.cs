using Anthropic;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Collections.Generic;

namespace agent_investigator;

internal class Program
{
    private const string separator = "--------";

    static async Task Main(string[] args)
    {
		string modelInstructions = "You are a helpful assistant that answers questions and provides information.";
		
        // Usage: dotnet run -- [provider] [model]
        // No arguments use OpenAI. The model argument overrides the provider's environment setting or default.
        if (args.Length > 2 || (args.Length > 0 && string.IsNullOrWhiteSpace(args[0])) ||
            (args.Length > 1 && string.IsNullOrWhiteSpace(args[1])))
        {
            throw new ArgumentException("Usage: dotnet run -- [provider] [model]");
        }

        string provider = args.Length > 0 ? args[0].Trim().ToLowerInvariant() : "openai";
        string? requestedModel = args.Length > 1 ? args[1].Trim() : null;

        AIAgent agent = provider switch
        {
            // Direct OpenAI API; requires OPENAI_API_KEY.
            "openai" => new OpenAIClient(RequiredEnvironmentVariable("OPENAI_API_KEY"))
                .GetChatClient(requestedModel ?? Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-4.1-nano")
                .AsAIAgent(instructions: modelInstructions),

            // Azure uses a deployment name as its model argument; requires AZUREAI_API_KEY.
            "azure" => new AzureOpenAIClient(
                        new Uri(Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
                            ?? "https://dk-resource.services.ai.azure.com"),
                        new ApiKeyCredential(RequiredEnvironmentVariable("AZUREAI_API_KEY")))
                    .GetChatClient(requestedModel ?? RequiredEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT"))
                    .AsAIAgent(instructions: modelInstructions),

            // Native Anthropic API; requires ANTHROPIC_API_KEY.
            "anthropic" => new AnthropicClient
            {
                ApiKey = RequiredEnvironmentVariable("ANTHROPIC_API_KEY")
            }
                .AsAIAgent(model: requestedModel ?? Environment.GetEnvironmentVariable("ANTHROPIC_MODEL") ?? "claude-haiku-4-5"),

            // Kimi's OpenAI-compatible API; requires MOONSHOT_API_KEY.
            "kimi" => OpenAICompatibleAgent(
                requestedModel ?? Environment.GetEnvironmentVariable("KIMI_MODEL") ?? "kimi-k2.6",
                RequiredEnvironmentVariable("MOONSHOT_API_KEY"),
                Environment.GetEnvironmentVariable("KIMI_BASE_URL") ?? "https://api.moonshot.ai/v1/"),

            // Ollama's local OpenAI-compatible API; pull the selected Llama model first.
            "llama" => OpenAICompatibleAgent(
                requestedModel ?? Environment.GetEnvironmentVariable("OLLAMA_MODEL") ?? "llama3.2",
                "ollama", // Local Ollama ignores this required OpenAI SDK value.
                Environment.GetEnvironmentVariable("OLLAMA_BASE_URL") ?? "http://localhost:11434/v1/"),

            // OpenRouter uses model IDs such as provider/model and requires OPENROUTER_API_KEY.
            "openrouter" => OpenAICompatibleAgent(
                requestedModel ?? throw new ArgumentException("OpenRouter requires a model argument."),
                RequiredEnvironmentVariable("OPENROUTER_API_KEY"),
                Environment.GetEnvironmentVariable("OPENROUTER_BASE_URL") ?? "https://openrouter.ai/api/v1/"),

            _ => throw new ArgumentException(
                $"Unknown provider '{provider}'. Use openai, azure, anthropic, kimi, llama, or openrouter.")
        };

//        await NewMethod2(agent);

//        NewMethod1(agent);

 //       NewMethod(agent);

        AgentSession session = await agent.CreateSessionAsync();

        while (true)
        {
            Console.Write(">");
            string input = Console.ReadLine() ?? string.Empty;
            List<AgentResponseUpdate> updates2 = new List<AgentResponseUpdate>();
            await foreach (AgentResponseUpdate update in agent.RunStreamingAsync(input, session))
            {
                updates2.Add(update);
                Console.WriteLine(update);
            }

            AgentResponse collectedResponse2 = updates2.ToAgentResponse();
            if (collectedResponse2.Usage != null)
            {
                Console.WriteLine();
                Console.WriteLine($"Tokens - In: {collectedResponse2.Usage.InputTokenCount}, Out: {collectedResponse2.Usage.OutputTokenCount}");
            }

            Console.WriteLine(separator);
        }
    }

    private static async Task NewMethod2(AIAgent agent)
    {
        AgentResponse response = await agent.RunAsync("What is the capital of France?");
        Console.WriteLine(response);

        Console.WriteLine(separator);
    }

    private static void NewMethod1(AIAgent agent)
    {
        Console.WriteLine("Streaming call (gathering output as it arrives):");
        await foreach (AgentResponseUpdate update in agent.RunStreamingAsync("How to make soup?"))
        {
            Console.WriteLine(update);
        }

        Console.WriteLine(separator);
    }

    private static void NewMethod(AIAgent agent)
    {
        Console.WriteLine("Streaming Call (gathering all updates to a response at the end)");
        List<AgentResponseUpdate> updates = [];
        await foreach (AgentResponseUpdate update in agent.RunStreamingAsync("How to make soup?"))
        {
            updates.Add(update);
            Console.WriteLine(update);
        }

        AgentResponse collectedResponse = updates.ToAgentResponse();

        //Use to the usage, and other return data
        Console.WriteLine(collectedResponse.Usage!.OutputTokenCount);
        Console.WriteLine(separator);
    }

    static AIAgent OpenAICompatibleAgent(string model, string apiKey, string endpoint)
	{
		OpenAIClient client = new(
			new ApiKeyCredential(apiKey),
			new OpenAIClientOptions { Endpoint = new Uri(endpoint) });

		return client.GetChatClient(model).AsAIAgent(instructions: null);
	}

	static string RequiredEnvironmentVariable(string name)
	{
		string? value = Environment.GetEnvironmentVariable(name);
		return !string.IsNullOrWhiteSpace(value)
			? value
			: throw new InvalidOperationException($"{name} is not set");
	}
}
