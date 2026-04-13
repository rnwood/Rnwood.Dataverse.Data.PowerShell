# Security & Access Control

## Overview

Fake4Dataverse includes a built-in security layer that mirrors the Dataverse security model — roles, privileges, record sharing, and teams. Security enforcement is **off by default**, so existing tests continue to work without configuration. Enable it when you need to verify that your code respects privilege checks.

The `SecurityManager` is accessible via `env.Security`.

```csharp
using Fake4Dataverse;
using Fake4Dataverse.Security;
```

## Enabling Security

Turn on enforcement explicitly or use the `Strict` preset (which also enables metadata validation):

```csharp
// Option 1: Strict preset (security + metadata validation)
var env = new FakeDataverseEnvironment(FakeOrganizationServiceOptions.Strict);
var service = env.CreateOrganizationService();

// Option 2: Enable security on an existing environment
var env = new FakeDataverseEnvironment();
env.Security.EnforceSecurityRoles = true;
var service = env.CreateOrganizationService();
```

When enforcement is enabled, every Create, Retrieve, Update, and Delete call checks whether the caller has the required privilege. If not, it throws a `FaultException<OrganizationServiceFault>` with error code `0x80040220` — exactly what Dataverse does.

## Security Roles

A `SecurityRole` is a named collection of entity-level privileges. Build roles with the fluent API:

```csharp
var salesRep = new SecurityRole("Sales Rep")
    .AddPrivilege("account", PrivilegeType.Create, PrivilegeDepth.User)
    .AddPrivilege("account", PrivilegeType.Read,   PrivilegeDepth.Organization)
    .AddPrivilege("account", PrivilegeType.Write,  PrivilegeDepth.User)
    .AddPrivilege("contact", PrivilegeType.Read,   PrivilegeDepth.Organization)
    .AddPrivilege("contact", PrivilegeType.Create, PrivilegeDepth.User);

// Query a role's depth for a specific privilege
PrivilegeDepth depth = salesRep.GetDepth("account", PrivilegeType.Delete);
// Returns PrivilegeDepth.None — not granted
```

### Privilege Types

| Value | Description |
|---|---|
| `Create` | Permission to create records |
| `Read` | Permission to read records |
| `Write` | Permission to update records |
| `Delete` | Permission to delete records |
| `Append` | Permission to append a record to another |
| `AppendTo` | Permission to have a record appended to this entity |
| `Share` | Permission to share records |
| `Assign` | Permission to assign records |

### Privilege Depths

| Value | Int | Description |
|---|---|---|
| `None` | 0 | No access |
| `User` | 1 | User-owned records only |
| `BusinessUnit` | 2 | Same business unit |
| `ParentChildBusinessUnit` | 3 | Same BU and child BUs |
| `Organization` | 4 | All records in the organization |

## Assigning Roles to Users

```csharp
var userId = Guid.NewGuid();
service.CallerId = userId;

env.Security.AssignRole(userId, salesRep);

// Remove all roles from a user
env.Security.ClearRoles(userId);
```

## Privilege Enforcement

When security is enabled, the service automatically checks privileges on every operation. You can also invoke checks directly:

```csharp
// Throws FaultException<OrganizationServiceFault> if privilege is missing
env.Security.CheckPrivilege(userId, "account", PrivilegeType.Create);

// Record-level check — considers ownership and sharing
env.Security.CheckRecordPrivilege(
    userId, "account", accountId, PrivilegeType.Write, ownerId);
```

Owners always have access to their own records regardless of role configuration.

### Example: Verifying Denied Access

```csharp
[Fact]
public void Create_WithoutRole_Throws()
{
    var env = new FakeDataverseEnvironment();
    env.Security.EnforceSecurityRoles = true;
    var service = env.CreateOrganizationService();

    var userId = Guid.NewGuid();
    service.CallerId = userId;
    // No roles assigned — every operation is denied

    var account = new Entity("account") { ["name"] = "Contoso" };

    var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(
        () => service.Create(account));

    Assert.Contains("missing Create privilege", ex.Message);
}
```

