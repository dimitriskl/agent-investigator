namespace agent_investigator.Tests;

public class SettingsStoreTests
{
    [Fact]
    public void CreateOrLoadSettings_FileDoesNotExist_CreatesFileWithEmptyLogsPath()
    {
        //Arrange
        string tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        SettingsStore settingsStore = new SettingsStore(tempFolder);
        
        //Act
        SettingsStoreResult settingsStoreResult = settingsStore.CreateOrLoadSettings();
        
        //Assert
        Assert.True(settingsStoreResult.IsSuccess);
        Assert.Equal(string.Empty, settingsStoreResult.LogPath);
        Assert.True(File.Exists(Path.Combine(tempFolder, "settings.json")));
    }
}