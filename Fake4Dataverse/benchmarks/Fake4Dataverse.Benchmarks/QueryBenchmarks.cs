#if NET8_0_OR_GREATER
using FakeXrmEasy.Middleware;
using FakeXrmEasy.Middleware.Crud;
using FakeXrmEasy.Abstractions.Enums;
#endif
using System;
using BenchmarkDotNet.Attributes;
#if NET462
using FakeXrmEasy;
#endif
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Fake4Dataverse.Benchmarks
{
    /// <summary>
    /// QueryExpression / RetrieveMultiple throughput comparison.
    /// Seeds EntityCount account records then runs paged queries (page 1, 50 per page).
    ///
    ///   .NET Framework 4.6.2 — Fake4Dataverse vs FakeXrmEasy 1.x (FakeXrmEasy.9 v1.58.1, MIT)
    ///   .NET 10              — Fake4Dataverse vs FakeXrmEasy 3.x (FakeXrmEasy.Core.v9 v3.9.0, RPL-1.5)
    /// </summary>
    [MemoryDiagnoser]
    public class QueryBenchmarks
    {
        private FakeOrganizationService _f4d = null!;
        private FakeDataverseEnvironment _f4dEnv = null!;

#if NET462
        private IOrganizationService _v1 = null!;
#elif NET8_0_OR_GREATER
        private IOrganizationService _v3 = null!;
#endif

        [Params(100, 1_000, 10_000)]
        public int EntityCount { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            var entities = new Entity[EntityCount];
            for (int i = 0; i < EntityCount; i++)
            {
                entities[i] = new Entity("account")
                {
                    Id = Guid.NewGuid(),
                    ["name"]              = $"Account {i:D6}",
                    ["revenue"]           = new Money(i * 100m),
                    ["statecode"]         = new OptionSetValue(i % 2),
                    ["numberofemployees"] = i % 500,
                };
            }

            _f4dEnv = new FakeDataverseEnvironment(FakeOrganizationServiceOptions.Lenient);
            _f4d = _f4dEnv.CreateOrganizationService();
            _f4dEnv.Seed(entities);

#if NET462
            var v1ctx = new XrmFakedContext();
            v1ctx.Initialize(entities);
            _v1 = v1ctx.GetOrganizationService();
#elif NET8_0_OR_GREATER
            var v3ctx = MiddlewareBuilder
                .New()
                .AddCrud()
                .UseCrud()
                .SetLicense(FakeXrmEasyLicense.RPL_1_5)
                .Build();
            v3ctx.Initialize(entities);
            _v3 = v3ctx.GetOrganizationService();
#endif
        }

        // Full scan, all columns, paged (page 1 / 50)

        [Benchmark(Baseline = true)]
        public EntityCollection F4D_AllColumns()
        {
            var qe = new QueryExpression("account")
            {
                ColumnSet = new ColumnSet(true),
                PageInfo  = new PagingInfo { Count = 50, PageNumber = 1 },
            };
            return _f4d.RetrieveMultiple(qe);
        }

#if NET462
        [Benchmark]
        public EntityCollection V1_AllColumns()
        {
            var qe = new QueryExpression("account")
            {
                ColumnSet = new ColumnSet(true),
                PageInfo  = new PagingInfo { Count = 50, PageNumber = 1 },
            };
            return _v1.RetrieveMultiple(qe);
        }
#elif NET8_0_OR_GREATER
        [Benchmark]
        public EntityCollection V3_AllColumns()
        {
            var qe = new QueryExpression("account")
            {
                ColumnSet = new ColumnSet(true),
                PageInfo  = new PagingInfo { Count = 50, PageNumber = 1 },
            };
            return _v3.RetrieveMultiple(qe);
        }
#endif

        // Equality filter on statecode (~50% of rows)

        [Benchmark]
        public EntityCollection F4D_WithFilter()
        {
            var qe = new QueryExpression("account")
            {
                ColumnSet = new ColumnSet("name", "revenue"),
                PageInfo  = new PagingInfo { Count = 50, PageNumber = 1 },
            };
            qe.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
            return _f4d.RetrieveMultiple(qe);
        }

#if NET462
        [Benchmark]
        public EntityCollection V1_WithFilter()
        {
            var qe = new QueryExpression("account")
            {
                ColumnSet = new ColumnSet("name", "revenue"),
                PageInfo  = new PagingInfo { Count = 50, PageNumber = 1 },
            };
            qe.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
            return _v1.RetrieveMultiple(qe);
        }
#elif NET8_0_OR_GREATER
        [Benchmark]
        public EntityCollection V3_WithFilter()
        {
            var qe = new QueryExpression("account")
            {
                ColumnSet = new ColumnSet("name", "revenue"),
                PageInfo  = new PagingInfo { Count = 50, PageNumber = 1 },
            };
            qe.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
            return _v3.RetrieveMultiple(qe);
        }
#endif

        // Order by name ascending

        [Benchmark]
        public EntityCollection F4D_WithOrder()
        {
            var qe = new QueryExpression("account")
            {
                ColumnSet = new ColumnSet("name"),
                PageInfo  = new PagingInfo { Count = 50, PageNumber = 1 },
            };
            qe.AddOrder("name", OrderType.Ascending);
            return _f4d.RetrieveMultiple(qe);
        }

#if NET462
        [Benchmark]
        public EntityCollection V1_WithOrder()
        {
            var qe = new QueryExpression("account")
            {
                ColumnSet = new ColumnSet("name"),
                PageInfo  = new PagingInfo { Count = 50, PageNumber = 1 },
            };
            qe.AddOrder("name", OrderType.Ascending);
            return _v1.RetrieveMultiple(qe);
        }
#elif NET8_0_OR_GREATER
        [Benchmark]
        public EntityCollection V3_WithOrder()
        {
            var qe = new QueryExpression("account")
            {
                ColumnSet = new ColumnSet("name"),
                PageInfo  = new PagingInfo { Count = 50, PageNumber = 1 },
            };
            qe.AddOrder("name", OrderType.Ascending);
            return _v3.RetrieveMultiple(qe);
        }
#endif

        // Filter + order combined

        [Benchmark]
        public EntityCollection F4D_FilterAndOrder()
        {
            var qe = new QueryExpression("account")
            {
                ColumnSet = new ColumnSet("name", "revenue"),
                PageInfo  = new PagingInfo { Count = 50, PageNumber = 1 },
            };
            qe.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
            qe.AddOrder("name", OrderType.Ascending);
            return _f4d.RetrieveMultiple(qe);
        }

#if NET462
        [Benchmark]
        public EntityCollection V1_FilterAndOrder()
        {
            var qe = new QueryExpression("account")
            {
                ColumnSet = new ColumnSet("name", "revenue"),
                PageInfo  = new PagingInfo { Count = 50, PageNumber = 1 },
            };
            qe.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
            qe.AddOrder("name", OrderType.Ascending);
            return _v1.RetrieveMultiple(qe);
        }
#elif NET8_0_OR_GREATER
        [Benchmark]
        public EntityCollection V3_FilterAndOrder()
        {
            var qe = new QueryExpression("account")
            {
                ColumnSet = new ColumnSet("name", "revenue"),
                PageInfo  = new PagingInfo { Count = 50, PageNumber = 1 },
            };
            qe.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
            qe.AddOrder("name", OrderType.Ascending);
            return _v3.RetrieveMultiple(qe);
        }
#endif
    }
}
