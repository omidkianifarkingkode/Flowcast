using NetArchTest.Rules;

namespace GameServer.Modules.DI.Tests;

public sealed class DiStructureTests
{
    [Fact]
    public void Identity_Domain_Should_Not_Depend_On_Upper_Layers()
    {
        AssertDomainHasNoUpperLayerDependencies(typeof(Identity.Domain.Entities.SigningKey).Assembly);
    }

    [Fact]
    public void Matchmaking_Domain_Should_Not_Depend_On_Upper_Layers()
    {
        AssertDomainHasNoUpperLayerDependencies(typeof(Matchmaking.Domain.Match).Assembly);
    }

    [Fact]
    public void Session_Domain_Should_Not_Depend_On_Upper_Layers()
    {
        AssertDomainHasNoUpperLayerDependencies(typeof(Session.Domain.SessionEntity).Assembly);
    }

    [Fact]
    public void PlayerProgressStore_Domain_Should_Not_Depend_On_Upper_Layers()
    {
        AssertDomainHasNoUpperLayerDependencies(typeof(PlayerProgressStore.Domain.PlayerNamespace).Assembly);
    }

    private static void AssertDomainHasNoUpperLayerDependencies(System.Reflection.Assembly domainAssembly)
    {
        var forbidden = new[]
        {
            ".Application",
            ".Contracts",
            ".Infrastructure",
            ".Presentation"
        };

        var result = Types.InAssembly(domainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbidden)
            .GetResult();

        Assert.True(result.IsSuccessful);
    }
}