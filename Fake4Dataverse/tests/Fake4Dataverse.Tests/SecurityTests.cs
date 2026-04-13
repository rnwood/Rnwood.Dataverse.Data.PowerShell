using System;
using System.ServiceModel;
using Fake4Dataverse.Security;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

using PrivilegeDepth = Fake4Dataverse.Security.PrivilegeDepth;

namespace Fake4Dataverse.Tests
{
    public class SecurityTests
    {
        // ── 7.1 Security Context ──────────────────────────────────────

        [Fact]
        public void CallerId_DefaultIsNotEmpty()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            Assert.NotEqual(Guid.Empty, service.CallerId);
        }

        [Fact]
        public void CallerId_UsedForCreatedByModifiedBy()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var userId = Guid.NewGuid();
            service.CallerId = userId;

            var id = service.Create(new Entity("account") { ["name"] = "Test" });
            var entity = service.Retrieve("account", id, new ColumnSet(true));

            Assert.Equal(userId, entity.GetAttributeValue<EntityReference>("createdby").Id);
            Assert.Equal(userId, entity.GetAttributeValue<EntityReference>("modifiedby").Id);
        }

        [Fact]
        public void InitiatingUserId_DefaultsToCallerId()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            Assert.Equal(service.CallerId, service.InitiatingUserId);
        }

        [Fact]
        public void InitiatingUserId_CanDifferFromCallerId()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var impersonator = Guid.NewGuid();
            var actualUser = Guid.NewGuid();

            service.CallerId = actualUser;
            service.InitiatingUserId = impersonator;

            Assert.NotEqual(service.CallerId, service.InitiatingUserId);
            Assert.Equal(impersonator, service.InitiatingUserId);
            Assert.Equal(actualUser, service.CallerId);
        }

        [Fact]
        public void BusinessUnitId_HasDefault()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            Assert.NotEqual(Guid.Empty, service.BusinessUnitId);
        }

        [Fact]
        public void OrganizationId_HasDefault()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            Assert.NotEqual(Guid.Empty, env.OrganizationId);
        }

        [Fact]
        public void BusinessUnitId_IsConfigurable()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var buId = Guid.NewGuid();
            service.BusinessUnitId = buId;
            Assert.Equal(buId, service.BusinessUnitId);
        }

        [Fact]
        public void OrganizationId_IsConfigurable()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var orgId = Guid.NewGuid();
            env.OrganizationId = orgId;
            Assert.Equal(orgId, env.OrganizationId);
        }

        // ── 7.2 Security Roles & Privileges ──────────────────────────

        [Fact]
        public void SecurityEnforcement_DisabledByDefault()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            Assert.False(env.Security.EnforceSecurityRoles);
        }

        [Fact]
        public void Create_WithoutRole_SucceedsWhenEnforcementDisabled()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            // No roles, enforcement off — should work
            var id = service.Create(new Entity("account") { ["name"] = "Test" });
            Assert.NotEqual(Guid.Empty, id);
        }

        [Fact]
        public void Create_WithoutRole_ThrowsWhenEnforcementEnabled()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.Security.EnforceSecurityRoles = true;

            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Create(new Entity("account") { ["name"] = "Test" }));

            Assert.Equal(SecurityManager.PrivilegeDepthNotSatisfied, ex.Detail.ErrorCode);
            Assert.Contains("Create", ex.Detail.Message);
            Assert.Contains("account", ex.Detail.Message);
        }

        [Fact]
        public void Create_WithRole_SucceedsWhenEnforcementEnabled()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.Security.EnforceSecurityRoles = true;

            var role = new SecurityRole("Salesperson")
                .AddPrivilege("account", PrivilegeType.Create, PrivilegeDepth.Organization)
                .AddPrivilege("account", PrivilegeType.Read, PrivilegeDepth.Organization);

            env.Security.AssignRole(service.CallerId, role);

            var id = service.Create(new Entity("account") { ["name"] = "Test" });
            Assert.NotEqual(Guid.Empty, id);
        }

        [Fact]
        public void Retrieve_WithoutRole_ThrowsWhenEnforcementEnabled()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            // Create before enabling enforcement
            var id = service.Create(new Entity("account") { ["name"] = "Test" });

            env.Security.EnforceSecurityRoles = true;

            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Retrieve("account", id, new ColumnSet(true)));

            Assert.Equal(SecurityManager.PrivilegeDepthNotSatisfied, ex.Detail.ErrorCode);
            Assert.Contains("Read", ex.Detail.Message);
        }

        [Fact]
        public void Update_WithoutRole_ThrowsWhenEnforcementEnabled()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Test" });

            env.Security.EnforceSecurityRoles = true;
            var role = new SecurityRole("Reader")
                .AddPrivilege("account", PrivilegeType.Read, PrivilegeDepth.Organization);
            env.Security.AssignRole(service.CallerId, role);

            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Update(new Entity("account", id) { ["name"] = "Updated" }));

            Assert.Equal(SecurityManager.PrivilegeDepthNotSatisfied, ex.Detail.ErrorCode);
            Assert.Contains("Write", ex.Detail.Message);
        }

        [Fact]
        public void Delete_WithoutRole_ThrowsWhenEnforcementEnabled()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Test" });

            env.Security.EnforceSecurityRoles = true;

            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Delete("account", id));

            Assert.Equal(SecurityManager.PrivilegeDepthNotSatisfied, ex.Detail.ErrorCode);
            Assert.Contains("Delete", ex.Detail.Message);
        }

        [Fact]
        public void CrudWithFullRole_AllOperationsSucceed()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.Security.EnforceSecurityRoles = true;

            var role = new SecurityRole("System Administrator")
                .AddPrivilege("account", PrivilegeType.Create, PrivilegeDepth.Organization)
                .AddPrivilege("account", PrivilegeType.Read, PrivilegeDepth.Organization)
                .AddPrivilege("account", PrivilegeType.Write, PrivilegeDepth.Organization)
                .AddPrivilege("account", PrivilegeType.Delete, PrivilegeDepth.Organization);

            env.Security.AssignRole(service.CallerId, role);

            var id = service.Create(new Entity("account") { ["name"] = "Test" });
            var retrieved = service.Retrieve("account", id, new ColumnSet(true));
            Assert.Equal("Test", retrieved.GetAttributeValue<string>("name"));

            service.Update(new Entity("account", id) { ["name"] = "Updated" });
            retrieved = service.Retrieve("account", id, new ColumnSet(true));
            Assert.Equal("Updated", retrieved.GetAttributeValue<string>("name"));

            service.Delete("account", id);
        }

        [Fact]
        public void SecurityRole_CaseInsensitiveEntityName()
        {
            var role = new SecurityRole("Test Role")
                .AddPrivilege("Account", PrivilegeType.Read, PrivilegeDepth.Organization);

            Assert.Equal(PrivilegeDepth.Organization, role.GetDepth("account", PrivilegeType.Read));
            Assert.Equal(PrivilegeDepth.Organization, role.GetDepth("ACCOUNT", PrivilegeType.Read));
        }

        [Fact]
        public void SecurityRole_UnsetPrivilege_ReturnsNone()
        {
            var role = new SecurityRole("Test Role")
                .AddPrivilege("account", PrivilegeType.Read, PrivilegeDepth.Organization);

            Assert.Equal(PrivilegeDepth.None, role.GetDepth("account", PrivilegeType.Write));
            Assert.Equal(PrivilegeDepth.None, role.GetDepth("contact", PrivilegeType.Read));
        }

        [Fact]
        public void ClearRoles_RemovesAllRoles()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.Security.EnforceSecurityRoles = true;

            var role = new SecurityRole("Admin")
                .AddPrivilege("account", PrivilegeType.Create, PrivilegeDepth.Organization);
            env.Security.AssignRole(service.CallerId, role);

            // Should work
            var id = service.Create(new Entity("account") { ["name"] = "Test" });

            env.Security.ClearRoles(service.CallerId);

            // Should fail now
            Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Create(new Entity("account") { ["name"] = "Test2" }));
        }

        [Fact]
        public void MultipleRoles_CombinePrivileges()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.Security.EnforceSecurityRoles = true;

            var readerRole = new SecurityRole("Reader")
                .AddPrivilege("account", PrivilegeType.Read, PrivilegeDepth.Organization);
            var writerRole = new SecurityRole("Writer")
                .AddPrivilege("account", PrivilegeType.Create, PrivilegeDepth.Organization)
                .AddPrivilege("account", PrivilegeType.Write, PrivilegeDepth.Organization);

            env.Security.AssignRole(service.CallerId, readerRole);
            env.Security.AssignRole(service.CallerId, writerRole);

            var id = service.Create(new Entity("account") { ["name"] = "Test" });
            var retrieved = service.Retrieve("account", id, new ColumnSet(true));
            Assert.Equal("Test", retrieved.GetAttributeValue<string>("name"));
        }

        // ── 7.3 Record Ownership & Sharing ───────────────────────────

        [Fact]
        public void Create_AutoSetsOwnerId()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var userId = Guid.NewGuid();
            service.CallerId = userId;

            var id = service.Create(new Entity("account") { ["name"] = "Test" });
            var entity = service.Retrieve("account", id, new ColumnSet(true));

            var owner = entity.GetAttributeValue<EntityReference>("ownerid");
            Assert.NotNull(owner);
            Assert.Equal(userId, owner.Id);
            Assert.Equal("systemuser", owner.LogicalName);
        }

        [Fact]
        public void Create_ExplicitOwnerId_IsPreserved()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var teamId = Guid.NewGuid();

            var id = service.Create(new Entity("account")
            {
                ["name"] = "Test",
                ["ownerid"] = new EntityReference("team", teamId)
            });

            var entity = service.Retrieve("account", id, new ColumnSet(true));
            var owner = entity.GetAttributeValue<EntityReference>("ownerid");
            Assert.Equal(teamId, owner.Id);
            Assert.Equal("team", owner.LogicalName);
        }

        [Fact]
        public void GrantAccessRequest_SharesRecord()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var ownerId = service.CallerId;
            var id = service.Create(new Entity("account") { ["name"] = "Test" });

            var otherUser = Guid.NewGuid();

            service.Execute(new GrantAccessRequest
            {
                Target = new EntityReference("account", id),
                PrincipalAccess = new PrincipalAccess
                {
                    Principal = new EntityReference("systemuser", otherUser),
                    AccessMask = AccessRights.ReadAccess | AccessRights.WriteAccess
                }
            });

            var response = (RetrievePrincipalAccessResponse)service.Execute(new RetrievePrincipalAccessRequest
            {
                Target = new EntityReference("account", id),
                Principal = new EntityReference("systemuser", otherUser)
            });

            Assert.True(response.AccessRights.HasFlag(AccessRights.ReadAccess));
            Assert.True(response.AccessRights.HasFlag(AccessRights.WriteAccess));
            Assert.False(response.AccessRights.HasFlag(AccessRights.DeleteAccess));
        }

        [Fact]
        public void ModifyAccessRequest_ReplacesAccess()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Test" });
            var otherUser = Guid.NewGuid();

            // Grant read+write
            service.Execute(new GrantAccessRequest
            {
                Target = new EntityReference("account", id),
                PrincipalAccess = new PrincipalAccess
                {
                    Principal = new EntityReference("systemuser", otherUser),
                    AccessMask = AccessRights.ReadAccess | AccessRights.WriteAccess
                }
            });

            // Modify to read-only
            service.Execute(new ModifyAccessRequest
            {
                Target = new EntityReference("account", id),
                PrincipalAccess = new PrincipalAccess
                {
                    Principal = new EntityReference("systemuser", otherUser),
                    AccessMask = AccessRights.ReadAccess
                }
            });

            var response = (RetrievePrincipalAccessResponse)service.Execute(new RetrievePrincipalAccessRequest
            {
                Target = new EntityReference("account", id),
                Principal = new EntityReference("systemuser", otherUser)
            });

            Assert.True(response.AccessRights.HasFlag(AccessRights.ReadAccess));
            Assert.False(response.AccessRights.HasFlag(AccessRights.WriteAccess));
        }

        [Fact]
        public void RevokeAccessRequest_RemovesAccess()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Test" });
            var otherUser = Guid.NewGuid();

            // Grant first
            service.Execute(new GrantAccessRequest
            {
                Target = new EntityReference("account", id),
                PrincipalAccess = new PrincipalAccess
                {
                    Principal = new EntityReference("systemuser", otherUser),
                    AccessMask = AccessRights.ReadAccess | AccessRights.WriteAccess
                }
            });

            // Revoke
            service.Execute(new RevokeAccessRequest
            {
                Target = new EntityReference("account", id),
                Revokee = new EntityReference("systemuser", otherUser)
            });

            var response = (RetrievePrincipalAccessResponse)service.Execute(new RetrievePrincipalAccessRequest
            {
                Target = new EntityReference("account", id),
                Principal = new EntityReference("systemuser", otherUser)
            });

            Assert.Equal(AccessRights.None, response.AccessRights);
        }

        [Fact]
        public void RetrievePrincipalAccess_OwnerHasFullAccess()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var ownerId = service.CallerId;
            var id = service.Create(new Entity("account") { ["name"] = "Test" });

            var response = (RetrievePrincipalAccessResponse)service.Execute(new RetrievePrincipalAccessRequest
            {
                Target = new EntityReference("account", id),
                Principal = new EntityReference("systemuser", ownerId)
            });

            Assert.True(response.AccessRights.HasFlag(AccessRights.ReadAccess));
            Assert.True(response.AccessRights.HasFlag(AccessRights.WriteAccess));
            Assert.True(response.AccessRights.HasFlag(AccessRights.DeleteAccess));
            Assert.True(response.AccessRights.HasFlag(AccessRights.ShareAccess));
            Assert.True(response.AccessRights.HasFlag(AccessRights.AssignAccess));
        }

        [Fact]
        public void RetrievePrincipalAccess_NonOwnerNoSharing_HasNoAccess()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Test" });
            var otherUser = Guid.NewGuid();

            var response = (RetrievePrincipalAccessResponse)service.Execute(new RetrievePrincipalAccessRequest
            {
                Target = new EntityReference("account", id),
                Principal = new EntityReference("systemuser", otherUser)
            });

            Assert.Equal(AccessRights.None, response.AccessRights);
        }

        [Fact]
        public void GrantAccess_Cumulative()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Test" });
            var otherUser = Guid.NewGuid();

            // Grant read
            service.Execute(new GrantAccessRequest
            {
                Target = new EntityReference("account", id),
                PrincipalAccess = new PrincipalAccess
                {
                    Principal = new EntityReference("systemuser", otherUser),
                    AccessMask = AccessRights.ReadAccess
                }
            });

            // Grant write (should add to existing)
            service.Execute(new GrantAccessRequest
            {
                Target = new EntityReference("account", id),
                PrincipalAccess = new PrincipalAccess
                {
                    Principal = new EntityReference("systemuser", otherUser),
                    AccessMask = AccessRights.WriteAccess
                }
            });

            var response = (RetrievePrincipalAccessResponse)service.Execute(new RetrievePrincipalAccessRequest
            {
                Target = new EntityReference("account", id),
                Principal = new EntityReference("systemuser", otherUser)
            });

            Assert.True(response.AccessRights.HasFlag(AccessRights.ReadAccess));
            Assert.True(response.AccessRights.HasFlag(AccessRights.WriteAccess));
        }

        [Fact]
        public void RetrievePrincipalAccess_RoleBasedAccess_IncludedInResult()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Test" });
            var otherUser = Guid.NewGuid();

            var role = new SecurityRole("Reader")
                .AddPrivilege("account", PrivilegeType.Read, PrivilegeDepth.Organization);
            env.Security.AssignRole(otherUser, role);

            var response = (RetrievePrincipalAccessResponse)service.Execute(new RetrievePrincipalAccessRequest
            {
                Target = new EntityReference("account", id),
                Principal = new EntityReference("systemuser", otherUser)
            });

            Assert.True(response.AccessRights.HasFlag(AccessRights.ReadAccess));
            Assert.False(response.AccessRights.HasFlag(AccessRights.WriteAccess));
        }

        [Fact]
        public void Assign_ChangesOwner()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var newOwner = Guid.NewGuid();
            var id = service.Create(new Entity("account") { ["name"] = "Test" });

            service.Execute(new AssignRequest
            {
                Target = new EntityReference("account", id),
                Assignee = new EntityReference("systemuser", newOwner)
            });

            var entity = service.Retrieve("account", id, new ColumnSet("ownerid"));
            Assert.Equal(newOwner, entity.GetAttributeValue<EntityReference>("ownerid").Id);
        }

        // ── 7.4 Team Membership Security ──────────────────────────────

        [Fact]
        public void TeamMember_InheritsTeamRole_CanPerformOperation()
        {
            var env = new FakeDataverseEnvironment(FakeOrganizationServiceOptions.Strict);
            var service = env.CreateOrganizationService();
            var teamId = Guid.NewGuid();
            var userId = service.CallerId;

            // Create a role with Create privilege on account
            var role = new SecurityRole("TeamRole")
                .AddPrivilege("account", PrivilegeType.Create, PrivilegeDepth.Organization)
                .AddPrivilege("account", PrivilegeType.Read, PrivilegeDepth.Organization);

            // Assign role to team, add user to team
            env.Security.AssignTeamRole(teamId, role);
            env.Security.AddTeamMember(teamId, userId);

            // User should be able to create via team role
            var id = service.Create(new Entity("account") { ["name"] = "TeamCreated" });
            Assert.NotEqual(Guid.Empty, id);
        }

        [Fact]
        public void TeamMember_WithoutTeamRole_CannotPerformOperation()
        {
            var env = new FakeDataverseEnvironment(FakeOrganizationServiceOptions.Strict);
            var service = env.CreateOrganizationService();
            var teamId = Guid.NewGuid();
            var userId = service.CallerId;

            // Team has no roles
            env.Security.AddTeamMember(teamId, userId);

            Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Create(new Entity("account") { ["name"] = "NoRoleTeam" }));
        }

        [Fact]
        public void AccessTeam_GrantsRecordAccess_ToTeamMembers()
        {
            var env = new FakeDataverseEnvironment(FakeOrganizationServiceOptions.Strict);
            var service = env.CreateOrganizationService();
            var ownerId = Guid.NewGuid();
            var teamId = Guid.NewGuid();
            var userId = service.CallerId;

            // Give owner role, create entity as owner
            var ownerRole = new SecurityRole("Owner")
                .AddPrivilege("account", PrivilegeType.Create, PrivilegeDepth.Organization)
                .AddPrivilege("account", PrivilegeType.Read, PrivilegeDepth.Organization);
            env.Security.AssignRole(ownerId, ownerRole);

            // Give user minimal read through team
            var readRole = new SecurityRole("Reader")
                .AddPrivilege("account", PrivilegeType.Read, PrivilegeDepth.Organization);
            env.Security.AssignRole(userId, readRole);

            // Add user to access team
            env.Security.AddTeamMember(teamId, userId);

            // Create entity as owner
            service.CallerId = ownerId;
            var id = service.Create(new Entity("account") { ["name"] = "SharedViaTeam" });

            // Grant team access
            env.Security.GrantTeamAccess("account", id, teamId, AccessRights.ReadAccess);

            // Switch to user, should be able to read
            service.CallerId = userId;
            var retrieved = service.Retrieve("account", id, new ColumnSet("name"));
            Assert.Equal("SharedViaTeam", retrieved.GetAttributeValue<string>("name"));
        }

        [Fact]
        public void RemoveTeamMember_LosesTeamRole()
        {
            var env = new FakeDataverseEnvironment(FakeOrganizationServiceOptions.Strict);
            var service = env.CreateOrganizationService();
            var teamId = Guid.NewGuid();
            var userId = service.CallerId;

            var role = new SecurityRole("TeamRole")
                .AddPrivilege("account", PrivilegeType.Create, PrivilegeDepth.Organization);
            env.Security.AssignTeamRole(teamId, role);
            env.Security.AddTeamMember(teamId, userId);

            // Remove from team
            env.Security.RemoveTeamMember(teamId, userId);

            Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Create(new Entity("account") { ["name"] = "ShouldFail" }));
        }

        // ── 7.5 System Context ────────────────────────────────────────

        [Fact]
        public void UseSystemContext_BypassesSecurityChecks()
        {
            var env = new FakeDataverseEnvironment(FakeOrganizationServiceOptions.Strict);
            var service = env.CreateOrganizationService();
            // No roles assigned, but system context bypasses checks
            service.UseSystemContext = true;

            var id = service.Create(new Entity("account") { ["name"] = "SystemCreated" });
            Assert.NotEqual(Guid.Empty, id);

            var retrieved = service.Retrieve("account", id, new ColumnSet("name"));
            Assert.Equal("SystemCreated", retrieved.GetAttributeValue<string>("name"));
        }

        [Fact]
        public void UseSystemContext_False_EnforcesSecurityChecks()
        {
            var env = new FakeDataverseEnvironment(FakeOrganizationServiceOptions.Strict);
            var service = env.CreateOrganizationService();
            service.UseSystemContext = false;

            Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Create(new Entity("account") { ["name"] = "ShouldFail" }));
        }

        [Fact]
        public void RetrieveSharedPrincipalsAndAccess_ReturnsSharedPrincipals()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var accountId = service.Create(new Entity("account") { ["name"] = "Shared" });

            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();
            service.Execute(new GrantAccessRequest
            {
                Target = new EntityReference("account", accountId),
                PrincipalAccess = new PrincipalAccess
                {
                    Principal = new EntityReference("systemuser", userId1),
                    AccessMask = AccessRights.ReadAccess | AccessRights.WriteAccess
                }
            });
            service.Execute(new GrantAccessRequest
            {
                Target = new EntityReference("account", accountId),
                PrincipalAccess = new PrincipalAccess
                {
                    Principal = new EntityReference("systemuser", userId2),
                    AccessMask = AccessRights.ReadAccess
                }
            });

            var request = new OrganizationRequest("RetrieveSharedPrincipalsAndAccess");
            request["Target"] = new EntityReference("account", accountId);
            var response = service.Execute(request);
            var principals = (PrincipalAccess[])response["PrincipalAccesses"];

            Assert.Equal(2, principals.Length);
        }

        [Fact]
        public void RetrieveUserPrivileges_ReturnsAssignedRolePrivileges()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var userId = service.CallerId;
            var role = new SecurityRole("Sales")
                .AddPrivilege("*", PrivilegeType.Read, PrivilegeDepth.Organization);
            env.Security.AssignRole(userId, role);

            var request = new OrganizationRequest("RetrieveUserPrivileges");
            request["UserId"] = userId;
            var response = service.Execute(request);
            var privileges = (RolePrivilege[])response["RolePrivileges"];

            Assert.NotEmpty(privileges);
        }
    }
}
