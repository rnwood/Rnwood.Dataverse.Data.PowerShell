using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets
{
    /// <summary>
    /// Collection definition for slow-running owner and status tests.
    /// These tests run in their own collection but allow parallel execution with other collections.
    /// </summary>
    [CollectionDefinition("OwnerAndStatus")]
    public class OwnerAndStatusCollection
    {
    }
}
