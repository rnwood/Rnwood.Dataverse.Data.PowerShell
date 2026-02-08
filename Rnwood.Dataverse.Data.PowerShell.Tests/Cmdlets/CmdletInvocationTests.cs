using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using FluentAssertions;
using Microsoft.Xrm.Sdk;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets
{
    /// <summary>
    /// Tests to verify PowerShell cmdlet invocation works with Microsoft.PowerShell.SDK.
    /// This is critical for migrating Pester tests to xUnit.
    /// </summary>
    public class CmdletInvocationTests : TestBase
    {
        [Fact]
        public void PowerShell_Create_ReturnsNonNull()
        {
            // Act
            using var ps = PS.Create();

            // Assert
            ps.Should().NotBeNull();
        }

        [Fact]
        public void Can_Create_Runspace_With_Cmdlets()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            
            // Add our cmdlets
            var cmdletTypes = typeof(GetDataverseConnectionCmdlet).Assembly
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(PSCmdlet)) && !t.IsAbstract);
            
            foreach (var cmdletType in cmdletTypes)
            {
                // Get cmdlet attribute to find verb and noun
                var cmdletAttr = cmdletType.GetCustomAttributes(typeof(CmdletAttribute), false)
                    .FirstOrDefault() as CmdletAttribute;
                
                if (cmdletAttr != null)
                {
                    var cmdletName = $"{cmdletAttr.VerbName}-{cmdletAttr.NounName}";
                    initialSessionState.Commands.Add(
                        new SessionStateCmdletEntry(cmdletName, cmdletType, null));
                }
            }

            // Act
            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            
            using var ps = PS.Create();
            ps.Runspace = runspace;
            
            // Try to get available commands
            ps.AddCommand("Get-Command")
              .AddParameter("Name", "Get-DataverseConnection");
            
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            var command = results[0].BaseObject as CommandInfo;
            command.Should().NotBeNull();
            command!.Name.Should().Be("Get-DataverseConnection");
        }

        [Fact(Skip = "ServiceClient virtual method 'Execute' doesn't work with mock - use E2E tests for full cmdlet invocation testing")]
        public void Can_Invoke_GetDataverseRecord_With_FakeXrmEasy()
        {
            // Arrange - Create a runspace with our cmdlets
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseRecord", typeof(GetDataverseRecordCmdlet), null));
            
            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            
            using var ps = PS.Create();
            ps.Runspace = runspace;
            
            // Create mock connection
            var mockConnection = CreateMockConnection("contact");
            
            // Add test contact
            var testContact = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "John",
                ["lastname"] = "Doe"
            };
            Service!.Create(testContact);
            
            // Act - Try to invoke Get-DataverseRecord
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", testContact.Id);
            
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            var result = results[0];
            result.Should().NotBeNull();
            result.Properties["firstname"].Value.Should().Be("John");
            result.Properties["lastname"].Value.Should().Be("Doe");
        }
    }
}
