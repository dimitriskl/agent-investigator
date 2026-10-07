namespace agent_investigator;

/// <summary>
/// Result for log tools 
/// </summary>
public class LogToolsResult
{
    /// <summary>
    /// The log files, grouped by the folder they were found in
    /// </summary>
    public List<LogFolderFiles> Folders { get; set; } = new();
    
    /// <summary>
    /// Message of the operation of reading the folder paths
    /// </summary>
    public string Message { get; set; } =  string.Empty;
    
    /// <summary>
    /// Success flag of the operations
    /// </summary>
    public bool IsSuccess { get; set; } =  false;
}