namespace agent_investigator;

/// <summary>
/// Result for log tools 
/// </summary>
public class LogToolsResult
{
    /// <summary>
    /// The file paths 
    /// </summary>
    public string[] LogFilePaths { get; set; } = Array.Empty<string>();
    
    /// <summary>
    /// Message of the operation of reading the folder paths
    /// </summary>
    public string Message { get; set; } =  string.Empty;
    
    /// <summary>
    /// Success flag of the operations
    /// </summary>
    public bool IsSuccess { get; set; } =  false;
}