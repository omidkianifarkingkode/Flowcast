using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace Identity.Test;

public sealed class ProjectStructureTest
{
    #region Identity_Domain_Test

    [Fact]
    public void Identity_Domain_Should_Not_Depend_On_Upper_Layers()
    {
        var assembly = typeof(Identity.Domain.AssemblyReference).Assembly;

        var forbiddenNamespaces = new[]
        {
            "Identity.Application",
            "Identity.Infrastructure",
            "Identity.Presentation"
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
        var assembly = typeof(Identity.Application.AssemblyReference).Assembly;

        var forbiddenNamespaces = new[]
        {
            "Identity.Infrastructure",
            "Identity.Presentation"
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
        var assembly = typeof(Identity.Infrastructure.AssemblyReference).Assembly;

        var forbiddenNamespaces = new[]
        {
            "Identity.Presentation"
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
    public void Identity_Presentation_Should_Not_Depend_On_Domain_Directly()
    {
        var assembly = typeof(Identity.Presentation.AssemblyReference).Assembly;

        var forbiddenNamespaces = new[]
        {
            "Identity.Domain"
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