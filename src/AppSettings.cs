namespace agent_investigator;

public class AppSettings
{
    /// <summary>
    ///     Extensions used when settings.json does not define any. Returns a new list on every call,
    ///     so nobody can change the defaults by accident.
    /// </summary>
    public static List<string> DefaultLogExtensions => [".log", ".md", ".txt"];

    public List<string> LogPaths { get; set; } = new();

    public List<string> LogExtensions { get; set; } = DefaultLogExtensions;
}
