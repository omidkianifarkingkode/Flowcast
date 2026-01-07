using FluentAssertions;
using NetArchTest.Rules;
using System.Reflection;

namespace Shared.Test;

public abstract class ProjectStructureTest
{
    protected abstract string ModuleName { get; }

    protected string Domain => $"{ModuleName}.Domain";
    protected string Application => $"{ModuleName}.Application";
    protected string Infrastructure => $"{ModuleName}.Infrastructure";
    protected string Presentation => $"{ModuleName}.Presentation";

    protected abstract Assembly DomainAssembly { get; }
    protected abstract Assembly ApplicationAssembly { get; }
    protected abstract Assembly InfrastructureAssembly { get; }
    protected abstract Assembly PresentationAssembly { get; }
    protected abstract Assembly ContractsAssembly { get; }

    #region Identity_Domain_Test

    [Fact]
    public void Identity_Domain_Should_Not_Depend_On_Upper_Layers()
    {
        var assembly = DomainAssembly;

        var forbiddenNamespaces = new[]
        {
            Application,
            Infrastructure,
            Presentation
        };

        var testResult = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenNamespaces)
            .GetResult();

        testResult
            .IsSuccessful
            .Should()
            .BeTrue();
    }

    #endregion

    #region Identity_Application_Test

    [Fact]
    public void Identity_Application_Should_Not_Depend_On_Upper_Layers()
    {
        var assembly = ApplicationAssembly;

        var forbiddenNamespaces = new[]
        {
            Infrastructure,
            Presentation
        };

        var testResult = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenNamespaces)
            .GetResult();

        testResult
            .IsSuccessful
            .Should()
            .BeTrue();
    }

    #endregion

    #region Identity_Infrastructure_Test

    [Fact]
    public void Identity_Infrastructure_Should_Not_Depend_On_Upper_Layers()
    {
        var assembly = InfrastructureAssembly;

        var forbiddenNamespaces = new[]
        {
            Presentation
        };

        var testResult = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenNamespaces)
            .GetResult();

        testResult
            .IsSuccessful
            .Should()
            .BeTrue();
    }

    #endregion

    #region Identity_Presentation_Test

    [Fact]
    public void Identity_Contracts_Should_Not_Depend_On_Other()
    {
        var assembly = ContractsAssembly;

        var forbiddenNamespaces = new[]
        {
            Domain,
            Application,
            Presentation,
            Infrastructure
        };

        var testResult = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenNamespaces)
            .GetResult();

        testResult
            .IsSuccessful
            .Should()
            .BeTrue();
    }

    #endregion
}