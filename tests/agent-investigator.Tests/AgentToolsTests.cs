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
        AgentTools agentTools = new AgentTools(logFolder);
        
        //Act
        var listOfTools = agentTools.Tools;
         
        //Assert
        Assert.Contains("list_log_files", listOfTools.Select(x=>x.Name));
    }
}