using NetArchTest.Rules;
using System.Reflection;

namespace GameServer.Modules.DI.Tests;

public sealed class DiStructureTests
{
    #region Domain

    public static IEnumerable<object[]> DomainAssemblies()
    {
        yield return [typeof(Identity.Domain.Entities.SigningKey).Assembly];
        yield return [typeof(Matchmaking.Domain.Match).Assembly];
        yield return [typeof(Session.Domain.SessionEntity).Assembly];
        yield return [typeof(PlayerProgressStore.Domain.PlayerNamespace).Assembly];
    }

    [Theory]
    [MemberData(nameof(DomainAssemblies))]
    public void Domain_Should_Not_Depend_On_Upper_Layers(Assembly domainAssembly)
    {
        AssertHasNoDependencies(domainAssembly, new[] { ".Application", ".Contracts", ".Infrastructure", ".Presentation" });
    }

    #endregion

    #region Application

    public static IEnumerable<object[]> ApplicationAssemblies()
    {
        yield return [typeof(Identity.Application.DependencyInjection).Assembly];
        yield return [typeof(MatchMaking.Application.Commands.FindMatchCommand).Assembly];
        yield return [typeof(Session.Application.Commands.CreateSessionCommand).Assembly];
        yield return [typeof(PlayerProgressStore.Application.DependencyInjection).Assembly];
    }

    [Theory]
    [MemberData(nameof(ApplicationAssemblies))]
    public void Application_Should_Not_Depend_On_Upper_Layers(Assembly applicationAssembly)
    {
        AssertHasNoDependencies(applicationAssembly, new[] { ".Infrastructure", ".Presentation" });
    }

    #endregion

    #region Infrastructure

    public static IEnumerable<object[]> InfrastructureAssemblies()
    {
        yield return [typeof(Identity.Infrastructure.DependencyInjection).Assembly];
        yield return [typeof(Matchmaking.Infrastructure.InMemoryTicketRepository).Assembly];
        yield return [typeof(Session.Infrastructure.DependencyInjection).Assembly];
        yield return [typeof(PlayerProgressStore.Infrastructure.DependencyInjection).Assembly];
    }

    [Theory]
    [MemberData(nameof(InfrastructureAssemblies))]
    public void Infrastructure_Should_Not_Depend_On_Upper_Layers(Assembly infrastructureAssembly)
    {
        AssertHasNoDependencies(infrastructureAssembly, new[] { ".Presentation" });
    }

    #endregion

    #region Presentation

    public static IEnumerable<object[]> PresentationAssemblies()
    {
        yield return [typeof(Identity.Presentation.DependencyInjection).Assembly];
        yield return [typeof(Session.Presentation.Endpoints.V1.CreateEndpoint).Assembly];
        yield return [typeof(PlayerProgressStore.Presentation.DependencyInjection).Assembly];
    }

    [Theory]
    [MemberData(nameof(PresentationAssemblies))]
    public void Presentation_Should_Not_Depend_On_Domain(Assembly presentationAssembly)
    {
        AssertHasNoDependencies(presentationAssembly, new[] { ".Domain" });
    }

    #endregion

    #region Helpers

    private static void AssertHasNoDependencies(Assembly assembly, string[] forbiddenNamespaces)
    {
        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenNamespaces)
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    #endregion
}