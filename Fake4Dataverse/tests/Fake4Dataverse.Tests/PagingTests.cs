using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class PagingTests
    {
        private FakeOrganizationService CreateSeededService(int count)
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            for (int i = 1; i <= count; i++)
            {
                service.Create(new Entity("account") { ["name"] = $"Account{i:D3}", ["index"] = i });
            }
            return service;
        }

        [Fact]
        public void FirstPage_ReturnsCorrectPageSize()
        {
            var service = CreateSeededService(10);

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.AddOrder("index", OrderType.Ascending);
            query.PageInfo = new PagingInfo { PageNumber = 1, Count = 3 };

            var result = service.RetrieveMultiple(query);

            Assert.Equal(3, result.Entities.Count);
            Assert.True(result.MoreRecords);
            Assert.NotNull(result.PagingCookie);
        }

        [Fact]
        public void SecondPage_SkipsFirstPage()
        {
            var service = CreateSeededService(10);

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.AddOrder("index", OrderType.Ascending);
            query.PageInfo = new PagingInfo { PageNumber = 2, Count = 3 };

            var result = service.RetrieveMultiple(query);

            Assert.Equal(3, result.Entities.Count);
            Assert.Equal(4, result.Entities[0].GetAttributeValue<int>("index"));
            Assert.True(result.MoreRecords);
        }

        [Fact]
        public void LastPage_MoreRecordsFalse()
        {
            var service = CreateSeededService(10);

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.AddOrder("index", OrderType.Ascending);
            query.PageInfo = new PagingInfo { PageNumber = 4, Count = 3 }; // page 4: items 10

            var result = service.RetrieveMultiple(query);

            Assert.Single(result.Entities);
            Assert.Equal(10, result.Entities[0].GetAttributeValue<int>("index"));
            Assert.False(result.MoreRecords);
            Assert.Null(result.PagingCookie);
        }

        [Fact]
        public void ExactMultiple_LastPageHasNoMoreRecords()
        {
            var service = CreateSeededService(9);

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.AddOrder("index", OrderType.Ascending);
            query.PageInfo = new PagingInfo { PageNumber = 3, Count = 3 };

            var result = service.RetrieveMultiple(query);

            Assert.Equal(3, result.Entities.Count);
            Assert.False(result.MoreRecords);
        }

        [Fact]
        public void PageThrough_NoDuplicates_NoGaps()
        {
            var service = CreateSeededService(25);
            var allIds = new HashSet<Guid>();

            for (int page = 1; page <= 6; page++)
            {
                var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
                query.AddOrder("index", OrderType.Ascending);
                query.PageInfo = new PagingInfo { PageNumber = page, Count = 5 };

                var result = service.RetrieveMultiple(query);

                foreach (var entity in result.Entities)
                {
                    Assert.True(allIds.Add(entity.Id), $"Duplicate entity found: {entity.Id}");
                }

                if (page < 5)
                    Assert.True(result.MoreRecords, $"Page {page} should have more records");
            }

            Assert.Equal(25, allIds.Count);
        }

        [Fact]
        public void PageInfo_WithZeroCount_ReturnsAll()
        {
            var service = CreateSeededService(5);

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.PageInfo = new PagingInfo { PageNumber = 1, Count = 0 };

            var result = service.RetrieveMultiple(query);

            Assert.Equal(5, result.Entities.Count);
        }

        [Fact]
        public void PageInfo_Combined_WithFilter()
        {
            var service = CreateSeededService(20);

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("index", ConditionOperator.LessEqual, 10);
            query.AddOrder("index", OrderType.Ascending);
            query.PageInfo = new PagingInfo { PageNumber = 1, Count = 4 };

            var result = service.RetrieveMultiple(query);

            Assert.Equal(4, result.Entities.Count);
            Assert.True(result.MoreRecords);
            Assert.Equal(1, result.Entities[0].GetAttributeValue<int>("index"));
        }

        [Fact]
        public void BeyondLastPage_ReturnsEmpty()
        {
            var service = CreateSeededService(5);

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.AddOrder("index", OrderType.Ascending);
            query.PageInfo = new PagingInfo { PageNumber = 10, Count = 5 };

            var result = service.RetrieveMultiple(query);

            Assert.Empty(result.Entities);
            Assert.False(result.MoreRecords);
        }
    }
}
