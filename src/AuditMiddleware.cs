using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace agent_investigator;

/// <summary>
///     Audits the LLM calls
/// </summary>
public class AuditMiddleware
{
    /// <summary>
    ///     Logs any tool call
    /// </summary>
    /// <param name="agent"></param>
    /// <param name="invocationContext"></param>
    /// <param name="next"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<object?> LogToolCall(
        AIAgent agent,
        FunctionInvocationContext context,
        Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next,
        CancellationToken cancellationToken
    )
    {
        Console.WriteLine($"Called tool name: {context.Function.Name}");
        string? withArgumentsString =
            $"With arguments: {string.Join(", ", context.Arguments.Select(x => $"{x.Key}={x.Value}"))}";
        Console.WriteLine(
            withArgumentsString);

        object? toolCall;
        try
        {
            toolCall = await next(context, cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(
                $"Failure: Calling tool {context.Function.Name} with arguments {withArgumentsString} produced the error {e.Message}");
            throw;
        }

        Console.WriteLine($"Result : {toolCall}");

        return toolCall;
    }
}