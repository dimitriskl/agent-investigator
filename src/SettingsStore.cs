using System.Text.Json;

namespace agent_investigator;

/// <summary>
///     Settings store class for saving and reading the app settings from the filesystem.
/// </summary>
public class SettingsStore
{
    private readonly string _folderPath;
    private readonly string _settingsJson = "settings.json";

    public SettingsStore(string folderPath)
    {
        _folderPath = folderPath;
    }

    /// <summary>
    ///     Create if do not exist or load application settings
    /// </summary>
    /// <returns></returns>
    public SettingsStoreResult CreateOrLoadSettings()
    {
        SettingsStoreResult settingsStoreResult = new();

        try
        {
            string settingsFullPath = Path.Combine(_folderPath, _settingsJson);

            AppSettings? settings = new();

            string contents;
            if (!File.Exists(settingsFullPath))
            {
                Directory.CreateDirectory(_folderPath);

                contents = JsonSerializer.Serialize(settings);
                File.WriteAllText(settingsFullPath, contents);
            }
            else
            {
                contents = File.ReadAllText(settingsFullPath);
                settings = JsonSerializer.Deserialize<AppSettings>(contents);
            }

            settingsStoreResult.IsSuccess = true;
            if (settings != null)
            {
                settingsStoreResult.LogPaths = settings.LogPaths;
                settingsStoreResult.LogExtensions = settings.LogExtensions;
            }
        }
        catch (DirectoryNotFoundException e)
        {
            settingsStoreResult.IsSuccess = false;
            settingsStoreResult.Message = e.Message;
        }
        catch (Exception e)
        {
            settingsStoreResult.IsSuccess = false;
            settingsStoreResult.LogPaths = new List<string>();
            settingsStoreResult.Message = e.Message;
        }

        return settingsStoreResult;
    }

    public SettingsStoreResult AddLogPath(string logPath)
    {
        SettingsStoreResult settings = CreateOrLoadSettings();
        settings.LogPaths.Add(logPath);

        string content = JsonSerializer.Serialize(new AppSettings
        {
            LogPaths = settings.LogPaths,
            LogExtensions = settings.LogExtensions
        });

        File.WriteAllText(Path.Combine(_folderPath, _settingsJson), content);

        return settings;
    }
}