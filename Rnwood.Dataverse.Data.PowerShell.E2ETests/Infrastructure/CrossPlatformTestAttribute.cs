using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure
{
    /// <summary>
    /// Marks E2E tests that should run on all platforms (Windows, Linux, macOS).
    /// Tests without this attribute will only run on Windows in CI.
    /// </summary>
    [TraitDiscoverer("Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure.CrossPlatformTestDiscoverer", "Rnwood.Dataverse.Data.PowerShell.E2ETests")]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class CrossPlatformTestAttribute : Attribute, ITraitAttribute
    {
    }

    public class CrossPlatformTestDiscoverer : ITraitDiscoverer
    {
        public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
        {
            yield return new KeyValuePair<string, string>("Category", "CrossPlatform");
        }
    }
}
