namespace Shared.Presentation.ApiGuard
{
    internal sealed record AllowOnlyLaunchModes(params string[] Modes);
    internal sealed record BlockLaunchModes(params string[] Modes);
}
