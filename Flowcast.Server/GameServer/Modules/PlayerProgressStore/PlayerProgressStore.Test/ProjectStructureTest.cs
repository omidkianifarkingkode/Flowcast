using System.Reflection;

namespace PlayerProgressStore.Test;

public class ProjectStructureTest : Shared.Test.ProjectStructureTest
{
    protected override string ModuleName => "PlayerProgressStore";

    protected override Assembly DomainAssembly => typeof(Domain.AssemblyReference).Assembly;
    protected override Assembly ApplicationAssembly => typeof(Application.AssemblyReference).Assembly;
    protected override Assembly InfrastructureAssembly => typeof(Infrastructure.AssemblyReference).Assembly;
    protected override Assembly PresentationAssembly => typeof(Presentation.AssemblyReference).Assembly;
    protected override Assembly ContractsAssembly => typeof(Contracts.AssemblyReference).Assembly;
}