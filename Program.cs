using Anthropic;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace agent_investigator;

internal class Program
{
	static async Task Main(string[] args)
	{
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
				.GetChatClient(requestedModel ?? Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-5-nano")
				.AsAIAgent(instructions: null),

			// Azure uses a deployment name as its model argument; requires AZUREAI_API_KEY.
			"azure" => new AzureOpenAIClient(
						new Uri(Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
							?? "https://dk-resource.services.ai.azure.com"),
						new ApiKeyCredential(RequiredEnvironmentVariable("AZUREAI_API_KEY")))
					.GetChatClient(requestedModel ?? RequiredEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT"))
					.AsAIAgent(instructions: null),

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

		AgentResponse response = await agent.RunAsync("What is the capital of France?");
		Console.WriteLine(response);
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
