using System.ComponentModel;

namespace agent_investigator
{
    /// <summary>
    /// Log tools class
    /// </summary>
    /// <param name="logFolder"></param>
    public class LogTools(string logFolder)
    {
        private static readonly string[] LogExtensions = [".log", ".md", ".txt"];
        
        private IEnumerable<string> LogFilePaths() => Directory.EnumerateFiles(logFolder, "*.*", SearchOption.AllDirectories)
            .Where(x=> LogExtensions.Contains(Path.GetExtension(x), StringComparer.OrdinalIgnoreCase));
        
        /// <summary>
        /// List of folder's file tool
        /// </summary>
        /// <returns></returns>
        public LogToolsResult ListLogFiles()
        {
            string message = string.Empty;
            bool isSuccess = false;
            Console.WriteLine("(ListLogFiles was called)");

            string[] logFilePaths = new string[] { };
            
            try
            {
                logFilePaths = LogFilePaths()
                    .Select(x=> Path.GetRelativePath(logFolder, x))
                    .ToArray();
                
                message = $"(ListLogFiles was called for {logFolder} with success.')";
                isSuccess = true;
            }
            catch (IOException e)
            {
                message = $"(Error: '{e.Message}')";
            }   
            
            LogToolsResult result = new LogToolsResult
            {
                LogFilePaths = logFilePaths,
                Message = message,
                Success = isSuccess
            };

            return result;
        }

        /// <summary>
        /// Search the  logs for data
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
            foreach (string path in LogFilePaths())
            {
                int lineNumber = 0;
                foreach (string line in File.ReadLines(path))
                {
                    lineNumber++;
                    if (line.Contains(text, StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add($"{Path.GetRelativePath(logFolder, path)}:{lineNumber}: {line}");
                        if (results.Count == 50)
                        {
                            return [.. results];
                        }
                    }
                }
            }

            return [.. results];
        }
    }
}