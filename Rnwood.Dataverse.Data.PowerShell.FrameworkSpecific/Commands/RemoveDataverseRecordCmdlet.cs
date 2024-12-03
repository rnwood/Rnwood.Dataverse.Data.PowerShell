


using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Windows.Markup;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
	[Cmdlet(VerbsCommon.Remove, "DataverseRecord", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
	///<summary>Deletes records from a Dataverse organization.</summary>
	public class RemoveDataverseRecordCmdlet : OrganizationServiceCmdlet
	{
		[Parameter(Mandatory = true)]
		public override ServiceClient Connection { get; set; }

		[Parameter(ValueFromPipeline = true)]
		public PSObject InputObject { get; set; }

		[Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of table")]
		[Alias("EntityName")]
		public string TableName { get; set; }

		[Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Id of record to process")]
		public Guid Id { get; set; }

		[Parameter()]
		public uint BatchSize { get; set; } = 100;

		private class BatchItem
		{
			public BatchItem(PSObject inputObject, OrganizationRequest request, Action<OrganizationResponse> responseCompletion) : this(inputObject, request, responseCompletion, null)
			{
			}

			public BatchItem(PSObject inputObject, OrganizationRequest request, Action<OrganizationResponse> responseCompletion, Func<OrganizationServiceFault, bool> responseExceptionCompletion)
			{
				InputObject = inputObject;
				Request = request;
				ResponseCompletion = responseCompletion;
				ResponseExceptionCompletion = responseExceptionCompletion;
			}

			public OrganizationRequest Request { get; set; }

			public Action<OrganizationResponse> ResponseCompletion { get; set; }

			public Func<OrganizationServiceFault, bool> ResponseExceptionCompletion { get; set; }

			public PSObject InputObject { get; set; }

			public override string ToString()
			{
				return Request.RequestName + " " + string.Join(", ", Request.Parameters.Select(p => $"{p.Key}='{FormatValue(p.Value)}'"));
			}

			private object FormatValue(object value)
			{
				return value is EntityReference er ? $"{er.LogicalName}:{er.Id}" : value?.ToString();
			}
		}

		private List<BatchItem> _nextBatchItems;

		protected override void ProcessRecord()
		{
			base.ProcessRecord();

			Delete(TableName, Id);
		}

		private void Delete(string entityName, Guid id)
		{
			EntityMetadata metadata = metadataFactory.GetMetadata(entityName);

			if (metadata.IsIntersect.GetValueOrDefault())
			{
				if (ShouldProcess(string.Format("Delete intersect record {0}:{1}", entityName, id)))
				{
					ManyToManyRelationshipMetadata manyToManyRelationshipMetadata = metadata.ManyToManyRelationships[0];

					QueryExpression getRecordWithMMColumns = new QueryExpression(TableName);
					getRecordWithMMColumns.ColumnSet = new ColumnSet(manyToManyRelationshipMetadata.Entity1IntersectAttribute, manyToManyRelationshipMetadata.Entity2IntersectAttribute);
					getRecordWithMMColumns.Criteria.AddCondition(metadata.PrimaryIdAttribute, ConditionOperator.Equal, id);

					Entity record = Connection.RetrieveMultiple(getRecordWithMMColumns).Entities.Single();

					Connection.Execute(new DisassociateRequest()
					{
						Target = new EntityReference(manyToManyRelationshipMetadata.Entity1LogicalName,
													 record.GetAttributeValue<Guid>(
														 manyToManyRelationshipMetadata.Entity1IntersectAttribute)),
						RelatedEntities =
							new EntityReferenceCollection()
								{
									new EntityReference(manyToManyRelationshipMetadata.Entity2LogicalName,
														record.GetAttributeValue<Guid>(
															manyToManyRelationshipMetadata.Entity2IntersectAttribute))
								},
						Relationship = new Relationship(manyToManyRelationshipMetadata.SchemaName) { PrimaryEntityRole = EntityRole.Referencing }
					});

					WriteVerbose(string.Format("Deleted intersect record {0}:{1}", entityName, id));
				}
			}
			else
			{
				DeleteRequest request = new DeleteRequest { Target = new EntityReference(entityName, id) };

				if (_nextBatchItems != null)
				{
					WriteVerbose(string.Format("Added delete of {0}:{1} to batch", entityName, id));
					QueueBatchItem(new BatchItem(InputObject, request, (response) => { DeleteCompletion(InputObject, entityName, id, (DeleteResponse)response); }));
				}
				else
				{ 
					if (ShouldProcess(string.Format("Delete record {0}:{1}", entityName, id)))
					{
						Connection.Delete(entityName, id);
					}
				}
			}
		}

		private void DeleteCompletion(PSObject inputObject, string entityName, Guid id , DeleteResponse response)
		{
			WriteVerbose(string.Format("Deleted record {0}:{1}", entityName, id));
		}

		private void QueueBatchItem(BatchItem item)
		{
			_nextBatchItems.Add(item);

			if (_nextBatchItems.Count == BatchSize)
			{
				ProcessBatch();
			}
		}
		private void AppendFaultDetails(OrganizationServiceFault fault, StringBuilder output)
		{
			output.AppendLine("OrganizationServiceFault " + fault.ErrorCode + ": " + fault.Message);
			output.AppendLine(fault.TraceText);

			if (fault.InnerFault != null)
			{
				output.AppendLine("---");
				AppendFaultDetails(fault.InnerFault, output);
			}
		}


		private void ProcessBatch()
		{
			if (_nextBatchItems.Count > 0 &&
				ShouldProcess("Execute batch of requests:\n" + string.Join("\n", _nextBatchItems.Select(r => r.ToString()))))
			{

				ExecuteMultipleRequest batchRequest = new ExecuteMultipleRequest()
				{
					Settings = new ExecuteMultipleSettings()
					{
						ReturnResponses = true,
						ContinueOnError = true
					},
					Requests = new OrganizationRequestCollection(),
					RequestId = Guid.NewGuid()
				};

				batchRequest.Requests.AddRange(_nextBatchItems.Select(i => i.Request));

				ExecuteMultipleResponse response = (ExecuteMultipleResponse)Connection.Execute(batchRequest);

				foreach (var itemResponse in response.Responses)
				{
					BatchItem batchItem = _nextBatchItems[itemResponse.RequestIndex];

					if (itemResponse.Fault != null)
					{
						if (batchItem.ResponseExceptionCompletion != null && batchItem.ResponseExceptionCompletion(itemResponse.Fault))
						{
							//Handled error
						}
						else
						{
							StringBuilder details = new StringBuilder();
							AppendFaultDetails(itemResponse.Fault, details);

							WriteError(new ErrorRecord(new Exception(details.ToString()), null, ErrorCategory.InvalidResult, batchItem.InputObject));
						}
					}
					else
					{
						batchItem.ResponseCompletion(itemResponse.Response);
					}
				}


			}

			_nextBatchItems.Clear();
		}

		protected override void BeginProcessing()
		{
			base.BeginProcessing();

			metadataFactory = new EntityMetadataFactory(Connection);

			if (BatchSize > 1)
			{
				_nextBatchItems = new List<BatchItem>();
			}
		}

		private EntityMetadataFactory metadataFactory;

		protected override void EndProcessing()
		{
			base.EndProcessing();

			if (_nextBatchItems != null)
			{
				ProcessBatch();
			}
		}
	}
}