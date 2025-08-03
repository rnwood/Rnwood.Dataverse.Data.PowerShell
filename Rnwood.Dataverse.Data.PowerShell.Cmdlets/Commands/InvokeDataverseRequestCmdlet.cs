using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;

using Microsoft.Xrm.Sdk;
using Microsoft.PowerPlatform.Dataverse.Client;
using System.Collections;
using Newtonsoft.Json;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
	[Cmdlet("Invoke", "DataverseRequest")]
	///<summary>Invokes a Dataverse request.</summary>
	public class InvokeDataverseRequestCmdlet : OrganizationServiceCmdlet
	{
		[Parameter(Mandatory = true)]
		public override ServiceClient Connection { get; set; }

		[Parameter(ParameterSetName = "Request", Mandatory = true, HelpMessage = "Request to execute", ValueFromRemainingArguments = true, ValueFromPipeline = true)]
		public OrganizationRequest Request { get; set; }

		[Parameter(ParameterSetName = "NameAndInputs", Mandatory = true, Position = 0)]
		public string RequestName { get; set; }

		[Parameter(ParameterSetName = "NameAndInputs", Mandatory = false, Position = 1)]
		public Hashtable Parameters { get; set; } = new Hashtable();

		[Parameter(ParameterSetName = "REST", Mandatory = true, Position = 0)]
		public System.Net.Http.HttpMethod Method { get; set; }

		[Parameter(ParameterSetName = "REST", Mandatory = true, Position = 1)]
		public string Path { get; set; }

		[Parameter(ParameterSetName = "REST", Mandatory = false, Position = 2)]
		public PSObject Body { get; set; } = "";

		[Parameter(ParameterSetName = "REST", Mandatory = false)]
		public Hashtable CustomHeaders { get; set; } = new Hashtable();
		protected override void BeginProcessing()
		{
			base.BeginProcessing();
		}

		protected override void ProcessRecord()
		{
			base.ProcessRecord();

			if (ParameterSetName == "REST")
			{
				var headers = new Dictionary<string, List<string>>();
				foreach (DictionaryEntry kvp in CustomHeaders.Cast<DictionaryEntry>())
				{
					headers[(string)kvp.Key] = new List<string>(new[] { (string)kvp.Value });
				}

				string bodyString = "";
				if (Body != null)
				{
					if (Body.ImmediateBaseObject is string bs)
					{
						bodyString = bs;
					} else
					{
						bodyString = (string)InvokeCommand.NewScriptBlock("param($body); $body | ConvertTo-Json -Depth 100").Invoke(Body).First().ImmediateBaseObject;
					}
				}

				System.Net.Http.HttpResponseMessage response = Connection.ExecuteWebRequest(Method, Path, bodyString, headers);
				response.EnsureSuccessStatusCode();
				string responseBody = response.Content.ReadAsStringAsync().Result;
				var result = InvokeCommand.NewScriptBlock("param($response); $response | ConvertFrom-Json -Depth 100").Invoke(responseBody);
				WriteObject(result);
			}
			else
			{


				if (ParameterSetName == "NameAndInputs")
				{
					Request = new OrganizationRequest(RequestName);
					Request.Parameters.AddRange(Parameters.Cast<DictionaryEntry>().Select(e => new KeyValuePair<string, object>((string)e.Key, e.Value)));
				}

				WriteObject(Connection.Execute(Request));
			}
		}
	}
}
