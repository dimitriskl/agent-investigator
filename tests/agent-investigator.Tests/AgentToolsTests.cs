using Microsoft.Extensions.AI;

using System.Text.Json;

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
        AgentTools agentTools = new AgentTools(logFolder);
        
        //Act
        var listOfTools = agentTools.Tools;
         
        //Assert
        Assert.Contains("list_log_files", listOfTools.Select(x=>x.Name));
    }

    [Fact]
    public async Task AgentTools_ToolListLogFiles_HasNoValidDirectory()
    {
        //Arrange
        string notExistingLogFolder = Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid().ToString());
        AgentTools agentTools = new AgentTools(notExistingLogFolder);
        AIFunction listLogFilesTool = (AIFunction)agentTools.Tools.Single(x => x.Name == "list_log_files");
        
        //Act
        var toolResult = await listLogFilesTool.InvokeAsync();

        //Assert
        JsonElement resultJson = Assert.IsType<JsonElement>(toolResult);
        
        bool isSuccess = resultJson.GetProperty("isSuccess").GetBoolean();
        Assert.False(isSuccess);
        
        string message = resultJson.GetProperty("message").GetString();
        Assert.Contains(notExistingLogFolder, message);
    }
}
