namespace agent_investigator.Tests;

public class SettingsStoreTests
{
    [Fact]
    public void CreateOrLoadSettings_FileDoesNotExist_CreatesFileWithEmptyLogsPath()
    {
        //Arrange
        string tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        SettingsStore settingsStore = new(tempFolder);

        //Act
        SettingsStoreResult settingsStoreResult = settingsStore.CreateOrLoadSettings();

        //Assert
        Assert.True(settingsStoreResult.IsSuccess);
        Assert.Empty(settingsStoreResult.LogPaths);
        Assert.True(File.Exists(Path.Combine(tempFolder, "settings.json")));
    }

    [Fact]
    public void CreateOrLoadSettings_FileExist_ReturnsWithLogsPath()
    {
        //Arrange
        string tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        SettingsStore settingsStore = new(tempFolder);
        string logPath = Path.Combine(Path.GetTempPath(), "logs.txt");

        //Act
        SettingsStoreResult settingsStoreResult = settingsStore.AddLogPath(logPath);

        //Assert
        Assert.True(settingsStoreResult.LogPaths.Any());
        Assert.Equal(logPath, settingsStoreResult.LogPaths.FirstOrDefault(x => x == logPath));
    }

    [Fact]
    public void AddLogPath_PathAdded_IsSavedToSettingsFile()
    {
        //Arrange
        string tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        string logPath = Path.Combine(tempFolder, "logs");
        new SettingsStore(tempFolder).AddLogPath(logPath);

        //Act
        SettingsStoreResult reloaded = new SettingsStore(tempFolder).CreateOrLoadSettings();

        //Assert
        Assert.Contains(logPath, reloaded.LogPaths);
    }
}