// See https://aka.ms/new-console-template for more information

using OpenAI;
using Microsoft.Agents.AI;
using OpenAI.Chat;

Console.WriteLine("Hello, World!");

string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

OpenAIClient client = new OpenAIClient(apiKey);

ChatClientAgent agent = client.GetChatClient("gpt-5-nano").AsAIAgent();

AgentResponse response = await agent.RunAsync("What is the capital of France?");

Console.WriteLine(response);

