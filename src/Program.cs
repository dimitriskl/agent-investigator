using System.ClientModel;
using Anthropic;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;

namespace agent_investigator;

internal class Program
{
    private const string Appname = "agent-investigator";

    private static async Task Main(string[] args)
    {
        SettingsStore settingsStore =
            new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                Appname));

        SettingsStoreResult result = settingsStore.CreateOrLoadSettings();

        if (result.IsSuccess && !result.LogPaths.Any())
        {
            Console.WriteLine("Please enter log paths (type q to finish, or /exit to exit the investigator):");
            string logPath = string.Empty;
            while (logPath != "q" && logPath != "/exit")
            {
                logPath = Console.ReadLine() ?? "/exit";

                if (logPath != "q" && logPath != "/exit")
                {
                    if (string.IsNullOrWhiteSpace(logPath))
                    {
                        Console.WriteLine("Please enter a valid log path:");
                    }
                    else
                    {
                        bool isValid = Directory.Exists(logPath);
                        if (isValid)
                        {
                            result = settingsStore.AddLogPath(logPath);
                        }
                        else
                        {
                            Console.WriteLine("The path dosen't exist");
                        }
                    }
                }
            }

            if ((result.IsSuccess && !result.LogPaths.Any()) || !result.IsSuccess)
            {
                Console.WriteLine(
                    "There is an error with the log paths application has to workwith. Please try again:");
                return;
            }

            if (logPath == "/exit")
            {
                return;
            }
        }


        string modelInstructions = "You are a log investigator. " +
                                   "Always use your tools to look at the real log files before answering. " +
                                   "Never say you cannot access logs; call list_log_files instead.";

        // Usage: dotnet run -- [provider] [model]
        // No arguments use OpenAI. The model argument overrides the provider's environment setting or default.
        if (args.Length > 2 || (args.Length > 0 && string.IsNullOrWhiteSpace(args[0])) ||
            (args.Length > 1 && string.IsNullOrWhiteSpace(args[1])))
        {
            throw new ArgumentException("Usage: dotnet run -- [provider] [model]");
        }

        //Get agent from command prompt parameters
        string provider = args.Length > 0 ? args[0].Trim().ToLowerInvariant() : "openai";
        string? requestedModel = args.Length > 1 ? args[1].Trim() : null;

        //Create tools
        AgentTools agentTools = new(result.LogPaths);

        //Create the agent
        AIAgent agent = AiAgent(provider, requestedModel, modelInstructions, agentTools.Tools);

        //Create a session for the agent
        AgentSession session = await agent.CreateSessionAsync();

        //Keep the loop of conversation going
        bool doTheLoop = true;

        //Start the chat loop
        while (doTheLoop)
        {
            Console.Write(">");
            string input = Console.ReadLine() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(input))
            {
                //Give system coomands
                if (input.StartsWith("/"))
                {
                    //Exit the app
                    if (input == "/exit")
                    {
                        doTheLoop = false;
                    }
                }
                else
                {
                    //Get the response and display it
                    List<AgentResponseUpdate> updates = new();
                    await foreach (AgentResponseUpdate update in agent.RunStreamingAsync(input, session))
                    {
                        updates.Add(update);
                        Console.Write(update);
                    }

                    AgentResponse collectedResponse2 = updates.ToAgentResponse();
                    if (collectedResponse2.Usage != null)
                    {
                        Console.WriteLine();
                        Console.WriteLine(
                            $"Tokens - In: {collectedResponse2.Usage.InputTokenCount}, Out: {collectedResponse2.Usage.OutputTokenCount}");
                    }
                }
            }
            else
            {
                Console.WriteLine("Type your request or /exit to exit the investigator.");
                Console.Write(">");
            }
        }
    }

    /// <summary>
    ///     Create an Ai Agent
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="requestedModel"></param>
    /// <param name="modelInstructions"></param>
    /// <param name="tools"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private static AIAgent AiAgent(string provider, string? requestedModel, string modelInstructions,
        List<AITool> tools)
    {
        AIAgent agent = provider switch
        {
            // Direct OpenAI API; requires OPENAI_API_KEY.
            "openai" => new OpenAIClient(RequiredEnvironmentVariable("OPENAI_API_KEY"))
                .GetChatClient(requestedModel ?? Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-4.1-nano")
                .AsAIAgent(modelInstructions, tools: tools),

            // Azure uses a deployment name as its model argument; requires AZUREAI_API_KEY.
            "azure" => new AzureOpenAIClient(
                    new Uri(Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
                            ?? "https://dk-resource.services.ai.azure.com"),
                    new ApiKeyCredential(RequiredEnvironmentVariable("AZUREAI_API_KEY")))
                .GetChatClient(requestedModel ?? RequiredEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT"))
                .AsAIAgent(modelInstructions),

            // Native Anthropic API; requires ANTHROPIC_API_KEY.
            "anthropic" => new AnthropicClient
                {
                    ApiKey = RequiredEnvironmentVariable("ANTHROPIC_API_KEY")
                }
                .AsAIAgent(
                    requestedModel ?? Environment.GetEnvironmentVariable("ANTHROPIC_MODEL") ?? "claude-haiku-4-5"),

            // Kimi's OpenAI-compatible API; requires MOONSHOT_API_KEY.
            "kimi" => OpenAiCompatibleAgent(
                requestedModel ?? Environment.GetEnvironmentVariable("KIMI_MODEL") ?? "kimi-k2.6",
                RequiredEnvironmentVariable("MOONSHOT_API_KEY"),
                Environment.GetEnvironmentVariable("KIMI_BASE_URL") ?? "https://api.moonshot.ai/v1/"),

            // Ollama's local OpenAI-compatible API; pull the selected Llama model first.
            "llama" => OpenAiCompatibleAgent(
                requestedModel ?? Environment.GetEnvironmentVariable("OLLAMA_MODEL") ?? "llama3.2",
                "ollama", // Local Ollama ignores this required OpenAI SDK value.
                Environment.GetEnvironmentVariable("OLLAMA_BASE_URL") ?? "http://localhost:11434/v1/"),

            // OpenRouter uses model IDs such as provider/model and requires OPENROUTER_API_KEY.
            "openrouter" => OpenAiCompatibleAgent(
                requestedModel ?? throw new ArgumentException("OpenRouter requires a model argument."),
                RequiredEnvironmentVariable("OPENROUTER_API_KEY"),
                Environment.GetEnvironmentVariable("OPENROUTER_BASE_URL") ?? "https://openrouter.ai/api/v1/"),

            _ => throw new ArgumentException(
                $"Unknown provider '{provider}'. Use openai, azure, anthropic, kimi, llama, or openrouter.")
        };
        return agent;
    }

    private static AIAgent OpenAiCompatibleAgent(string model, string apiKey, string endpoint)
    {
        OpenAIClient client = new(
            new ApiKeyCredential(apiKey),
            new OpenAIClientOptions { Endpoint = new Uri(endpoint) });

        return client.GetChatClient(model).AsAIAgent(instructions: null);
    }

    private static string RequiredEnvironmentVariable(string name)
    {
        string? value = Environment.GetEnvironmentVariable(name);
        return !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new InvalidOperationException($"{name} is not set");
    }
}