using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class MembershipRequestTests
    {
        [Fact]
        public void AddMembersTeamRequest_AddsTeamMemberships()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var teamId = service.Create(new Entity("team") { ["name"] = "TestTeam" });
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();

            service.Execute(new AddMembersTeamRequest
            {
                TeamId = teamId,
                MemberIds = new[] { userId1, userId2 }
            });

            var result = service.RetrieveMultiple(new QueryExpression("teammembership") { ColumnSet = new ColumnSet(true) });
            Assert.Equal(2, result.Entities.Count);
        }

        [Fact]
        public void RemoveMembersTeamRequest_RemovesTeamMemberships()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var teamId = service.Create(new Entity("team") { ["name"] = "TestTeam" });
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();

            service.Execute(new AddMembersTeamRequest
            {
                TeamId = teamId,
                MemberIds = new[] { userId1, userId2 }
            });

            service.Execute(new RemoveMembersTeamRequest
            {
                TeamId = teamId,
                MemberIds = new[] { userId1 }
            });

            var result = service.RetrieveMultiple(new QueryExpression("teammembership") { ColumnSet = new ColumnSet(true) });
            Assert.Single(result.Entities);
        }

        [Fact]
        public void AddListMembersListRequest_AddsListMembers()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var listId = service.Create(new Entity("list") { ["listname"] = "TestList" });
            var memberId1 = Guid.NewGuid();
            var memberId2 = Guid.NewGuid();

            service.Execute(new AddListMembersListRequest
            {
                ListId = listId,
                MemberIds = new[] { memberId1, memberId2 }
            });

            var result = service.RetrieveMultiple(new QueryExpression("listmember") { ColumnSet = new ColumnSet(true) });
            Assert.Equal(2, result.Entities.Count);
        }

        [Fact]
        public void RemoveMemberListRequest_RemovesListMember()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var listId = service.Create(new Entity("list") { ["listname"] = "TestList" });
            var memberId = Guid.NewGuid();

            service.Execute(new AddListMembersListRequest
            {
                ListId = listId,
                MemberIds = new[] { memberId }
            });

            var members = service.RetrieveMultiple(new QueryExpression("listmember") { ColumnSet = new ColumnSet(true) });
            Assert.Single(members.Entities);

            service.Execute(new RemoveMemberListRequest
            {
                ListId = listId,
                EntityId = members.Entities[0].Id
            });

            var remaining = service.RetrieveMultiple(new QueryExpression("listmember") { ColumnSet = new ColumnSet(true) });
            Assert.Empty(remaining.Entities);
        }

        [Fact]
        public void RetrieveMembersTeam_ReturnsTeamMembers()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var teamId = service.Create(new Entity("team") { ["name"] = "Dev" });
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();

            service.Execute(new AddMembersTeamRequest
            {
                TeamId = teamId,
                MemberIds = new[] { userId1, userId2 }
            });

            var request = new OrganizationRequest("RetrieveMembersTeam");
            request["EntityId"] = teamId;
            var response = service.Execute(request);
            var collection = (EntityCollection)response["EntityCollection"];

            Assert.Equal(2, collection.Entities.Count);
        }

        [Fact]
        public void AddUserToRecordTeam_CreatesTeamMembership()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var teamId = service.Create(new Entity("team") { ["name"] = "Access Team" });
            var userId = Guid.NewGuid();
            var accountId = service.Create(new Entity("account") { ["name"] = "Target" });

            var request = new OrganizationRequest("AddUserToRecordTeam");
            request["TeamId"] = teamId;
            request["SystemUserId"] = userId;
            request["Record"] = new EntityReference("account", accountId);
            var response = service.Execute(request);

            Assert.Equal(teamId, (Guid)response["AccessTeamId"]);
            var memberships = service.RetrieveMultiple(new QueryExpression("teammembership") { ColumnSet = new ColumnSet(true) });
            Assert.Single(memberships.Entities);
        }

        [Fact]
        public void RemoveUserFromRecordTeam_RemovesMembership()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var teamId = service.Create(new Entity("team") { ["name"] = "Access Team" });
            var userId = Guid.NewGuid();

            service.Execute(new AddMembersTeamRequest
            {
                TeamId = teamId,
                MemberIds = new[] { userId }
            });

            var request = new OrganizationRequest("RemoveUserFromRecordTeam");
            request["TeamId"] = teamId;
            request["SystemUserId"] = userId;
            request["Record"] = new EntityReference("account", Guid.NewGuid());
            service.Execute(request);

            var memberships = service.RetrieveMultiple(new QueryExpression("teammembership") { ColumnSet = new ColumnSet(true) });
            Assert.Empty(memberships.Entities);
        }
    }
}
