using System.Text.Json;
using Microsoft.Extensions.AI;

namespace agent_investigator.Tests;

public class AgentToolsTests : IDisposable
{
    private readonly string logFolder = Path.Combine(
        Path.GetTempPath(), $"LogToolTests-{Guid.NewGuid():N}");

    public AgentToolsTests()
    {
        Directory.CreateDirectory(logFolder);
    }

    public void Dispose()
    {
        Directory.Delete(logFolder, true);
    }

    [Fact]
    public void AgentTools_LoadsAllTools_AndGetListLogFileTool()
    {
        //Arrange
        AgentTools agentTools = new(new List<string> { logFolder }, AppSettings.DefaultLogExtensions);

        //Act
        List<AITool> listOfTools = agentTools.Tools;

        //Assert
        Assert.Contains("list_log_files", listOfTools.Select(x => x.Name));
    }

    [Fact]
    public async Task AgentTools_ToolListLogFiles_HasNoValidDirectory()
    {
        //Arrange
        string notExistingLogFolder = Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid().ToString());
        AgentTools agentTools = new(new List<string> { notExistingLogFolder }, AppSettings.DefaultLogExtensions);
        AIFunction listLogFilesTool = (AIFunction)agentTools.Tools.Single(x => x.Name == "list_log_files");

        //Act
        object? toolResult = await listLogFilesTool.InvokeAsync();

        //Assert
        JsonElement resultJson = Assert.IsType<JsonElement>(toolResult);

        bool isSuccess = resultJson.GetProperty("isSuccess").GetBoolean();
        Assert.False(isSuccess);

        string message = resultJson.GetProperty("message").GetString() ?? string.Empty;
        Assert.Contains(notExistingLogFolder, message);
    }
}