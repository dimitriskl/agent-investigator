namespace agent_investigator;

/// <summary>
/// The log files found in one log folder, relative to that folder
/// </summary>
public class LogFolderFiles
{
    /// <summary>
    /// The log folder, as configured in the settings
    /// </summary>
    public string Folder { get; set; } = string.Empty;

    /// <summary>
    /// The log files, relative to <see cref="Folder"/>
    /// </summary>
    public string[] Files { get; set; } = Array.Empty<string>();
}
