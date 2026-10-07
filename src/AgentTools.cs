using Microsoft.Extensions.AI;

namespace agent_investigator;

public class AgentTools
{
    public AgentTools(List<string> logFolders)
    {
        LogTools logtools = new(logFolders);

        Tools =
        [
            AIFunctionFactory.Create(logtools.ListLogFiles, "list_log_files",
                "Lists the log files available for investigation"),
            AIFunctionFactory.Create(logtools.SearchLogs, "search_logs",
                "Searches all log files containing a text. Returns 'file:line: content'")
        ];
    }

    public List<AITool> Tools { get; }
}