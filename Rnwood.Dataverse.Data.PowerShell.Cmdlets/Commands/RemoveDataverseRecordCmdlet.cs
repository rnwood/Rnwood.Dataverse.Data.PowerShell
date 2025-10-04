


using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.ServiceModel;
using System.Text;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
	///<summary>Deletes records from a Dataverse organization.</summary>
	[Cmdlet(VerbsCommon.Remove, "DataverseRecord", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
	public class RemoveDataverseRecordCmdlet : CustomLogicBypassableOrganizationServiceCmdlet
	{
		/// <summary>
		/// DataverseConnection instance obtained from Get-DataverseConnection cmdlet
		/// </summary>
		[Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
		public override ServiceClient Connection { get; set; }
		/// <summary>
		/// Record from pipeline. This allows piping in record to delete.
		/// </summary>
		[Parameter(ValueFromPipeline = true, HelpMessage = "Record from pipeline. This allows piping in record to delete.")]
		public PSObject InputObject { get; set; }
	/// <summary>
	/// The logical name of the table to operate on.
	/// </summary>
	[Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of table")]
	[Alias("EntityName")]
	[ArgumentCompleter(typeof(TableNameArgumentCompleter))]
	public string TableName { get; set; }
		/// <summary>
		/// Id of record to process
		/// </summary>
		[Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Id of record to process")]
		public Guid Id { get; set; }
		/// <summary>
		/// Controls the maximum number of requests sent to Dataverse in one batch (where possible) to improve throughput. Specify 1 to disable. When value is 1, requests are sent to Dataverse one request at a time. When > 1, batching is used. Note that the batch will continue on error and any errors will be returned once the batch has completed. The error contains the input record to allow correlation.
		/// </summary>
		[Parameter(HelpMessage = "Controls the maximum number of requests sent to Dataverse in one batch (where possible) to improve throughput. Specify 1 to disable. When value is 1, requests are sent to Dataverse one request at a time. When > 1, batching is used. Note that the batch will continue on error and any errors will be returned once the batch has completed. The error contains the input record to allow correlation.")]
		public uint BatchSize { get; set; } = 100;
		/// <summary>
		/// If specified, the cmdlet will not raise an error if the record does not exist.
		/// </summary>
		[Parameter(HelpMessage = "If specified, the cmdlet will not raise an error if the record does not exist.")]
		public SwitchParameter IfExists { get; set; }
		/// <summary>
		/// Specifies the types of business logic (for example plugins) to bypass
		/// </summary>
		[Parameter(HelpMessage = "Specifies the types of business logic (for example plugins) to bypass")]
		public override BusinessLogicTypes[] BypassBusinessLogicExecution { get; set; }

		/// <summary>
		/// Specifies the IDs of plugin steps to bypass
		/// </summary>
		[Parameter(HelpMessage = "Specifies the IDs of plugin steps to bypass")]
		public override Guid[] BypassBusinessLogicExecutionStepIds { get; set; }

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

		/// <summary>


		/// Processes each record in the pipeline.


		/// </summary>


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

					DisassociateRequest request = new DisassociateRequest()
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
					};
					ApplyBypassBusinessLogicExecution(request);
					Connection.Execute(request);

					WriteVerbose(string.Format("Deleted intersect record {0}:{1}", entityName, id));
				}
			}
			else
			{
				DeleteRequest request = new DeleteRequest { Target = new EntityReference(entityName, id) };
				ApplyBypassBusinessLogicExecution(request);

				if (_nextBatchItems != null)
				{
					WriteVerbose(string.Format("Added delete of {0}:{1} to batch", entityName, id));
					QueueBatchItem(new BatchItem(InputObject, request, (response) => { DeleteCompletion(InputObject, entityName, id, (DeleteResponse)response); }, ex =>
					{
						if (IfExists.IsPresent && ex.ErrorCode == -2147220969)
						{
							WriteVerbose(string.Format("Record {0}:{1} was not present", entityName, id));
							return true;
						}

						return false;
					}));
				}
				else
				{
					if (ShouldProcess(string.Format("Delete record {0}:{1}", entityName, id)))
					{
						try
						{
							DeleteRequest deleteRequest = new DeleteRequest { Target = new EntityReference(entityName, id) };
							ApplyBypassBusinessLogicExecution(deleteRequest);
							Connection.Execute(deleteRequest);
						}
						catch (FaultException ex)
						{
							if (IfExists.IsPresent && ex.HResult == -2147220969)
							{
								WriteVerbose(string.Format("Record {0}:{1} was not present", entityName, id));
							}
							else
							{
								throw;
							}
						}
					}
				}
			}
		}

		private void DeleteCompletion(PSObject inputObject, string entityName, Guid id, DeleteResponse response)
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
				ApplyBypassBusinessLogicExecution(batchRequest);

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

		/// <summary>


		/// Initializes the cmdlet.


		/// </summary>


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

		/// <summary>


		/// Completes cmdlet processing.


		/// </summary>


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