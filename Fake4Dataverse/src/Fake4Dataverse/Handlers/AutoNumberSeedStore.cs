using System.Collections.Concurrent;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Shared in-memory store for auto-number seed values used by the auto-number request handlers.
    /// </summary>
    internal static class AutoNumberSeedStore
    {
        internal static readonly ConcurrentDictionary<string, long> Seeds = new ConcurrentDictionary<string, long>();

        internal const long DefaultSeed = 1000L;

        internal static string MakeKey(string entityName, string attributeName) =>
            entityName + ":" + attributeName;

        internal static long GetOrDefault(string entityName, string attributeName) =>
            Seeds.GetOrAdd(MakeKey(entityName, attributeName), DefaultSeed);

        internal static void Set(string entityName, string attributeName, long value) =>
            Seeds[MakeKey(entityName, attributeName)] = value;

        internal static long IncrementAndGet(string entityName, string attributeName)
        {
            var key = MakeKey(entityName, attributeName);
            return Seeds.AddOrUpdate(key, DefaultSeed + 1, (_, current) => current + 1);
        }
    }
}
