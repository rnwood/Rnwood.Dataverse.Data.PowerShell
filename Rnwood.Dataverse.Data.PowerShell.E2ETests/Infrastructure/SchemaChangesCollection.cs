using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure
{
    /// <summary>
    /// Defines a test collection for tests that must run sequentially because they:
    /// - Modify Dataverse schema (create/update/delete tables, columns, optionsets, relationships, entity keys)
    /// - Execute Publish customizations operations
    /// - Modify shared singleton state (e.g. organisation settings, shared option sets)
    ///
    /// Tests in this collection run one at a time to avoid CustomizationLockException retries
    /// and shared-state conflicts. This collection itself still runs in parallel alongside other
    /// test collections (read-only and data-only tests) thanks to parallelizeTestCollections=true.
    ///
    /// Tests that only create isolated data records (using unique GUIDs) or are read-only
    /// should NOT be placed in this collection - leave them with their default implicit
    /// collection so they continue to run in parallel.
    /// </summary>
    [CollectionDefinition(Name)]
    public class SchemaChangesCollection
    {
        public const string Name = "SchemaAndPublishChanges";
    }
}
