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
        try
        {
            Directory.Delete(logFolder, true);
        }
        catch (IOException exception)
        {
            // A file with the same name and location specified by 'path' exists.
            // -or-
            // The directory specified by 'path' is read-only, or 'recursive' is 'false' and 'path' is not an empty directory.
            // -or-
            // The directory is the application's current working directory.
            // -or-
            // The directory contains a read-only file.
            // -or-
            // The directory is being used by another process.
            Console.WriteLine(exception);
            throw;
        }
    }

    
    [Fact]
    public void ListLogFiles_FolderWithLogsAndOtherFiles_ReturnsLogsOnly()
    {
        //Arrange
        LogTools logTools = new LogTools(logFolder);
        
        //Act
        string[] files = logTools.ListLogFiles();
        
        //Assert
        Assert.Equal(new []{"log.log", "log.md", "log.txt"}, files.OrderBy(name => name, StringComparer.Ordinal).ToArray());
    }
}
