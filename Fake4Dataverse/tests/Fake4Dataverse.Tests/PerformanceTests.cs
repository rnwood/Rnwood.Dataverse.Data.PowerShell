using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class PerformanceTests
    {
        private const int EntityCount = 1000;

        [Fact]
        public void BulkCreate_LargeDataSet_AllRetrievable()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var ids = new List<Guid>(EntityCount);

            for (int i = 0; i < EntityCount; i++)
            {
                ids.Add(service.Create(new Entity("account") { ["name"] = $"Account {i}" }));
            }

            Assert.Equal(EntityCount, ids.Count);
            Assert.All(ids, id => Assert.NotEqual(Guid.Empty, id));

            for (int i = 0; i < EntityCount; i++)
            {
                var entity = service.Retrieve("account", ids[i], new ColumnSet("name"));
                Assert.Equal($"Account {i}", entity.GetAttributeValue<string>("name"));
            }
        }

        [Fact]
        public void BulkUpdate_LargeDataSet_AllUpdated()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var ids = SeedAccounts(service, EntityCount);

            for (int i = 0; i < EntityCount; i++)
            {
                service.Update(new Entity("account", ids[i]) { ["name"] = $"Updated {i}" });
            }

            for (int i = 0; i < EntityCount; i++)
            {
                var entity = service.Retrieve("account", ids[i], new ColumnSet("name"));
                Assert.Equal($"Updated {i}", entity.GetAttributeValue<string>("name"));
            }
        }

        [Fact]
        public void BulkDelete_LargeDataSet_AllRemoved()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var ids = SeedAccounts(service, EntityCount);

            for (int i = 0; i < EntityCount; i++)
            {
                service.Delete("account", ids[i]);
            }

            var result = service.RetrieveMultiple(new QueryExpression("account") { ColumnSet = new ColumnSet(true) });
            Assert.Empty(result.Entities);
        }

        [Fact]
        public void RetrieveMultiple_LargeDataSet_ReturnsAllMatches()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            for (int i = 0; i < EntityCount; i++)
            {
                service.Create(new Entity("account")
                {
                    ["name"] = $"Account {i}",
                    ["industrycode"] = new OptionSetValue(i % 10)
                });
            }

            var query = new QueryExpression("account")
            {
                ColumnSet = new ColumnSet("name", "industrycode")
            };
            query.Criteria.AddCondition("industrycode", ConditionOperator.Equal, 5);

            var result = service.RetrieveMultiple(query);

            Assert.Equal(EntityCount / 10, result.Entities.Count);
            Assert.All(result.Entities, e =>
                Assert.Equal(5, e.GetAttributeValue<OptionSetValue>("industrycode").Value));
        }

        [Fact]
        public void IndexedQuery_ReturnsCorrectResults()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.AddIndex("account", "industrycode");

            for (int i = 0; i < EntityCount; i++)
            {
                service.Create(new Entity("account")
                {
                    ["name"] = $"Account {i}",
                    ["industrycode"] = new OptionSetValue(i % 10)
                });
            }

            var query = new QueryExpression("account")
            {
                ColumnSet = new ColumnSet("name", "industrycode")
            };
            query.Criteria.AddCondition("industrycode", ConditionOperator.Equal, 3);

            var result = service.RetrieveMultiple(query);

            Assert.Equal(EntityCount / 10, result.Entities.Count);
            Assert.All(result.Entities, e =>
                Assert.Equal(3, e.GetAttributeValue<OptionSetValue>("industrycode").Value));
        }

        [Fact]
        public void IndexedQuery_MatchesFullScanResults()
        {
            var indexedEnv = new FakeDataverseEnvironment();
            var indexed = indexedEnv.CreateOrganizationService();
            var plainEnv = new FakeDataverseEnvironment();
            var plain = plainEnv.CreateOrganizationService();
            indexedEnv.AddIndex("account", "industrycode");

            for (int i = 0; i < EntityCount; i++)
            {
                var id = Guid.NewGuid();
                indexed.Create(new Entity("account", id) { ["name"] = $"Account {i}", ["industrycode"] = new OptionSetValue(i % 10) });
                plain.Create(new Entity("account", id) { ["name"] = $"Account {i}", ["industrycode"] = new OptionSetValue(i % 10) });
            }

            var query = new QueryExpression("account")
            {
                ColumnSet = new ColumnSet("name", "industrycode")
            };
            query.Criteria.AddCondition("industrycode", ConditionOperator.Equal, 7);

            var indexedResult = indexed.RetrieveMultiple(query);
            var plainResult = plain.RetrieveMultiple(query);

            Assert.Equal(plainResult.Entities.Count, indexedResult.Entities.Count);

            var indexedIds = indexedResult.Entities.Select(e => e.Id).OrderBy(x => x).ToList();
            var plainIds = plainResult.Entities.Select(e => e.Id).OrderBy(x => x).ToList();
            Assert.Equal(plainIds, indexedIds);
        }

        [Fact]
        public void AddIndex_AfterSeed_RetroactivelyIndexes()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            for (int i = 0; i < EntityCount; i++)
            {
                service.Create(new Entity("account")
                {
                    ["name"] = $"Account {i}",
                    ["industrycode"] = new OptionSetValue(i % 10)
                });
            }

            // Add index AFTER entities already exist
            env.AddIndex("account", "industrycode");

            var query = new QueryExpression("account")
            {
                ColumnSet = new ColumnSet("name", "industrycode")
            };
            query.Criteria.AddCondition("industrycode", ConditionOperator.Equal, 2);

            var result = service.RetrieveMultiple(query);

            Assert.Equal(EntityCount / 10, result.Entities.Count);
            Assert.All(result.Entities, e =>
                Assert.Equal(2, e.GetAttributeValue<OptionSetValue>("industrycode").Value));
        }

        [Fact]
        public void MultipleIndexedConditions_IntersectsResults()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.AddIndex("account", "industrycode");
            env.AddIndex("account", "region");

            for (int i = 0; i < EntityCount; i++)
            {
                service.Create(new Entity("account")
                {
                    ["name"] = $"Account {i}",
                    ["industrycode"] = new OptionSetValue(i % 10),
                    ["region"] = new OptionSetValue(i % 3)
                });
            }

            var query = new QueryExpression("account")
            {
                ColumnSet = new ColumnSet("name", "industrycode", "region")
            };
            query.Criteria.AddCondition("industrycode", ConditionOperator.Equal, 5);
            query.Criteria.AddCondition("region", ConditionOperator.Equal, 1);

            var result = service.RetrieveMultiple(query);

            var expected = Enumerable.Range(0, EntityCount)
                .Count(i => i % 10 == 5 && i % 3 == 1);
            Assert.Equal(expected, result.Entities.Count);
            Assert.All(result.Entities, e =>
            {
                Assert.Equal(5, e.GetAttributeValue<OptionSetValue>("industrycode").Value);
                Assert.Equal(1, e.GetAttributeValue<OptionSetValue>("region").Value);
            });
        }

        [Fact]
        public void BulkSeed_LargeDataSet_AllQueryable()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var entities = Enumerable.Range(0, EntityCount).Select(i =>
                new Entity("contact", Guid.NewGuid()) { ["firstname"] = $"First{i}", ["lastname"] = $"Last{i}" }).ToArray();

            env.Seed(entities);

            var result = service.RetrieveMultiple(new QueryExpression("contact") { ColumnSet = new ColumnSet(true) });
            Assert.Equal(EntityCount, result.Entities.Count);
        }

        [Fact]
        public void IndexUpdated_OnUpdateAndDelete()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.AddIndex("account", "industrycode");

            var ids = new List<Guid>();
            for (int i = 0; i < 100; i++)
            {
                ids.Add(service.Create(new Entity("account")
                {
                    ["name"] = $"Account {i}",
                    ["industrycode"] = new OptionSetValue(i % 5)
                }));
            }

            // Update first 20 entities (industrycode 0-4) to industrycode=9
            for (int i = 0; i < 20; i++)
            {
                service.Update(new Entity("account", ids[i]) { ["industrycode"] = new OptionSetValue(9) });
            }

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("industrycode", ConditionOperator.Equal, 0);

            var result = service.RetrieveMultiple(query);
            // Originally 20 entities had industrycode=0, first 4 of them (i=0,5,10,15) were updated
            var expectedZeros = Enumerable.Range(0, 100)
                .Count(i => i % 5 == 0 && i >= 20);
            Assert.Equal(expectedZeros, result.Entities.Count);

            // Delete some entities and verify index consistency
            for (int i = 0; i < 20; i++)
            {
                service.Delete("account", ids[i]);
            }

            query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("industrycode", ConditionOperator.Equal, 9);

            result = service.RetrieveMultiple(query);
            Assert.Empty(result.Entities); // All industrycode=9 entities were deleted
        }

        private static List<Guid> SeedAccounts(FakeOrganizationService service, int count)
        {
            var ids = new List<Guid>(count);
            for (int i = 0; i < count; i++)
            {
                ids.Add(service.Create(new Entity("account") { ["name"] = $"Account {i}" }));
            }
            return ids;
        }
    }
}
