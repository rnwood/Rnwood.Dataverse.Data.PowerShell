using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class NoLockTests
    {
        // ── QueryExpression NoLock — results are identical regardless of value ──

        [Fact]
        public void QueryExpression_NoLockFalse_ReturnsResults()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso" });

            var query = new QueryExpression("account")
            {
                ColumnSet = new ColumnSet(true),
                NoLock = false
            };
            var result = service.RetrieveMultiple(query);

            Assert.Single(result.Entities);
        }

        [Fact]
        public void QueryExpression_NoLockTrue_ReturnsResults()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso" });

            var query = new QueryExpression("account")
            {
                ColumnSet = new ColumnSet(true),
                NoLock = true
            };
            var result = service.RetrieveMultiple(query);

            Assert.Single(result.Entities);
        }

        [Fact]
        public void QueryExpression_NoLock_DoesNotMutateCallerObject()
        {
            // The service must not side-effect NoLock on the caller's query object
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso" });

            var query = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
            Assert.False(query.NoLock); // SDK default

            service.RetrieveMultiple(query);

            // Must remain unchanged — service should not mutate the caller's query
            Assert.False(query.NoLock);
        }

        // ── FetchXml no-lock attribute parsing ──────────────────────────────

        [Fact]
        public void FetchXml_NoLockOmitted_DefaultsToTrue_ReturnsResults()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso" });

            // no-lock not specified — FetchXmlEvaluator defaults to NoLock = true
            var fetchXml = @"<fetch><entity name='account'><attribute name='name'/></entity></fetch>";
            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));

            Assert.Single(result.Entities);
        }

        [Fact]
        public void FetchXml_ExplicitNoLockTrue_ReturnsResults()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso" });

            var fetchXml = @"<fetch no-lock='true'><entity name='account'><attribute name='name'/></entity></fetch>";
            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));

            Assert.Single(result.Entities);
        }

        [Fact]
        public void FetchXml_ExplicitNoLockFalse_ReturnsResults()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso" });

            var fetchXml = @"<fetch no-lock='false'><entity name='account'><attribute name='name'/></entity></fetch>";
            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));

            Assert.Single(result.Entities);
        }

        // ── FetchXmlToQueryExpression conversion ────────────────────────────

        [Fact]
        public void FetchXmlToQueryExpression_NoLockTrue_SetsNoLock()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var response = (Microsoft.Crm.Sdk.Messages.FetchXmlToQueryExpressionResponse)service.Execute(
                new Microsoft.Crm.Sdk.Messages.FetchXmlToQueryExpressionRequest
                {
                    FetchXml = @"<fetch no-lock='true'><entity name='account'><attribute name='name'/></entity></fetch>"
                });

            Assert.True(response.Query.NoLock);
        }

        [Fact]
        public void FetchXmlToQueryExpression_NoLockFalse_SetsNoLockFalse()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var response = (Microsoft.Crm.Sdk.Messages.FetchXmlToQueryExpressionResponse)service.Execute(
                new Microsoft.Crm.Sdk.Messages.FetchXmlToQueryExpressionRequest
                {
                    FetchXml = @"<fetch no-lock='false'><entity name='account'><attribute name='name'/></entity></fetch>"
                });

            Assert.False(response.Query.NoLock);
        }

        [Fact]
        public void FetchXmlToQueryExpression_NoLockOmitted_SetsNoLockFalse()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var response = (Microsoft.Crm.Sdk.Messages.FetchXmlToQueryExpressionResponse)service.Execute(
                new Microsoft.Crm.Sdk.Messages.FetchXmlToQueryExpressionRequest
                {
                    FetchXml = @"<fetch><entity name='account'><attribute name='name'/></entity></fetch>"
                });

            // Conversion preserves the FetchXml spec default (omitted = false)
            Assert.False(response.Query.NoLock);
        }
    }
}