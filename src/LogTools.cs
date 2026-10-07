using System.ComponentModel;

namespace agent_investigator;

/// <summary>
///     Log tools class
/// </summary>
/// <param name="logFolders"></param>
public class LogTools(List<string> logFolders)
{
    private static readonly string[] LogExtensions = [".log", ".md", ".txt"];

    /// <summary>
    ///     List of folder's file tool
    /// </summary>
    /// <returns></returns>
    public LogToolsResult ListLogFiles()
    {
        Console.WriteLine("(ListLogFiles was called)");

        List<LogFolderFiles> folders = [];
        List<string> errors = [];

        foreach (string logFolder in logFolders)
        {
            try
            {
                if (Directory.Exists(logFolder))
                {
                    folders.Add(new LogFolderFiles
                    {
                        Folder = logFolder,
                        Files = LogFilesIn(logFolder).Select(x => Path.GetRelativePath(logFolder, x)).ToArray()
                    });
                }
                else
                {
                    errors.Add($"{logFolder} does not exist.  Please fix the log path in the settings and restart.");
                }
            }
            catch (IOException e)
            {
                errors.Add($"Error: '{e.Message}', Folder: '{logFolder}'.");
            }
        }

        return new LogToolsResult
        {
            Folders = folders,
            Message = errors.Count == 0
                ? $"Listed {folders.Sum(x => x.Files.Length)} log files from {folders.Count} folder(s)."
                : string.Join(" ", errors),
            IsSuccess = errors.Count == 0
        };
    }

    private static IEnumerable<string> LogFilesIn(string logFolder) =>
        Directory.EnumerateFiles(logFolder, "*.*", SearchOption.AllDirectories)
            .Where(x => LogExtensions.Contains(Path.GetExtension(x), StringComparer.OrdinalIgnoreCase));

    /// <summary>
    ///     Search the  logs for data
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public string[] SearchLogs(
        [Description("Text to search for, e.g. 'timeout' or '1002'. Case-insensitive.")]
        string text
    )
    {
        Console.WriteLine($"(SearchLogs was called with '{text}')");

        List<string> results = [];
        foreach (string logFolder in logFolders.Where(Directory.Exists))
        {
            foreach (string path in LogFilesIn(logFolder))
            {
                int lineNumber = 0;
                foreach (string line in File.ReadLines(path))
                {
                    lineNumber++;
                    if (line.Contains(text, StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add($"{path}:{lineNumber}: {line}");
                        if (results.Count == 50)
                        {
                            return [.. results];
                        }
                    }
                }
            }
        }

        return [.. results];
    }
}