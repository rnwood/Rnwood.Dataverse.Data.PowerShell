using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class TimeManipulationTests
    {
        [Fact]
        public void AdvanceTime_WithFakeClock_AdvancesTime()
        {
            var clock = new FakeClock(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            var env = new FakeDataverseEnvironment();
            env.Clock = clock;

            env.AdvanceTime(TimeSpan.FromHours(2));

            Assert.Equal(new DateTime(2026, 1, 1, 2, 0, 0, DateTimeKind.Utc), clock.UtcNow);
        }

        [Fact]
        public void AdvanceTime_WithSystemClock_Throws()
        {
            var env = new FakeDataverseEnvironment();

            Assert.Throws<InvalidOperationException>(() => env.AdvanceTime(TimeSpan.FromHours(1)));
        }

        [Fact]
        public void AdvanceTime_AffectsCreatedOn()
        {
            var clock = new FakeClock(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.Clock = clock;

            var id1 = service.Create(new Entity("account") { ["name"] = "First" });
            env.AdvanceTime(TimeSpan.FromDays(1));
            var id2 = service.Create(new Entity("account") { ["name"] = "Second" });

            var e1 = service.Retrieve("account", id1, new ColumnSet("createdon"));
            var e2 = service.Retrieve("account", id2, new ColumnSet("createdon"));

            Assert.Equal(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), e1.GetAttributeValue<DateTime>("createdon"));
            Assert.Equal(new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc), e2.GetAttributeValue<DateTime>("createdon"));
        }

        [Fact]
        public void AdvanceTime_AffectsModifiedOn()
        {
            var clock = new FakeClock(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.Clock = clock;

            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });
            env.AdvanceTime(TimeSpan.FromHours(6));
            service.Update(new Entity("account", id) { ["name"] = "Updated" });

            var retrieved = service.Retrieve("account", id, new ColumnSet("createdon", "modifiedon"));
            Assert.Equal(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), retrieved.GetAttributeValue<DateTime>("createdon"));
            Assert.Equal(new DateTime(2026, 1, 1, 6, 0, 0, DateTimeKind.Utc), retrieved.GetAttributeValue<DateTime>("modifiedon"));
        }

        [Fact]
        public void AdvanceTime_AffectsDateFiltering()
        {
            var clock = new FakeClock(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.Clock = clock;

            service.Create(new Entity("account") { ["name"] = "Old" });
            env.AdvanceTime(TimeSpan.FromDays(7));
            service.Create(new Entity("account") { ["name"] = "New" });

            var query = new QueryExpression("account")
            {
                ColumnSet = new ColumnSet("name", "createdon")
            };
            query.Criteria.AddCondition("createdon", ConditionOperator.OnOrAfter, new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc));

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("New", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void FakeClock_Advance_WorksDirectly()
        {
            var clock = new FakeClock();
            var initial = clock.UtcNow;

            clock.Advance(TimeSpan.FromMinutes(30));

            Assert.Equal(initial.AddMinutes(30), clock.UtcNow);
        }

        [Fact]
        public void FakeClock_Set_ChangesTime()
        {
            var clock = new FakeClock();
            var newTime = new DateTime(2030, 6, 15, 12, 0, 0, DateTimeKind.Utc);

            clock.Set(newTime);

            Assert.Equal(newTime, clock.UtcNow);
        }

        [Fact]
        public void AdvanceTime_MultipleAdvances_Accumulate()
        {
            var clock = new FakeClock(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            var env = new FakeDataverseEnvironment();
            env.Clock = clock;

            env.AdvanceTime(TimeSpan.FromHours(1));
            env.AdvanceTime(TimeSpan.FromHours(2));
            env.AdvanceTime(TimeSpan.FromMinutes(30));

            Assert.Equal(new DateTime(2026, 1, 1, 3, 30, 0, DateTimeKind.Utc), clock.UtcNow);
        }
    }
}
