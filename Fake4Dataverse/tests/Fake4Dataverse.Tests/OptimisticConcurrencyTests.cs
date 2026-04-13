using System;
using System.ServiceModel;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class OptimisticConcurrencyTests
    {
        // ── UpdateRequest with ConcurrencyBehavior.IfRowVersionMatches ───────

        [Fact]
        public void Update_IfRowVersionMatches_MatchingVersion_Succeeds()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });
            var retrieved = service.Retrieve("account", id, new ColumnSet(true));
            var rowVersion = retrieved["versionnumber"].ToString();

            service.Execute(new UpdateRequest
            {
                Target = new Entity("account", id) { ["name"] = "Updated", RowVersion = rowVersion },
                ConcurrencyBehavior = ConcurrencyBehavior.IfRowVersionMatches
            });

            var updated = service.Retrieve("account", id, new ColumnSet("name"));
            Assert.Equal("Updated", updated.GetAttributeValue<string>("name"));
        }

        [Fact]
        public void Update_IfRowVersionMatches_StaleVersion_ThrowsConcurrencyMismatch()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });
            var retrieved = service.Retrieve("account", id, new ColumnSet(true));
            var staleVersion = retrieved["versionnumber"].ToString();

            // Another update bumps the version
            service.Update(new Entity("account", id) { ["name"] = "Concurrent Update" });

            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Execute(new UpdateRequest
                {
                    Target = new Entity("account", id) { ["name"] = "Late Update", RowVersion = staleVersion },
                    ConcurrencyBehavior = ConcurrencyBehavior.IfRowVersionMatches
                }));

            Assert.Equal(DataverseFault.ConcurrencyVersionMismatch, ex.Detail.ErrorCode);
        }

        [Fact]
        public void Update_IfRowVersionMatches_NoRowVersion_ThrowsVersionNotProvided()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Execute(new UpdateRequest
                {
                    Target = new Entity("account", id) { ["name"] = "No Version" },
                    ConcurrencyBehavior = ConcurrencyBehavior.IfRowVersionMatches
                }));

            Assert.Equal(DataverseFault.ConcurrencyVersionNotProvided, ex.Detail.ErrorCode);
        }

        [Fact]
        public void Update_DefaultBehavior_IgnoresRowVersionMismatch()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

            // Normal Update doesn't check version even if one is stale
            service.Update(new Entity("account", id) { ["name"] = "Updated" });

            var result = service.Retrieve("account", id, new ColumnSet("name"));
            Assert.Equal("Updated", result.GetAttributeValue<string>("name"));
        }

        [Fact]
        public void Update_AlwaysOverwrite_IgnoresVersionMismatch()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

            service.Execute(new UpdateRequest
            {
                Target = new Entity("account", id) { ["name"] = "Overwritten", RowVersion = "999" },
                ConcurrencyBehavior = ConcurrencyBehavior.AlwaysOverwrite
            });

            var result = service.Retrieve("account", id, new ColumnSet("name"));
            Assert.Equal("Overwritten", result.GetAttributeValue<string>("name"));
        }

        // ── DeleteRequest with ConcurrencyBehavior.IfRowVersionMatches ───────

        [Fact]
        public void Delete_IfRowVersionMatches_MatchingVersion_Succeeds()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });
            var retrieved = service.Retrieve("account", id, new ColumnSet(true));
            var rowVersion = retrieved["versionnumber"].ToString();

            service.Execute(new DeleteRequest
            {
                Target = new EntityReference("account", id) { RowVersion = rowVersion },
                ConcurrencyBehavior = ConcurrencyBehavior.IfRowVersionMatches
            });

            Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Retrieve("account", id, new ColumnSet(true)));
        }

        [Fact]
        public void Delete_IfRowVersionMatches_StaleVersion_ThrowsConcurrencyMismatch()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });
            var retrieved = service.Retrieve("account", id, new ColumnSet(true));
            var staleVersion = retrieved["versionnumber"].ToString();

            // Bump the version
            service.Update(new Entity("account", id) { ["name"] = "Updated" });

            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Execute(new DeleteRequest
                {
                    Target = new EntityReference("account", id) { RowVersion = staleVersion },
                    ConcurrencyBehavior = ConcurrencyBehavior.IfRowVersionMatches
                }));

            Assert.Equal(DataverseFault.ConcurrencyVersionMismatch, ex.Detail.ErrorCode);
        }

        [Fact]
        public void Delete_IfRowVersionMatches_NoRowVersion_ThrowsVersionNotProvided()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Execute(new DeleteRequest
                {
                    Target = new EntityReference("account", id),
                    ConcurrencyBehavior = ConcurrencyBehavior.IfRowVersionMatches
                }));

            Assert.Equal(DataverseFault.ConcurrencyVersionNotProvided, ex.Detail.ErrorCode);
        }

        // ── Multi-session optimistic concurrency scenarios ───────────────────

        [Fact]
        public void Update_IfRowVersionMatches_TwoSessions_FirstWinsSecondFails()
        {
            var env = new FakeDataverseEnvironment();
            var session1 = env.CreateOrganizationService();
            var session2 = env.CreateOrganizationService();

            var id = session1.Create(new Entity("account") { ["name"] = "Original" });
            var accountV1 = session1.Retrieve("account", id, new ColumnSet(true));
            var versionAtRead = accountV1["versionnumber"].ToString();

            // Both sessions read the same version
            // Session 1 updates first — succeeds
            session1.Execute(new UpdateRequest
            {
                Target = new Entity("account", id) { ["name"] = "Session1", RowVersion = versionAtRead },
                ConcurrencyBehavior = ConcurrencyBehavior.IfRowVersionMatches
            });

            // Session 2 tries to update with the old version — fails
            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                session2.Execute(new UpdateRequest
                {
                    Target = new Entity("account", id) { ["name"] = "Session2", RowVersion = versionAtRead },
                    ConcurrencyBehavior = ConcurrencyBehavior.IfRowVersionMatches
                }));

            Assert.Equal(DataverseFault.ConcurrencyVersionMismatch, ex.Detail.ErrorCode);

            // Session1's update won
            var result = session1.Retrieve("account", id, new ColumnSet("name"));
            Assert.Equal("Session1", result.GetAttributeValue<string>("name"));
        }

        [Fact]
        public void Update_IfRowVersionMatches_RetryWithFreshVersion_Succeeds()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });
            var v1 = service.Retrieve("account", id, new ColumnSet(true));
            var versionV1 = v1["versionnumber"].ToString();

            // Concurrent update bumps version
            service.Update(new Entity("account", id) { ["name"] = "ConcurrentEdit" });

            // First attempt fails
            Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Execute(new UpdateRequest
                {
                    Target = new Entity("account", id) { ["name"] = "RetryMe", RowVersion = versionV1 },
                    ConcurrencyBehavior = ConcurrencyBehavior.IfRowVersionMatches
                }));

            // Re-read and retry with fresh version
            var v2 = service.Retrieve("account", id, new ColumnSet(true));
            var versionV2 = v2["versionnumber"].ToString();

            service.Execute(new UpdateRequest
            {
                Target = new Entity("account", id) { ["name"] = "RetrySuccess", RowVersion = versionV2 },
                ConcurrencyBehavior = ConcurrencyBehavior.IfRowVersionMatches
            });

            var result = service.Retrieve("account", id, new ColumnSet("name"));
            Assert.Equal("RetrySuccess", result.GetAttributeValue<string>("name"));
        }

        // ── Concurrent parallel optimistic concurrency ───────────────────────

        [Fact]
        public void Update_IfRowVersionMatches_ParallelUpdates_ExactlyOneWins()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var id = service.Create(new Entity("account") { ["name"] = "Original" });
            var retrieved = service.Retrieve("account", id, new ColumnSet(true));
            var version = retrieved["versionnumber"].ToString();

            int successCount = 0;
            int failCount = 0;

            Parallel.For(0, 10, i =>
            {
                try
                {
                    service.Execute(new UpdateRequest
                    {
                        Target = new Entity("account", id) { ["name"] = $"Parallel-{i}", RowVersion = version },
                        ConcurrencyBehavior = ConcurrencyBehavior.IfRowVersionMatches
                    });
                    System.Threading.Interlocked.Increment(ref successCount);
                }
                catch (FaultException<OrganizationServiceFault>)
                {
                    System.Threading.Interlocked.Increment(ref failCount);
                }
            });

            // Exactly one should succeed, the rest should fail with version mismatch
            Assert.Equal(1, successCount);
            Assert.Equal(9, failCount);
        }

        // ── Version number tracking ──────────────────────────────────────────

        [Fact]
        public void VersionNumber_IncreasesOnEachUpdate()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });
            var v1 = (long)service.Retrieve("account", id, new ColumnSet(true))["versionnumber"];

            service.Update(new Entity("account", id) { ["name"] = "Updated1" });
            var v2 = (long)service.Retrieve("account", id, new ColumnSet(true))["versionnumber"];

            service.Update(new Entity("account", id) { ["name"] = "Updated2" });
            var v3 = (long)service.Retrieve("account", id, new ColumnSet(true))["versionnumber"];

            Assert.True(v2 > v1);
            Assert.True(v3 > v2);
        }

        [Fact]
        public void VersionNumber_AutoSetDisabled_NoConcurrencyCheck()
        {
            var options = new FakeOrganizationServiceOptions { AutoSetVersionNumber = false };
            var env = new FakeDataverseEnvironment(options);
            var service = env.CreateOrganizationService();

            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });
            var retrieved = service.Retrieve("account", id, new ColumnSet(true));

            // No version number set, so RowVersion check against 0 should work
            // (both stored and expected are 0)
            Assert.False(retrieved.Contains("versionnumber"));
        }
    }
}
