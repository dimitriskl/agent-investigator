namespace agent_investigator;

/// <summary>
///     Result of the settings store
/// </summary>
public class SettingsStoreResult
{
    public bool IsSuccess { get; set; }
    public List<string> LogPaths { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}