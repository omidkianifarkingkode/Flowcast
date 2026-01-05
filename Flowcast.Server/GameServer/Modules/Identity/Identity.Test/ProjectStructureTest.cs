using FluentAssertions;
using NetArchTest.Rules;
using static NetArchTest.Rules.Types;
using Xunit;
using Domain = Identity.Domain;

namespace Identity.Test;

public sealed class ProjectStructureTest
{
    private const string ApplicationNameSpace = "Application";
    private const string ContractsNameSpace = "Contracts";
    private const string InfrastructureNameSpace = "Infrastructure";
    private const string PresentationNameSpace = "Presentation";

    [Fact]
    public void Identity_Domain_Should_Not_Depend_On_Upper_Layers()
    {
        var assembly = typeof(Domain.AssemblyReference).Assembly;

        var forbiddenNamespaces = new[]
        {
            $".{ApplicationNameSpace}",
            $".{ContractsNameSpace}",
            $".{InfrastructureNameSpace}",
            $".{PresentationNameSpace}"
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
}
