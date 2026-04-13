using System;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class TransactionConcurrencyTests
    {
        // ── Transaction Rollback ─────────────────────────────────────────────

        [Fact]
        public void ExecuteTransaction_AllSucceed_AllCommitted()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var response = (ExecuteTransactionResponse)service.Execute(new ExecuteTransactionRequest
            {
                Requests = new OrganizationRequestCollection
                {
                    new CreateRequest { Target = new Entity("account") { ["name"] = "A" } },
                    new CreateRequest { Target = new Entity("account") { ["name"] = "B" } },
                    new CreateRequest { Target = new Entity("account") { ["name"] = "C" } }
                },
                ReturnResponses = true
            });

            Assert.Equal(3, response.Responses.Count);
            var all = service.RetrieveMultiple(new QueryExpression("account") { ColumnSet = new ColumnSet(true) });
            Assert.Equal(3, all.Entities.Count);
        }

        [Fact]
        public void ExecuteTransaction_FailsAtMiddle_RollsBackAllChanges()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var existingId = Guid.NewGuid();
            service.Create(new Entity("account", existingId) { ["name"] = "Existing" });

            var requests = new OrganizationRequestCollection
            {
                new CreateRequest { Target = new Entity("account") { ["name"] = "New1" } },
                new CreateRequest { Target = new Entity("account") { ["name"] = "New2" } },
                // This will fail — duplicate ID
                new CreateRequest { Target = new Entity("account", existingId) { ["name"] = "Duplicate" } },
                new CreateRequest { Target = new Entity("account") { ["name"] = "New3" } }
            };

            Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Execute(new ExecuteTransactionRequest { Requests = requests }));

            // Only the original entity should exist — New1 and New2 should be rolled back
            var all = service.RetrieveMultiple(new QueryExpression("account") { ColumnSet = new ColumnSet("name") });
            Assert.Single(all.Entities);
            Assert.Equal("Existing", all.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void ExecuteTransaction_FailsAtFirst_NoChanges()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var existingId = Guid.NewGuid();
            service.Create(new Entity("account", existingId) { ["name"] = "Existing" });

            var requests = new OrganizationRequestCollection
            {
                // Fails immediately
                new CreateRequest { Target = new Entity("account", existingId) { ["name"] = "Dup" } },
                new CreateRequest { Target = new Entity("account") { ["name"] = "After" } }
            };

            Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Execute(new ExecuteTransactionRequest { Requests = requests }));

            var all = service.RetrieveMultiple(new QueryExpression("account") { ColumnSet = new ColumnSet(true) });
            Assert.Single(all.Entities);
        }

        [Fact]
        public void ExecuteTransaction_UpdateRollback_RestoresOriginalValues()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var id = service.Create(new Entity("account") { ["name"] = "Original" });

            var requests = new OrganizationRequestCollection
            {
                new UpdateRequest { Target = new Entity("account", id) { ["name"] = "TxUpdated" } },
                // Second request fails — update nonexistent
                new UpdateRequest { Target = new Entity("account", Guid.NewGuid()) { ["name"] = "BadId" } }
            };

            Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Execute(new ExecuteTransactionRequest { Requests = requests }));

            // Original value should be restored
            var result = service.Retrieve("account", id, new ColumnSet("name"));
            Assert.Equal("Original", result.GetAttributeValue<string>("name"));
        }

        [Fact]
        public void ExecuteTransaction_DeleteRollback_RestoresDeletedEntity()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var id = service.Create(new Entity("account") { ["name"] = "WillDelete" });

            var requests = new OrganizationRequestCollection
            {
                new DeleteRequest { Target = new EntityReference("account", id) },
                // Fails — can't create with same ID that was just deleted, then something else fails
                new UpdateRequest { Target = new Entity("account", Guid.NewGuid()) { ["name"] = "Nonexistent" } }
            };

            Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Execute(new ExecuteTransactionRequest { Requests = requests }));

            // Deleted entity should be restored
            var result = service.Retrieve("account", id, new ColumnSet("name"));
            Assert.Equal("WillDelete", result.GetAttributeValue<string>("name"));
        }

        [Fact]
        public void ExecuteTransaction_MixedOperationsRollback_RestoresAllState()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var id1 = service.Create(new Entity("account") { ["name"] = "Account1" });
            var id2 = service.Create(new Entity("account") { ["name"] = "Account2" });

            var requests = new OrganizationRequestCollection
            {
                // Create a new one
                new CreateRequest { Target = new Entity("contact") { ["lastname"] = "Smith" } },
                // Update existing
                new UpdateRequest { Target = new Entity("account", id1) { ["name"] = "Modified1" } },
                // Delete existing
                new DeleteRequest { Target = new EntityReference("account", id2) },
                // This fails
                new UpdateRequest { Target = new Entity("bogus", Guid.NewGuid()) { ["x"] = "y" } }
            };

            Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Execute(new ExecuteTransactionRequest { Requests = requests }));

            // All changes should be rolled back
            var accounts = service.RetrieveMultiple(new QueryExpression("account") { ColumnSet = new ColumnSet("name") });
            Assert.Equal(2, accounts.Entities.Count);
            Assert.Equal("Account1", accounts.Entities.First(e => e.Id == id1).GetAttributeValue<string>("name"));
            Assert.Contains(accounts.Entities, e => e.Id == id2);

            var contacts = service.RetrieveMultiple(new QueryExpression("contact") { ColumnSet = new ColumnSet(true) });
            Assert.Empty(contacts.Entities);
        }

        [Fact]
        public void ExecuteTransaction_RetrieveMultipleWithinTransaction_SeesStagedCreate()
        {
            var env = new FakeDataverseEnvironment();
            env.AddIndex("account", "name"); // ensure indexed path does not hide transaction-local writes
            var service = env.CreateOrganizationService();

            var response = (ExecuteTransactionResponse)service.Execute(new ExecuteTransactionRequest
            {
                Requests = new OrganizationRequestCollection
                {
                    new CreateRequest
                    {
                        Target = new Entity("account") { ["name"] = "TxVisible" }
                    },
                    new RetrieveMultipleRequest
                    {
                        Query = new QueryExpression("account")
                        {
                            ColumnSet = new ColumnSet("name"),
                            Criteria =
                            {
                                Conditions =
                                {
                                    new ConditionExpression("name", ConditionOperator.Equal, "TxVisible")
                                }
                            }
                        }
                    }
                },
                ReturnResponses = true
            });

            var retrieveResponse = Assert.IsType<RetrieveMultipleResponse>(response.Responses[1]);
            Assert.Single(retrieveResponse.EntityCollection.Entities);
            Assert.Equal("TxVisible", retrieveResponse.EntityCollection.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void ExecuteTransaction_RetrieveMultipleWithinTransaction_SeesStagedUpdateThatNowMatchesIndex()
        {
            var env = new FakeDataverseEnvironment();
            env.AddIndex("account", "name");
            var service = env.CreateOrganizationService();

            var accountId = service.Create(new Entity("account") { ["name"] = "Before" });

            var response = (ExecuteTransactionResponse)service.Execute(new ExecuteTransactionRequest
            {
                Requests = new OrganizationRequestCollection
                {
                    new UpdateRequest
                    {
                        Target = new Entity("account", accountId) { ["name"] = "After" }
                    },
                    new RetrieveMultipleRequest
                    {
                        Query = new QueryExpression("account")
                        {
                            ColumnSet = new ColumnSet("name"),
                            Criteria =
                            {
                                Conditions =
                                {
                                    new ConditionExpression("name", ConditionOperator.Equal, "After")
                                }
                            }
                        }
                    }
                },
                ReturnResponses = true
            });

            var retrieveResponse = Assert.IsType<RetrieveMultipleResponse>(response.Responses[1]);
            Assert.Single(retrieveResponse.EntityCollection.Entities);
            Assert.Equal(accountId, retrieveResponse.EntityCollection.Entities[0].Id);
            Assert.Equal("After", retrieveResponse.EntityCollection.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void ExecuteTransaction_ReturnResponses_True_ReturnsAllResponses()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var response = (ExecuteTransactionResponse)service.Execute(new ExecuteTransactionRequest
            {
                Requests = new OrganizationRequestCollection
                {
                    new CreateRequest { Target = new Entity("account") { ["name"] = "A" } },
                    new CreateRequest { Target = new Entity("account") { ["name"] = "B" } }
                },
                ReturnResponses = true
            });

            Assert.Equal(2, response.Responses.Count);
            Assert.All(response.Responses, r => Assert.IsType<CreateResponse>(r));
        }

        // ── Nesting Validation ───────────────────────────────────────────────

        [Fact]
        public void ExecuteTransaction_ContainingExecuteMultiple_Throws()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var requests = new OrganizationRequestCollection
            {
                new CreateRequest { Target = new Entity("account") { ["name"] = "A" } },
                new ExecuteMultipleRequest
                {
                    Requests = new OrganizationRequestCollection
                    {
                        new CreateRequest { Target = new Entity("account") { ["name"] = "B" } }
                    },
                    Settings = new ExecuteMultipleSettings { ContinueOnError = false, ReturnResponses = true }
                }
            };

            Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Execute(new ExecuteTransactionRequest { Requests = requests }));
        }

        [Fact]
        public void ExecuteTransaction_ContainingExecuteTransaction_Throws()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var requests = new OrganizationRequestCollection
            {
                new ExecuteTransactionRequest
                {
                    Requests = new OrganizationRequestCollection
                    {
                        new CreateRequest { Target = new Entity("account") { ["name"] = "A" } }
                    }
                }
            };

            Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Execute(new ExecuteTransactionRequest { Requests = requests }));
        }

        // ── Concurrent Transaction Isolation ─────────────────────────────────

        [Fact]
        public void ExecuteTransaction_ConcurrentSessions_IndependentTransactions()
        {
            var env = new FakeDataverseEnvironment();
            var session1 = env.CreateOrganizationService();
            var session2 = env.CreateOrganizationService();

            // Pre-seed
            var id = session1.Create(new Entity("account") { ["name"] = "Shared" });

            // Session 1: transaction that succeeds
            session1.Execute(new ExecuteTransactionRequest
            {
                Requests = new OrganizationRequestCollection
                {
                    new UpdateRequest { Target = new Entity("account", id) { ["name"] = "Session1Updated" } }
                }
            });

            // Session 2: can see Session 1's committed changes
            var result = session2.Retrieve("account", id, new ColumnSet("name"));
            Assert.Equal("Session1Updated", result.GetAttributeValue<string>("name"));
        }

        [Fact]
        public void ExecuteTransaction_RolledBackChanges_InvisibleToOtherSessions()
        {
            var env = new FakeDataverseEnvironment();
            var session1 = env.CreateOrganizationService();
            var session2 = env.CreateOrganizationService();

            var id = session1.Create(new Entity("account") { ["name"] = "Original" });

            // Session 1: transaction that will fail and roll back
            Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                session1.Execute(new ExecuteTransactionRequest
                {
                    Requests = new OrganizationRequestCollection
                    {
                        new UpdateRequest { Target = new Entity("account", id) { ["name"] = "TxUpdate" } },
                        new UpdateRequest { Target = new Entity("account", Guid.NewGuid()) { ["name"] = "BadId" } }
                    }
                }));

            // Session 2: should still see original
            var result = session2.Retrieve("account", id, new ColumnSet("name"));
            Assert.Equal("Original", result.GetAttributeValue<string>("name"));
        }

        // ── Transaction + Optimistic Concurrency ─────────────────────────────

        [Fact]
        public void ExecuteTransaction_WithOptimisticConcurrency_VersionMismatch_RollsBack()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var id1 = service.Create(new Entity("account") { ["name"] = "Account1" });
            var id2 = service.Create(new Entity("account") { ["name"] = "Account2" });

            // Read version of account2
            var account2 = service.Retrieve("account", id2, new ColumnSet(true));
            var staleVersion = account2["versionnumber"].ToString();

            // Bump account2's version externally
            service.Update(new Entity("account", id2) { ["name"] = "BumpedExternally" });

            var requests = new OrganizationRequestCollection
            {
                // First request succeeds
                new UpdateRequest { Target = new Entity("account", id1) { ["name"] = "TxUpdated1" } },
                // Second request fails due to stale version
                new UpdateRequest
                {
                    Target = new Entity("account", id2) { ["name"] = "TxUpdated2", RowVersion = staleVersion },
                    ConcurrencyBehavior = ConcurrencyBehavior.IfRowVersionMatches
                }
            };

            Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Execute(new ExecuteTransactionRequest { Requests = requests }));

            // Account1 should NOT be modified (rolled back)
            var result1 = service.Retrieve("account", id1, new ColumnSet("name"));
            Assert.Equal("Account1", result1.GetAttributeValue<string>("name"));

            // Account2 should retain the externally bumped value
            var result2 = service.Retrieve("account", id2, new ColumnSet("name"));
            Assert.Equal("BumpedExternally", result2.GetAttributeValue<string>("name"));
        }

        // ── Parallel Transaction Stress Test ─────────────────────────────────

        [Fact]
        public void ExecuteTransaction_ParallelTransactions_MaintainConsistency()
        {
            var env = new FakeDataverseEnvironment();
            var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();
            int successCount = 0;

            Parallel.For(0, 20, i =>
            {
                try
                {
                    var session = env.CreateOrganizationService();
                    session.Execute(new ExecuteTransactionRequest
                    {
                        Requests = new OrganizationRequestCollection
                        {
                            new CreateRequest { Target = new Entity("account") { ["name"] = $"TxAccount-{i}-A" } },
                            new CreateRequest { Target = new Entity("account") { ["name"] = $"TxAccount-{i}-B" } }
                        },
                        ReturnResponses = true
                    });
                    Interlocked.Increment(ref successCount);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            });

            Assert.Empty(exceptions);
            Assert.Equal(20, successCount);

            // All 40 accounts should exist
            var service = env.CreateOrganizationService();
            var all = service.RetrieveMultiple(new QueryExpression("account") { ColumnSet = new ColumnSet(true) });
            Assert.Equal(40, all.Entities.Count);
        }

        // ── ExecuteMultiple within ExecuteTransaction ────────────────────────

        [Fact]
        public void ExecuteMultiple_CanContainExecuteTransaction()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var outerRequest = new ExecuteMultipleRequest
            {
                Requests = new OrganizationRequestCollection
                {
                    new ExecuteTransactionRequest
                    {
                        Requests = new OrganizationRequestCollection
                        {
                            new CreateRequest { Target = new Entity("account") { ["name"] = "TxInMultiple" } }
                        },
                        ReturnResponses = true
                    }
                },
                Settings = new ExecuteMultipleSettings { ContinueOnError = false, ReturnResponses = true }
            };

            var response = (ExecuteMultipleResponse)service.Execute(outerRequest);
            Assert.Single(response.Responses);
            Assert.Null(response.Responses[0].Fault);

            var all = service.RetrieveMultiple(new QueryExpression("account") { ColumnSet = new ColumnSet(true) });
            Assert.Single(all.Entities);
        }
    }
}