### Example: Verifying Allowed Access

```csharp
[Fact]
public void Create_WithRole_Succeeds()
{
    var env = new FakeDataverseEnvironment();
    env.Security.EnforceSecurityRoles = true;
    var service = env.CreateOrganizationService();

    var userId = Guid.NewGuid();
    service.CallerId = userId;

    var role = new SecurityRole("Account Creator")
        .AddPrivilege("account", PrivilegeType.Create, PrivilegeDepth.Organization);
    env.Security.AssignRole(userId, role);

    var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

    Assert.NotEqual(Guid.Empty, id);
}
```

## Record Sharing

Share individual records with specific users to grant access beyond their role privileges:

```csharp
var recordId = service.Create(new Entity("account") { ["name"] = "Shared Inc." });

// Grant read + write access to another user
env.Security.GrantAccess("account", recordId, otherUserId,
    AccessRights.ReadAccess | AccessRights.WriteAccess);

// Replace access rights entirely
env.Security.ModifyAccess("account", recordId, otherUserId,
    AccessRights.ReadAccess);

// Remove all shared access
env.Security.RevokeAccess("account", recordId, otherUserId);

// Query effective access (combines ownership, roles, and sharing)
AccessRights rights = env.Security.RetrievePrincipalAccess(
    "account", recordId, otherUserId, ownerId: service.CallerId);
```

### Sharing via Execute Requests

You can also share records through `OrganizationRequest` messages, matching how production code typically works:

```csharp
service.Execute(new GrantAccessRequest
{
    Target = new EntityReference("account", recordId),
    PrincipalAccess = new PrincipalAccess
    {
        Principal = new EntityReference("systemuser", otherUserId),
        AccessMask = AccessRights.ReadAccess | AccessRights.WriteAccess
    }
});
```

`ModifyAccessRequest`, `RevokeAccessRequest`, and `RetrievePrincipalAccessRequest` work the same way.

### Example: Testing Shared Access

```csharp
[Fact]
public void SharedUser_CanReadRecord()
{
    var env = new FakeDataverseEnvironment();
    env.Security.EnforceSecurityRoles = true;

    var ownerId = Guid.NewGuid();
    var service = env.CreateOrganizationService(ownerId);

    // Owner has full access via a role
    var ownerRole = new SecurityRole("Owner Role")
        .AddPrivilege("account", PrivilegeType.Create, PrivilegeDepth.Organization)
        .AddPrivilege("account", PrivilegeType.Read,   PrivilegeDepth.Organization)
        .AddPrivilege("account", PrivilegeType.Share,  PrivilegeDepth.Organization);
    env.Security.AssignRole(ownerId, ownerRole);

    var accountId = service.Create(new Entity("account") { ["name"] = "Shared Corp" });

    // Share with a second user who has no roles
    var readerId = Guid.NewGuid();
    env.Security.GrantAccess("account", accountId, readerId, AccessRights.ReadAccess);

    // Create a session for the reader — reader can now retrieve the shared record
    var readerService = env.CreateOrganizationService(readerId);
    var result = readerService.Retrieve("account", accountId, new ColumnSet("name"));

    Assert.Equal("Shared Corp", result["name"]);
}
```

## Team Security

Teams let you manage privileges collectively. Members inherit the team's assigned roles.

```csharp
var teamId = Guid.NewGuid();
var memberA = Guid.NewGuid();
var memberB = Guid.NewGuid();

// Build the team
env.Security.AddTeamMember(teamId, memberA);
env.Security.AddTeamMember(teamId, memberB);

// Assign a role to the team — both members inherit it
var teamRole = new SecurityRole("Support Team")
    .AddPrivilege("incident", PrivilegeType.Read,  PrivilegeDepth.Organization)
    .AddPrivilege("incident", PrivilegeType.Write, PrivilegeDepth.BusinessUnit);
env.Security.AssignTeamRole(teamId, teamRole);

// Grant record-level access to all team members at once
env.Security.GrantTeamAccess("incident", caseId, teamId,
    AccessRights.ReadAccess | AccessRights.WriteAccess);

// Remove a member
env.Security.RemoveTeamMember(teamId, memberB);
```

