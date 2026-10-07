namespace agent_investigator.Tests;

public class LogToolTests : IDisposable
{
    private readonly string logFolder = Path.Combine(
        Path.GetTempPath(), $"LogToolTests-{Guid.NewGuid():N}");

    private readonly byte[] png = Convert.FromBase64String(
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
        LogTools logTools = new(new List<string> { logFolder });

        //Act
        LogToolsResult result = logTools.ListLogFiles();

        //Assert
        Assert.True(result.IsSuccess);
        LogFolderFiles folder = Assert.Single(result.Folders);
        Assert.Equal(logFolder, folder.Folder);
        Assert.Equal(new[] { "log.log", "log.md", "log.txt" },
            folder.Files.OrderBy(name => name, StringComparer.Ordinal).ToArray());
    }

    [Fact]
    public void ListLogFiles_FolderDoesNotExist_ReturnsErrorMessage()
    {
        //Arrange
        string notExistingLogFolder = Path.Combine("./", Guid.NewGuid().ToString());
        LogTools logTools = new(new List<string> { notExistingLogFolder });

        //Act
        LogToolsResult result = logTools.ListLogFiles();

        //Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void ListLogFiles_FolderDoesNotExist_ReturnsAMessageWithTheFolder()
    {
        //Arrange
        string folderDosentExist = Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid().ToString());
        LogTools logTools = new(new List<string> { folderDosentExist });

        //Act
        LogToolsResult result = logTools.ListLogFiles();

        //Assert
        Assert.Contains(folderDosentExist, result.Message);
        Assert.Contains("Please fix the log path in the settings and restart", result.Message);
    }

    [Fact]
    public void ListLogFiles_TwoFoldersWithSameFileName_GroupsFilesByFolder()
    {
        //Arrange
        string firstFolder = Path.Combine(logFolder, "first");
        string secondFolder = Path.Combine(logFolder, "second");
        Directory.CreateDirectory(firstFolder);
        Directory.CreateDirectory(secondFolder);
        File.WriteAllText(Path.Combine(firstFolder, "app.log"), "first app log");
        File.WriteAllText(Path.Combine(secondFolder, "app.log"), "second app log");
        LogTools logTools = new(new List<string> { firstFolder, secondFolder });

        //Act
        LogToolsResult result = logTools.ListLogFiles();

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Collection(result.Folders,
            folder =>
            {
                Assert.Equal(firstFolder, folder.Folder);
                Assert.Equal(new[] { "app.log" }, folder.Files);
            },
            folder =>
            {
                Assert.Equal(secondFolder, folder.Folder);
                Assert.Equal(new[] { "app.log" }, folder.Files);
            });
    }

    [Fact]
    public void SearchLogs_WithoutListingFilesFirst_FindsMatchingLine()
    {
        //Arrange
        File.WriteAllText(Path.Combine(logFolder, "app.log"), "INFO started\nERROR Timeout for order 1002");
        LogTools logTools = new(new List<string> { logFolder });

        //Act
        string[] lines = logTools.SearchLogs("timeout");

        //Assert
        Assert.Equal(new[] { $"{Path.Combine(logFolder, "app.log")}:2: ERROR Timeout for order 1002" }, lines);
    }
}