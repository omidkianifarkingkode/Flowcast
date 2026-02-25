using System.Reflection;

namespace Identity.Test;

public class ProjectStructureTest : Shared.Test.ProjectStructureTest
{
    protected override string ModuleName => "Identity";

    protected override Assembly DomainAssembly => typeof(Identity.Domain.AssemblyReference).Assembly;
    protected override Assembly ApplicationAssembly => typeof(Identity.Application.AssemblyReference).Assembly;
    protected override Assembly InfrastructureAssembly => typeof(Identity.Infrastructure.AssemblyReference).Assembly;
    protected override Assembly PresentationAssembly => typeof(Identity.Presentation.AssemblyReference).Assembly;
    protected override Assembly ContractsAssembly => typeof(Identity.Contracts.AssemblyReference).Assembly;
}