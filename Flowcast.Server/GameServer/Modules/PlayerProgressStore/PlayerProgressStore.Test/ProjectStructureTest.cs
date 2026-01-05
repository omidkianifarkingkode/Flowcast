using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace PlayerProgressStore.Test;

public sealed class ProjectStructureTest
{
    private const string ApplicationNameSpace = "Application";
    private const string InfrastructureNameSpace = "Infrastructure";
    private const string PresentationNameSpace = "Presentation";
    private const string DomainNameSpace = "Domain";

    #region PPS_Domain_Test

    [Fact]
    public void PPS_Domain_Should_Not_Depend_On_Upper_Layers()
    {
        var assembly = typeof(Domain.AssemblyReference).Assembly;

        var forbiddenNameSpace = new[]
        {
            $".{ApplicationNameSpace}",
            $".{InfrastructureNameSpace}",
            $".{PresentationNameSpace}"
        };
        
        var testResult = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenNameSpace)
            .GetResult();

        testResult
            .IsSuccessful
            .Should()
            .BeTrue();
    }

    #endregion
    
    #region PPS_Application_Test

    [Fact]
    public void PPS_Application_Should_Not_Depend_On_Upper_Layers()
    {
        var assembly = typeof(Application.AssemblyReference).Assembly;

        var forbiddenNameSpace = new[]
        {
            $".{InfrastructureNameSpace}",
            $".{PresentationNameSpace}"
        };
        
        var testResult = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenNameSpace)
            .GetResult();

        testResult
            .IsSuccessful
            .Should()
            .BeTrue();
    }

    #endregion
    
    #region PPS_Infrastructure_Test

    [Fact]
    public void PPS_Infrastructure_Should_Not_Depend_On_Upper_Layers()
    {
        var assembly = typeof(Infrastructure.AssemblyReference).Assembly;

        var forbiddenNameSpace = new[]
        {
            $".{PresentationNameSpace}"
        };
        
        var testResult = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenNameSpace)
            .GetResult();

        testResult
            .IsSuccessful
            .Should()
            .BeTrue();
    }

    #endregion
    
    #region PPS_Infrastructure_Test

    [Fact]
    public void PPS_Presentation_Should_Not_Depend_On_Upper_Layers()
    {
        var assembly = typeof(Presentation.AssemblyReference).Assembly;

        var forbiddenNameSpace = new[]
        {
            $".{InfrastructureNameSpace}",
            $".{PresentationNameSpace}"
        };
        
        var testResult = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenNameSpace)
            .GetResult();

        testResult
            .IsSuccessful
            .Should()
            .BeTrue();
    }

    #endregion
    
}