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
        Assert.Equal(string.Empty, settingsStoreResult.LogPath);
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
        SettingsStoreResult settingsStoreResult = settingsStore.UpdateLogPath(logPath);

        //Assert
        Assert.True(!string.IsNullOrWhiteSpace(settingsStoreResult.LogPath));
        Assert.Equal(logPath, settingsStoreResult.LogPath);
    }
}