using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class ConditionOperatorTests
    {
        private FakeOrganizationService CreateServiceWithClock(DateTime utcNow)
        {
            var clock = new FakeClock(utcNow);
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.Clock = clock;
            return service;
        }

        #region Comparison Operators

        [Fact]
        public void GreaterThan_FiltersCorrectly()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["revenue"] = new Money(100m) });
            service.Create(new Entity("account") { ["revenue"] = new Money(200m) });
            service.Create(new Entity("account") { ["revenue"] = new Money(300m) });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("revenue", ConditionOperator.GreaterThan, 150m);

            var result = service.RetrieveMultiple(query);
            Assert.Equal(2, result.Entities.Count);
        }

        [Fact]
        public void GreaterEqual_FiltersCorrectly()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["revenue"] = new Money(100m) });
            service.Create(new Entity("account") { ["revenue"] = new Money(200m) });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("revenue", ConditionOperator.GreaterEqual, 200m);

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
        }

        [Fact]
        public void LessThan_FiltersCorrectly()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["revenue"] = new Money(100m) });
            service.Create(new Entity("account") { ["revenue"] = new Money(200m) });
            service.Create(new Entity("account") { ["revenue"] = new Money(300m) });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("revenue", ConditionOperator.LessThan, 200m);

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
        }

        [Fact]
        public void LessEqual_FiltersCorrectly()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["revenue"] = new Money(100m) });
            service.Create(new Entity("account") { ["revenue"] = new Money(200m) });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("revenue", ConditionOperator.LessEqual, 100m);

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
        }

        #endregion

        #region String Operators

        [Fact]
        public void BeginsWith_CaseInsensitive()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso Ltd" });
            service.Create(new Entity("account") { ["name"] = "Fabrikam Inc" });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("name", ConditionOperator.BeginsWith, "contoso");

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        [Fact]
        public void EndsWith_CaseInsensitive()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso Ltd" });
            service.Create(new Entity("account") { ["name"] = "Fabrikam Inc" });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("name", ConditionOperator.EndsWith, "INC");

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        [Fact]
        public void Contains_CaseInsensitive()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso Ltd" });
            service.Create(new Entity("account") { ["name"] = "Fabrikam Inc" });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("name", ConditionOperator.Contains, "TOSO");

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        [Fact]
        public void DoesNotContain_FiltersCorrectly()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso Ltd" });
            service.Create(new Entity("account") { ["name"] = "Fabrikam Inc" });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("name", ConditionOperator.DoesNotContain, "Contoso");

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        [Fact]
        public void NotLike_FiltersCorrectly()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso Ltd" });
            service.Create(new Entity("account") { ["name"] = "Fabrikam Inc" });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("name", ConditionOperator.NotLike, "%Contoso%");

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        #endregion

        #region Between

        [Fact]
        public void Between_IntRange()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["employees"] = 50 });
            service.Create(new Entity("account") { ["employees"] = 150 });
            service.Create(new Entity("account") { ["employees"] = 250 });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("employees", ConditionOperator.Between, 100, 200);

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        [Fact]
        public void NotBetween_IntRange()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["employees"] = 50 });
            service.Create(new Entity("account") { ["employees"] = 150 });
            service.Create(new Entity("account") { ["employees"] = 250 });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("employees", ConditionOperator.NotBetween, 100, 200);

            Assert.Equal(2, service.RetrieveMultiple(query).Entities.Count);
        }

        [Fact]
        public void Between_DateRange()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 10) });
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 20) });
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 30) });

            var query = new QueryExpression("task") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("due", ConditionOperator.Between,
                new DateTime(2026, 3, 15), new DateTime(2026, 3, 25));

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        #endregion

        #region Date — Single Day

        [Fact]
        public void Yesterday_MatchesCorrectDay()
        {
            // "now" is 2026-03-15 12:00 UTC -> yesterday = 2026-03-14
            var service = CreateServiceWithClock(new DateTime(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc));
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 14, 8, 0, 0) });
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 15, 8, 0, 0) });

            var query = new QueryExpression("task") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("due", ConditionOperator.Yesterday);

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        [Fact]
        public void Today_MatchesCorrectDay()
        {
            var service = CreateServiceWithClock(new DateTime(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc));
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 15, 8, 0, 0) });
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 16, 8, 0, 0) });

            var query = new QueryExpression("task") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("due", ConditionOperator.Today);

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        [Fact]
        public void Tomorrow_MatchesCorrectDay()
        {
            var service = CreateServiceWithClock(new DateTime(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc));
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 16, 14, 0, 0) });
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 17, 8, 0, 0) });

            var query = new QueryExpression("task") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("due", ConditionOperator.Tomorrow);

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        #endregion

        #region Date — Relative Ranges

        [Fact]
        public void Last7Days_MatchesCorrectRange()
        {
            var service = CreateServiceWithClock(new DateTime(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc));
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 10, 8, 0, 0) });
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 1, 8, 0, 0) }); // too old

            var query = new QueryExpression("task") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("due", ConditionOperator.Last7Days);

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        [Fact]
        public void Next7Days_MatchesCorrectRange()
        {
            var service = CreateServiceWithClock(new DateTime(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc));
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 18, 8, 0, 0) });
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 30, 8, 0, 0) }); // too far

            var query = new QueryExpression("task") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("due", ConditionOperator.Next7Days);

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        [Fact]
        public void LastWeek_MatchesCorrectWeek()
        {
            // 2026-03-15 is a Sunday. Last Monday = 2026-03-02, this Monday = 2026-03-09
            var service = CreateServiceWithClock(new DateTime(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc));
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 4, 8, 0, 0) }); // last week (Wed)
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 12, 8, 0, 0) }); // this week

            var query = new QueryExpression("task") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("due", ConditionOperator.LastWeek);

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        [Fact]
        public void ThisWeek_MatchesCurrentWeek()
        {
            // 2026-03-11 is Wednesday. This week Monday = 2026-03-09
            var service = CreateServiceWithClock(new DateTime(2026, 3, 11, 12, 0, 0, DateTimeKind.Utc));
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 10, 8, 0, 0) }); // Tue (this week)
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 2, 8, 0, 0) });  // previous week

            var query = new QueryExpression("task") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("due", ConditionOperator.ThisWeek);

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        [Fact]
        public void ThisMonth_MatchesCurrentMonth()
        {
            var service = CreateServiceWithClock(new DateTime(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc));
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 20, 8, 0, 0) }); // March
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 2, 20, 8, 0, 0) }); // February

            var query = new QueryExpression("task") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("due", ConditionOperator.ThisMonth);

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        [Fact]
        public void LastMonth_MatchesPreviousMonth()
        {
            var service = CreateServiceWithClock(new DateTime(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc));
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 2, 15, 8, 0, 0) }); // February
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 15, 8, 0, 0) }); // March

            var query = new QueryExpression("task") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("due", ConditionOperator.LastMonth);

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        [Fact]
        public void NextMonth_MatchesNextMonth()
        {
            var service = CreateServiceWithClock(new DateTime(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc));
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 4, 10, 8, 0, 0) }); // April
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 15, 8, 0, 0) }); // March

            var query = new QueryExpression("task") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("due", ConditionOperator.NextMonth);

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        [Fact]
        public void ThisYear_MatchesCurrentYear()
        {
            var service = CreateServiceWithClock(new DateTime(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc));
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 6, 1, 8, 0, 0) });
            service.Create(new Entity("task") { ["due"] = new DateTime(2025, 6, 1, 8, 0, 0) });

            var query = new QueryExpression("task") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("due", ConditionOperator.ThisYear);

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        [Fact]
        public void LastYear_MatchesPreviousYear()
        {
            var service = CreateServiceWithClock(new DateTime(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc));
            service.Create(new Entity("task") { ["due"] = new DateTime(2025, 6, 1, 8, 0, 0) });
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 6, 1, 8, 0, 0) });

            var query = new QueryExpression("task") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("due", ConditionOperator.LastYear);

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        #endregion

        #region LastX / NextX

        [Fact]
        public void LastXDays_MatchesCorrectRange()
        {
            var service = CreateServiceWithClock(new DateTime(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc));
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 12, 8, 0, 0) }); // 3 days ago
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 1, 8, 0, 0) });  // 14 days ago

            var query = new QueryExpression("task") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("due", ConditionOperator.LastXDays, 5);

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        [Fact]
        public void NextXDays_MatchesCorrectRange()
        {
            var service = CreateServiceWithClock(new DateTime(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc));
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 17, 8, 0, 0) }); // 2 days ahead
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 30, 8, 0, 0) }); // 15 days ahead

            var query = new QueryExpression("task") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("due", ConditionOperator.NextXDays, 5);

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        [Fact]
        public void LastXHours_MatchesCorrectRange()
        {
            var service = CreateServiceWithClock(new DateTime(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc));
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 15, 10, 0, 0) }); // 2 hours ago
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 14, 10, 0, 0) }); // 26 hours ago

            var query = new QueryExpression("task") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("due", ConditionOperator.LastXHours, 4);

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        [Fact]
        public void NextXHours_MatchesCorrectRange()
        {
            var service = CreateServiceWithClock(new DateTime(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc));
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 15, 14, 0, 0) }); // 2 hours ahead
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 16, 12, 0, 0) }); // 24 hours ahead

            var query = new QueryExpression("task") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("due", ConditionOperator.NextXHours, 4);

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        [Fact]
        public void LastXMonths_MatchesCorrectRange()
        {
            var service = CreateServiceWithClock(new DateTime(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc));
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 2, 1, 8, 0, 0) });  // ~1.5 months ago
            service.Create(new Entity("task") { ["due"] = new DateTime(2025, 6, 1, 8, 0, 0) });  // 9 months ago

            var query = new QueryExpression("task") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("due", ConditionOperator.LastXMonths, 3);

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        [Fact]
        public void NextXMonths_MatchesCorrectRange()
        {
            var service = CreateServiceWithClock(new DateTime(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc));
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 4, 10, 8, 0, 0) });  // ~1 month ahead
            service.Create(new Entity("task") { ["due"] = new DateTime(2027, 1, 1, 8, 0, 0) });   // 10 months ahead

            var query = new QueryExpression("task") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("due", ConditionOperator.NextXMonths, 3);

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        [Fact]
        public void LastXYears_MatchesCorrectRange()
        {
            var service = CreateServiceWithClock(new DateTime(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc));
            service.Create(new Entity("task") { ["due"] = new DateTime(2025, 6, 1, 8, 0, 0) });
            service.Create(new Entity("task") { ["due"] = new DateTime(2020, 1, 1, 8, 0, 0) });

            var query = new QueryExpression("task") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("due", ConditionOperator.LastXYears, 2);

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        [Fact]
        public void LastXWeeks_MatchesCorrectRange()
        {
            var service = CreateServiceWithClock(new DateTime(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc));
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 5, 8, 0, 0) });  // ~10 days ago
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 1, 1, 8, 0, 0) });  // ~73 days ago

            var query = new QueryExpression("task") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("due", ConditionOperator.LastXWeeks, 2);

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        [Fact]
        public void NextXWeeks_MatchesCorrectRange()
        {
            var service = CreateServiceWithClock(new DateTime(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc));
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 20, 8, 0, 0) });
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 5, 1, 8, 0, 0) });

            var query = new QueryExpression("task") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("due", ConditionOperator.NextXWeeks, 2);

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        [Fact]
        public void NextXYears_MatchesCorrectRange()
        {
            var service = CreateServiceWithClock(new DateTime(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc));
            service.Create(new Entity("task") { ["due"] = new DateTime(2027, 1, 1, 8, 0, 0) });
            service.Create(new Entity("task") { ["due"] = new DateTime(2030, 1, 1, 8, 0, 0) });

            var query = new QueryExpression("task") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("due", ConditionOperator.NextXYears, 2);

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        #endregion

        #region On / OnOrBefore / OnOrAfter

        [Fact]
        public void On_MatchesExactDate()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 15, 10, 30, 0) });
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 16, 10, 30, 0) });

            var query = new QueryExpression("task") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("due", ConditionOperator.On, new DateTime(2026, 3, 15));

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        [Fact]
        public void OnOrBefore_MatchesDateAndEarlier()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 14, 10, 0, 0) });
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 15, 23, 59, 0) });
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 16, 0, 1, 0) });

            var query = new QueryExpression("task") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("due", ConditionOperator.OnOrBefore, new DateTime(2026, 3, 15));

            Assert.Equal(2, service.RetrieveMultiple(query).Entities.Count);
        }

        [Fact]
        public void OnOrAfter_MatchesDateAndLater()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 14, 23, 0, 0) });
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 15, 0, 0, 0) });
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 16, 10, 0, 0) });

            var query = new QueryExpression("task") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("due", ConditionOperator.OnOrAfter, new DateTime(2026, 3, 15));

            Assert.Equal(2, service.RetrieveMultiple(query).Entities.Count);
        }

        #endregion

        #region OlderThanX

        [Fact]
        public void OlderThanXMinutes_FiltersCorrectly()
        {
            var service = CreateServiceWithClock(new DateTime(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc));
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 15, 11, 0, 0) }); // 60 min old
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 15, 11, 50, 0) }); // 10 min old

            var query = new QueryExpression("task") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("due", ConditionOperator.OlderThanXMinutes, 30);

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        [Fact]
        public void OlderThanXHours_FiltersCorrectly()
        {
            var service = CreateServiceWithClock(new DateTime(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc));
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 15, 8, 0, 0) }); // 4 hours old
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 15, 11, 0, 0) }); // 1 hour old

            var query = new QueryExpression("task") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("due", ConditionOperator.OlderThanXHours, 2);

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        [Fact]
        public void OlderThanXDays_FiltersCorrectly()
        {
            var service = CreateServiceWithClock(new DateTime(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc));
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 1, 8, 0, 0) }); // 14 days old
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 14, 8, 0, 0) }); // 1 day old

            var query = new QueryExpression("task") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("due", ConditionOperator.OlderThanXDays, 7);

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        [Fact]
        public void OlderThanXWeeks_FiltersCorrectly()
        {
            var service = CreateServiceWithClock(new DateTime(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc));
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 2, 1, 8, 0, 0) });  // ~6 weeks old
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 3, 10, 8, 0, 0) }); // ~5 days old

            var query = new QueryExpression("task") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("due", ConditionOperator.OlderThanXWeeks, 4);

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        [Fact]
        public void OlderThanXMonths_FiltersCorrectly()
        {
            var service = CreateServiceWithClock(new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc));
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 1, 1, 8, 0, 0) });  // ~5.5 months old
            service.Create(new Entity("task") { ["due"] = new DateTime(2026, 5, 1, 8, 0, 0) });  // ~1.5 months old

            var query = new QueryExpression("task") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("due", ConditionOperator.OlderThanXMonths, 3);

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        [Fact]
        public void OlderThanXYears_FiltersCorrectly()
        {
            var service = CreateServiceWithClock(new DateTime(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc));
            service.Create(new Entity("task") { ["due"] = new DateTime(2020, 1, 1, 8, 0, 0) });
            service.Create(new Entity("task") { ["due"] = new DateTime(2025, 6, 1, 8, 0, 0) });

            var query = new QueryExpression("task") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("due", ConditionOperator.OlderThanXYears, 3);

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        #endregion

        #region User Context Operators

        [Fact]
        public void EqualUserId_MatchesCallerId()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var callerId = service.CallerId;
            var otherId = Guid.NewGuid();
            service.Create(new Entity("task") { ["ownerid"] = new EntityReference("systemuser", callerId) });
            service.Create(new Entity("task") { ["ownerid"] = new EntityReference("systemuser", otherId) });

            var query = new QueryExpression("task") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("ownerid", ConditionOperator.EqualUserId);

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        [Fact]
        public void NotEqualUserId_ExcludesCallerId()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var callerId = service.CallerId;
            service.Create(new Entity("task") { ["ownerid"] = new EntityReference("systemuser", callerId) });
            service.Create(new Entity("task") { ["ownerid"] = new EntityReference("systemuser", Guid.NewGuid()) });

            var query = new QueryExpression("task") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("ownerid", ConditionOperator.NotEqualUserId);

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        #endregion

        #region NotNull / NotEqual

        [Fact]
        public void NotNull_FiltersNullValues()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Create(new Entity("account")); // no name

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("name", ConditionOperator.NotNull);

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        [Fact]
        public void NotEqual_FiltersCorrectly()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Create(new Entity("account") { ["name"] = "Fabrikam" });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("name", ConditionOperator.NotEqual, "Contoso");

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        [Fact]
        public void NotIn_FiltersCorrectly()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "A" });
            service.Create(new Entity("account") { ["name"] = "B" });
            service.Create(new Entity("account") { ["name"] = "C" });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("name", ConditionOperator.NotIn, "A", "B");

            Assert.Single(service.RetrieveMultiple(query).Entities);
        }

        [Fact]
        public void DoesNotBeginWith_FiltersCorrectly()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Create(new Entity("account") { ["name"] = "Fabrikam" });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("name", ConditionOperator.DoesNotBeginWith, "Con");

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("Fabrikam", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void DoesNotEndWith_FiltersCorrectly()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso Ltd" });
            service.Create(new Entity("account") { ["name"] = "Fabrikam Inc" });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("name", ConditionOperator.DoesNotEndWith, "Ltd");

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("Fabrikam Inc", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void In_WithEmptyArray_ReturnsNoMatches()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso" });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("name", ConditionOperator.In);

            var result = service.RetrieveMultiple(query);
            Assert.Empty(result.Entities);
        }

        [Fact]
        public void NotIn_WithEmptyArray_ReturnsAll()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "A" });
            service.Create(new Entity("account") { ["name"] = "B" });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("name", ConditionOperator.NotIn);

            var result = service.RetrieveMultiple(query);
            Assert.Equal(2, result.Entities.Count);
        }

        [Fact]
        public void NotEqual_WithNull_ReturnsNonNullRecords()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Create(new Entity("account")); // no name

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("name", ConditionOperator.NotEqual, null);

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("Contoso", result.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void GreaterThan_WithNullValue_DoesNotMatch()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["revenue"] = new Money(100m) });
            service.Create(new Entity("account")); // no revenue

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("revenue", ConditionOperator.GreaterThan, 50m);

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
        }

        [Fact]
        public void Equal_OnEntityReference_MatchesById()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var targetId = Guid.NewGuid();
            service.Create(new Entity("contact") { ["parentcustomerid"] = new EntityReference("account", targetId) });
            service.Create(new Entity("contact") { ["parentcustomerid"] = new EntityReference("account", Guid.NewGuid()) });

            var query = new QueryExpression("contact") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("parentcustomerid", ConditionOperator.Equal, targetId);

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
        }

        [Fact]
        public void Equal_OnOptionSetValue_MatchesByInt()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["statecode"] = new OptionSetValue(0) });
            service.Create(new Entity("account") { ["statecode"] = new OptionSetValue(1) });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
        }

        #endregion

        #region Ordering

        [Fact]
        public void Descending_Order()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Alice" });
            service.Create(new Entity("account") { ["name"] = "Charlie" });
            service.Create(new Entity("account") { ["name"] = "Bob" });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.AddOrder("name", OrderType.Descending);

            var result = service.RetrieveMultiple(query);
            Assert.Equal("Charlie", result.Entities[0].GetAttributeValue<string>("name"));
            Assert.Equal("Bob", result.Entities[1].GetAttributeValue<string>("name"));
            Assert.Equal("Alice", result.Entities[2].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void MultiFieldOrdering()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("contact") { ["lastname"] = "Smith", ["firstname"] = "Bob" });
            service.Create(new Entity("contact") { ["lastname"] = "Smith", ["firstname"] = "Alice" });
            service.Create(new Entity("contact") { ["lastname"] = "Jones", ["firstname"] = "Charlie" });

            var query = new QueryExpression("contact") { ColumnSet = new ColumnSet(true) };
            query.AddOrder("lastname", OrderType.Ascending);
            query.AddOrder("firstname", OrderType.Ascending);

            var result = service.RetrieveMultiple(query);
            Assert.Equal("Jones", result.Entities[0].GetAttributeValue<string>("lastname"));
            Assert.Equal("Alice", result.Entities[1].GetAttributeValue<string>("firstname"));
            Assert.Equal("Bob", result.Entities[2].GetAttributeValue<string>("firstname"));
        }

        #endregion

        #region Equal on Different Types

        [Fact]
        public void Equal_OnInt_MatchesByValue()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["employeecount"] = 100 });
            service.Create(new Entity("account") { ["employeecount"] = 200 });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("employeecount", ConditionOperator.Equal, 100);

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
        }

        [Fact]
        public void Equal_OnGuid_MatchesByValue()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var targetGuid = Guid.NewGuid();
            var otherGuid = Guid.NewGuid();
            service.Create(new Entity("account") { ["parentid"] = targetGuid });
            service.Create(new Entity("account") { ["parentid"] = otherGuid });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("parentid", ConditionOperator.Equal, targetGuid);

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
        }

        [Fact]
        public void Equal_OnDateTime_MatchesByValue()
        {
            var env = new FakeDataverseEnvironment(FakeOrganizationServiceOptions.Lenient);
            var service = env.CreateOrganizationService();
            var date1 = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            var date2 = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc);
            service.Create(new Entity("task") { ["scheduledend"] = date1 });
            service.Create(new Entity("task") { ["scheduledend"] = date2 });

            var query = new QueryExpression("task") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("scheduledend", ConditionOperator.Equal, date1);

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
        }

        [Fact]
        public void Equal_OnMoney_MatchesByDecimalValue()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["revenue"] = new Money(1000m) });
            service.Create(new Entity("account") { ["revenue"] = new Money(2000m) });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("revenue", ConditionOperator.Equal, 1000m);

            var result = service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
        }

        #endregion
    }
}
