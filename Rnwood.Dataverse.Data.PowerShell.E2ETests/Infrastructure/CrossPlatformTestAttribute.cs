using System;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure
{
    /// <summary>
    /// Marks E2E tests that should run on all platforms (Windows, Linux, macOS).
    /// Tests without this attribute will only run on Windows in CI.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class CrossPlatformTestAttribute : TraitAttribute
    {
        public CrossPlatformTestAttribute() : base("Category", "CrossPlatform")
        {
        }
    }
}
