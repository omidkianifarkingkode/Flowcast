using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace PlayerProgressStore.Test;

public sealed class ProjectStructureTest
{
    #region PlayerProgressStore_Domain_Test

    [Fact]
    public void PlayerProgressStore_Domain_Should_Not_Depend_On_Upper_Layers()
    {
        var assembly = typeof(PlayerProgressStore.Domain.AssemblyReference).Assembly;

        var forbiddenNamespaces = new[]
        {
            "PlayerProgressStore.Application",
            "PlayerProgressStore.Infrastructure",
            "PlayerProgressStore.Presentation"
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

    #region PlayerProgressStore_Application_Test

    [Fact]
    public void PlayerProgressStore_Application_Should_Not_Depend_On_Upper_Layers()
    {
        var assembly = typeof(PlayerProgressStore.Application.AssemblyReference).Assembly;

        var forbiddenNamespaces = new[]
        {
            "PlayerProgressStore.Infrastructure",
            "PlayerProgressStore.Presentation"
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

    #region PlayerProgressStore_Infrastructure_Test

    [Fact]
    public void PlayerProgressStore_Infrastructure_Should_Not_Depend_On_Upper_Layers()
    {
        var assembly = typeof(PlayerProgressStore.Infrastructure.AssemblyReference).Assembly;

        var forbiddenNamespaces = new[]
        {
            "PlayerProgressStore.Presentation"
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

    #region PlayerProgressStore_Presentation_Test

    [Fact]
    public void PlayerProgressStore_Presentation_Should_Not_Depend_On_Domain_Directly()
    {
        var assembly = typeof(PlayerProgressStore.Presentation.AssemblyReference).Assembly;

        var forbiddenNamespaces = new[]
        {
            "PlayerProgressStore.Domain"
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