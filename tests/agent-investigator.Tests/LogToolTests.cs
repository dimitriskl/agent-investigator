namespace agent_investigator.Tests;

public class LogToolTests : IDisposable
{
    private readonly string logFolder = Path.Combine(
        Path.GetTempPath(), $"LogToolTests-{Guid.NewGuid():N}");
        byte[] png = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+jRZkAAAAASUVORK5CYII=");

    
    public LogToolTests()
    {
        //create the temp folder
        Directory.CreateDirectory(logFolder);
        File.WriteAllText(Path.Combine(logFolder, "log.txt"), "text log");
        File.WriteAllText(Path.Combine(logFolder, "log.md"), "md log");
        File.WriteAllText(Path.Combine(logFolder, "log.log"), "log log");
        File.WriteAllBytes(Path.Combine(logFolder, "image.png"), png);
    }

    public void Dispose()
    {
        //delete the temp folder
        Directory.Delete(logFolder, true);
    }

    
    [Fact]
    public void ListLogFiles_FolderWithLogsAndOtherFiles_ReturnsLogsOnly()
    {
        //Arrange
        LogTools logTools = new LogTools(logFolder);
        
        //Act
        LogToolsResult result = logTools.ListLogFiles();
        
        //Assert
        Assert.True(result.Success);
        Assert.Equal(new []{"log.log", "log.md", "log.txt"}, result.LogFilePaths.OrderBy(name => name, StringComparer.Ordinal).ToArray());
    }

    [Fact]
    public void ListLogFiles_FolderDoesNotExist_ReturnsErrorMessage()
    {
        //Arrange
        string notExistingLogFolder = Path.Combine("./", Guid.NewGuid().ToString());
        LogTools logTools = new LogTools(notExistingLogFolder);
        
        //Act
        LogToolsResult result = logTools.ListLogFiles();
        
        //Assert
        Assert.False(result.Success);
        
    }
    
    [Fact]
    public void ListLogFiles_FolderDoesNotExist_ReturnsAMessageWithTheFolder()
    {
        //Arrange
        string notExistingLogFolder = Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid().ToString());
        LogTools logtools = new LogTools(notExistingLogFolder);
        
        //Act
        LogToolsResult  result = logtools.ListLogFiles();

        //Assert
        Assert.Contains(notExistingLogFolder,result.Message);

    }
}
