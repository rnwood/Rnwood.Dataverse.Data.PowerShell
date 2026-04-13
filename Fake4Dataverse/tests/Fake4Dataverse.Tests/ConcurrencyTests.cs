using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class ConcurrencyTests
    {
        [Fact]
        public void ParallelCreates_AllSucceed()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            const int count = 500;
            var ids = new Guid[count];

            Parallel.For(0, count, i =>
            {
                ids[i] = service.Create(new Entity("account") { ["name"] = $"Account {i}" });
            });

            Assert.Equal(count, ids.Distinct().Count());
            var all = service.RetrieveMultiple(new QueryExpression("account") { ColumnSet = new ColumnSet(true) });
            Assert.Equal(count, all.Entities.Count);
        }

        [Fact]
        public async Task ParallelReads_WhileWriting_NoExceptions()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            const int seedCount = 100;
            var seedIds = new List<Guid>();
            for (int i = 0; i < seedCount; i++)
                seedIds.Add(service.Create(new Entity("account") { ["name"] = $"Account {i}" }));

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var exceptions = new List<Exception>();

            // Writer thread: continuously create more entities
            var writer = Task.Run(() =>
            {
                int n = seedCount;
                while (!cts.IsCancellationRequested)
                {
                    try { service.Create(new Entity("account") { ["name"] = $"Account {n++}" }); }
                    catch (Exception ex) { lock (exceptions) { exceptions.Add(ex); } }
                }
            });

            // Multiple reader threads: continuously retrieve and query
            var readers = Enumerable.Range(0, 4).Select(_ => Task.Run(() =>
            {
                var rng = new Random();
                while (!cts.IsCancellationRequested)
                {
                    try
                    {
                        var id = seedIds[rng.Next(seedIds.Count)];
                        service.Retrieve("account", id, new ColumnSet(true));
                        service.RetrieveMultiple(new QueryExpression("account")
                        {
                            ColumnSet = new ColumnSet("name"),
                            TopCount = 10
                        });
                    }
                    catch (Exception ex) { lock (exceptions) { exceptions.Add(ex); } }
                }
            })).ToArray();

            await Task.WhenAll(new[] { writer }.Concat(readers).ToArray());
            Assert.Empty(exceptions);
        }

        [Fact]
        public void ParallelUpdates_SameEntity_AllApply()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Original", ["counter"] = 0 });

            const int count = 100;
            var barrier = new Barrier(count);
            Parallel.For(0, count, i =>
            {
                barrier.SignalAndWait();
                service.Update(new Entity("account", id) { [$"field_{i}"] = i });
            });

            var result = service.Retrieve("account", id, new ColumnSet(true));
            for (int i = 0; i < count; i++)
                Assert.Equal(i, result.GetAttributeValue<int>($"field_{i}"));
        }

        [Fact]
        public void ParallelCreateAndDelete_NoDeadlocks()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            const int iterations = 200;
            var created = new System.Collections.Concurrent.ConcurrentBag<Guid>();

            // Create entities in parallel
            Parallel.For(0, iterations, i =>
            {
                var id = service.Create(new Entity("account") { ["name"] = $"Account {i}" });
                created.Add(id);
            });

            // Delete them all in parallel
            Parallel.ForEach(created, id =>
            {
                service.Delete("account", id);
            });

            var remaining = service.RetrieveMultiple(new QueryExpression("account") { ColumnSet = new ColumnSet(true) });
            Assert.Empty(remaining.Entities);
        }

        [Fact]
        public async Task ParallelHandlerRegistration_AndExecution_ThreadSafe()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            const int count = 50;
            var barrier = new Barrier(count + 1);

            // Register handlers from multiple threads while executing
            var registrations = Enumerable.Range(0, count).Select(i => Task.Run(() =>
            {
                barrier.SignalAndWait();
                env.HandlerRegistry.Register(new Handlers.WhoAmIRequestHandler
                {
                    UserId = Guid.NewGuid()
                });
            })).ToArray();

            // Simultaneously execute
            var executor = Task.Run(() =>
            {
                barrier.SignalAndWait();
                for (int i = 0; i < count; i++)
                {
                    var response = (Microsoft.Crm.Sdk.Messages.WhoAmIResponse)
                        service.Execute(new Microsoft.Crm.Sdk.Messages.WhoAmIRequest());
                    Assert.NotEqual(Guid.Empty, response.UserId);
                }
            });

            await Task.WhenAll(registrations.Concat(new[] { executor }).ToArray());
        }
    }
}
