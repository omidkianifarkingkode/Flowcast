namespace GameServer.Modules.DI.Tests;

public class DIStructureTests
{
    private const string ApplicationNameSpace = "Application";
    private const string ContractsNameSpace = "Contracts";
    private const string DomainNameSpace = "Domain";
    private const string InfrastructureNameSpace = "Infrastructure";
    private const string PresentationNameSpace = "Presentation";
    
    [Fact]
    public void Domain_Should_Not_HaveDependency_On_Application()
    {
        
    }
}