### Example: Team-Based Access

```csharp
[Fact]
public void TeamMember_InheritsTeamRole()
{
    var env = new FakeDataverseEnvironment();
    env.Security.EnforceSecurityRoles = true;

    var teamId = Guid.NewGuid();
    var userId = Guid.NewGuid();
    env.Security.AddTeamMember(teamId, userId);

    var role = new SecurityRole("Case Handlers")
        .AddPrivilege("incident", PrivilegeType.Create, PrivilegeDepth.Organization)
        .AddPrivilege("incident", PrivilegeType.Read,   PrivilegeDepth.Organization);
    env.Security.AssignTeamRole(teamId, role);

    var service = env.CreateOrganizationService(userId);
    var id = service.Create(new Entity("incident") { ["title"] = "Broken widget" });

    Assert.NotEqual(Guid.Empty, id);
}
```

## System Context (Bypass)

Set `UseSystemContext` to bypass all security checks — useful when your code has a legitimate system-context path (e.g., plugin code running under the SYSTEM user):

```csharp
service.UseSystemContext = true;

// This succeeds regardless of roles or sharing
service.Create(new Entity("account") { ["name"] = "System Record" });

service.UseSystemContext = false; // Re-enable enforcement
```

## Identity Properties

Control who the "current user" is for security and audit purposes:

| Property | Default | Description |
|---|---|---|
| `CallerId` | `00000000-...01` | The user performing operations; checked against roles |
| `InitiatingUserId` | Same as `CallerId` | The original caller in impersonation scenarios |
| `BusinessUnitId` | `00000000-...03` | The caller's business unit |

```csharp
// Simulate a different user
service.CallerId = salesUserId;

// Simulate impersonation (plugin running as system, initiated by a user)
service.CallerId = systemUserId;
service.InitiatingUserId = actualUserId;
```

## Common Testing Patterns

### Pattern 1: Least-Privilege Verification

Confirm that code fails without the right privilege, then succeeds with it:

```csharp
[Fact]
public void DeleteAccount_RequiresDeletePrivilege()
{
    var env = new FakeDataverseEnvironment();
    env.Security.EnforceSecurityRoles = true;

    var userId = Guid.NewGuid();
    var service = env.CreateOrganizationService(userId);

    // Read-only role — no delete
    var readOnly = new SecurityRole("Read Only")
        .AddPrivilege("account", PrivilegeType.Create, PrivilegeDepth.Organization)
        .AddPrivilege("account", PrivilegeType.Read,   PrivilegeDepth.Organization);
    env.Security.AssignRole(userId, readOnly);

    var id = service.Create(new Entity("account") { ["name"] = "Test" });

    Assert.Throws<FaultException<OrganizationServiceFault>>(
        () => service.Delete("account", id));
}
```

### Pattern 2: Role Escalation Test

Verify that adding a role dynamically grants access:

```csharp
[Fact]
public void AddingRole_GrantsAccess()
{
    var env = new FakeDataverseEnvironment();
    env.Security.EnforceSecurityRoles = true;

    var userId = Guid.NewGuid();
    var service = env.CreateOrganizationService(userId);

    // Starts with no roles — denied
    Assert.Throws<FaultException<OrganizationServiceFault>>(
        () => service.Create(new Entity("contact")));

    // Add a role — now allowed
    var role = new SecurityRole("Contact Manager")
        .AddPrivilege("contact", PrivilegeType.Create, PrivilegeDepth.User);
    env.Security.AssignRole(userId, role);

    var id = service.Create(new Entity("contact") { ["lastname"] = "Doe" });
    Assert.NotEqual(Guid.Empty, id);
}
```
