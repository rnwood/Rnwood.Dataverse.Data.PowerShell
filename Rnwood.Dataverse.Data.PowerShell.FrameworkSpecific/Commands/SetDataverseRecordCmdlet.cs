
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
	[Cmdlet(VerbsCommon.Set, "DataverseRecord", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
	///<summary>Creates or updates records in a Dataverse environment. If a matching record is found then it will be updated, otherwise a new record is created (some options can override this).
	///This command can also handle creation/update of intersect records(many to many relationships).</summary>

	public class SetDataverseRecordCmdlet : CustomLogicBypassableOrganizationServiceCmdlet
	{
		[Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnnection cmdlet, or string specifying Dataverse organization URL (e.g. http://server.com/MyOrg/)")]
		public override ServiceClient Connection { get; set; }

		[Parameter(Mandatory = true, ValueFromPipeline = true, ValueFromRemainingArguments = true,
			HelpMessage = "Object containing values to be used. Property names must match the logical names of Dataverse columns in the specified table and the property values are used to set the values of the Dataverse record being created/updated. The properties may include ownerid, statecode and statuscode which will assign and change the record state/status.")]
		public PSObject InputObject { get; set; }

		[Parameter()]
		public uint BatchSize { get; set; } = 100;

		[Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of table")]
		[Alias("EntityName", "LogicalName")]
		public string TableName { get; set; }

		[Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "List of properties on the input object which are ignored and not attemted to be mapped to the record. Default is none.")]
		public string[] IgnoreProperties { get; set; }

		[Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of record to be created or updated.")]
		public Guid Id { get; set; }

		[Parameter(Mandatory = false, HelpMessage = "List of list of column names that identify an existing record to update based on the values of those columns in the InputObject. These are used if a record with and Id matching the value of the Id cannot be found. The first list that returns a match is used. e.g. (\"firstname\", \"lastname\"), \"fullname\" will try to find an existing record based on the firstname AND listname from the InputObject and if not found it will try by fullname. Not supported with -Upsert")]
		public string[][] MatchOn { get; set; }

		[Parameter(HelpMessage = "If specified, the InputObject is written to the pipeline with an Id property set indicating the primary key of the affected record (even if nothing was updated).")]
		public SwitchParameter PassThru { get; set; }

		[Parameter(HelpMessage = "If specified, existing records matching the ID and or MatchOn columns will not be updated.")]
		public SwitchParameter NoUpdate { get; set; }

		[Parameter(HelpMessage = "If specified, then no records will be created even if no existing records matching the ID and or MatchOn columns is found.")]
		public SwitchParameter NoCreate { get; set; }

		[Parameter(HelpMessage = "List of column names which will not be included when updating existing records.")]
		public string[] NoUpdateColumns { get; set; }

		[Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "If specified, the creation/updates will be done on behalf of the user with the specified ID. For best performance, sort the records using this value since a new batch request is needed each time this value changes.")]
		public Guid? CallerId { get; set; }

		[Parameter(HelpMessage = "If specified, an update containing all supplied columns will be issued without retrieving the existing record for comparison (default is to remove unchanged columns). Id must be provided")]
		public SwitchParameter UpdateAllColumns { get; set; }

		[Parameter(HelpMessage = "If specified, no check for existing record is made and records will always be attempted to be created. Use this option when it's known that no existing matching records will exist to improve performance. See the -noupdate option for an alternative.")]
		public SwitchParameter CreateOnly { get; set; }

		[Parameter(HelpMessage = "If specified, upsert request will be used to create/update existing records as appropriate. -MatchOn is not supported with this option")]
		public SwitchParameter Upsert { get; set; }

		[Parameter(Mandatory = false, HelpMessage = "Hashset of lookup column name in the target entity to column name in the referred to table with which to find the records.")]
		public Hashtable LookupColumns
		{
			get;
			set;
		}

		[Parameter(HelpMessage = "Specifies the types of business logic (for example plugins) to bypass")]
		public override BusinessLogicTypes[] BypassBusinessLogicExecution { get; set; }


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

			private object FormatValue(object value)
			{
				if (value is EntityReference er)
				{
					return $"{er.LogicalName}:{er.Id}";
				} 

				if (value is Entity en)
				{
					string attributeValues = string.Join(", ", en.Attributes.Select(a => $"{a.Key}='{a.Value}'"));
					return $"{en.LogicalName} : {{{attributeValues}}}";
				}

				return value?.ToString();
			}

			public override string ToString()
			{
				return Request.RequestName + " " + string.Join(", ", Request.Parameters.Select(p => $"{p.Key}='{FormatValue(p.Value)}'"));
			}
		}

		private List<BatchItem> _nextBatchItems;
		private Guid? _nextBatchCallerId;

		private void QueueBatchItem(BatchItem item, Guid? callerId)
		{
			if (_nextBatchItems.Any() && _nextBatchCallerId != callerId)
			{
				ProcessBatch();
			}

			_nextBatchCallerId = callerId;
			_nextBatchItems.Add(item);

			if (_nextBatchItems.Count == BatchSize)
			{
				ProcessBatch();
			}
		}

		protected override void BeginProcessing()
		{
			base.BeginProcessing();

			recordCount = 0;
			entityMetadataFactory = new EntityMetadataFactory(Connection);
			entityConverter = new DataverseEntityConverter(Connection, entityMetadataFactory);



			if (BatchSize > 1)
			{
				_nextBatchItems = new List<BatchItem>();
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

				Guid oldCallerId = Connection.CallerId;
				Connection.CallerId = _nextBatchCallerId.GetValueOrDefault();
				try
				{
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
				finally
				{
					Connection.CallerId = oldCallerId;
				}
			}

			_nextBatchItems.Clear();
			_nextBatchCallerId = null;
		}

		protected override void EndProcessing()
		{
			base.EndProcessing();

			if (_nextBatchItems != null)
			{
				ProcessBatch();
			}
		}

		private int recordCount;
		private EntityMetadataFactory entityMetadataFactory;
		private DataverseEntityConverter entityConverter;

		private ConvertToDataverseEntityOptions GetConversionOptions()
		{
			ConvertToDataverseEntityOptions options = new ConvertToDataverseEntityOptions();


			options.IgnoredPropertyName.Add("TableName");
			options.IgnoredPropertyName.Add("EntityName");
			options.IgnoredPropertyName.Add("Id");

			if (IgnoreProperties != null)
			{
				foreach (string ignoreProperty in IgnoreProperties)
				{
					options.IgnoredPropertyName.Add(ignoreProperty);
				}
			}

			if (LookupColumns != null)
			{
				foreach (DictionaryEntry lookupColumn in LookupColumns)
				{
					options.ColumnOptions[(string)lookupColumn.Key] = new ConvertToDataverseEntityColumnOptions() { LookupColumn = (string)lookupColumn.Value };
				}
			}

			return options;
		}

		protected override void ProcessRecord()
		{
			base.ProcessRecord();

			if (CallerId.HasValue)
			{
				if (BatchSize > 1)
				{
					throw new ArgumentException("CreatedOnBehalfBy not supported with BatchSize > 1");
				}
			}

			recordCount++;
			WriteVerbose("Processing record #" + recordCount);

			Entity target;

			try
			{
				target = entityConverter.ConvertToDataverseEntity(InputObject, TableName, GetConversionOptions());
			}
			catch (FormatException e)
			{
				WriteError(new ErrorRecord(new Exception("Error converting input object: " + e.Message, e), null, ErrorCategory.InvalidData, InputObject));
				return;
			}

			EntityMetadata entityMetadata = entityMetadataFactory.GetMetadata(TableName);

			Entity existingRecord;

			try
			{
				existingRecord = GetExistingRecord(entityMetadata, target);
			}
			catch (Exception e)
			{
				WriteError(new ErrorRecord(e, null, ErrorCategory.InvalidOperation, InputObject)); ;
				return;
			}

			if (Upsert.IsPresent)
			{
				UpsertRecord(entityMetadata, target);
			}
			else
			{
				if (existingRecord != null)
				{
					UpdateExistingRecord(entityMetadata, target, existingRecord);
				}
				else
				{
					CreateNewRecord(entityMetadata, target);
				}
			}

			//Skip assignment/status if record creation failed.
			if ((existingRecord != null && !NoUpdate) || (existingRecord == null && !NoCreate))
			{
				if (target.Contains("ownerid"))
				{
					EntityReference ownerid = target.GetAttributeValue<EntityReference>("ownerid");
					AssignRequest request = new AssignRequest()
					{
						Assignee = ownerid,
						Target = target.ToEntityReference()
					};
					ApplyBypassBusinessLogicExecution(request);

					if (_nextBatchItems != null)
					{
						WriteVerbose(string.Format("Added assignment of record {0}:{1} to {2} to batch", TableName, target.Id, ownerid.Name));
						QueueBatchItem(new BatchItem(InputObject, request, (response) => { AssignRecordCompletion(target, ownerid); }), CallerId);
					}
					else
					{
						if (ShouldProcess(string.Format("Assign record {0}:{1} to {2}", TableName, target.Id, ownerid.Name)))
						{
							try
							{
								Connection.Execute(request);
								AssignRecordCompletion(target, ownerid);
							}
							catch (Exception e)
							{
								WriteError(new ErrorRecord(e, null, ErrorCategory.WriteError, InputObject));
							}
						}
					}
				}

				if (target.Contains("statecode") || target.Contains("statuscode"))
				{
					OptionSetValue statuscode = target.GetAttributeValue<OptionSetValue>("statuscode") ?? new OptionSetValue(-1);

					OptionSetValue stateCode = target.GetAttributeValue<OptionSetValue>("statecode");

					if (stateCode == null)
					{
						StatusAttributeMetadata statusMetadata =
							(StatusAttributeMetadata)entityMetadata.Attributes.First(
								a => a.LogicalName.Equals("statuscode", StringComparison.OrdinalIgnoreCase));

						stateCode = new OptionSetValue(statusMetadata.OptionSet.Options.Cast<StatusOptionMetadata>()
									  .First(o => o.Value.Value == statuscode.Value)
									  .State.Value);
					}

					SetStateRequest request = new SetStateRequest()
					{
						EntityMoniker = target.ToEntityReference(),
						State = stateCode,
						Status = statuscode
					};
					ApplyBypassBusinessLogicExecution(request);

					if (_nextBatchItems != null)
					{
						WriteVerbose(string.Format("Added set record {0}:{1} status to State:{2} Status: {3} to batch", TableName, Id, stateCode.Value, statuscode.Value));
						QueueBatchItem(new BatchItem(InputObject, request, (response) => { SetStateCompletion(target, statuscode, stateCode); }), CallerId);
					}
					else
					{
						if (ShouldProcess(string.Format("Set record {0}:{1} status to State:{2} Status: {3}", TableName, Id, stateCode.Value, statuscode.Value)))
						{
							try
							{
								Connection.Execute(request);
								SetStateCompletion(target, statuscode, stateCode);
							}
							catch (Exception e)
							{
								WriteError(new ErrorRecord(e, null, ErrorCategory.WriteError, InputObject));
							}
						}
					}
				}
			}
		}

		private void SetStateCompletion(Entity target, OptionSetValue statuscode, OptionSetValue stateCode)
		{
			WriteVerbose(string.Format("Record {0}:{1} status set to State:{2} Status: {3}", target.LogicalName, target.Id, stateCode.Value, statuscode.Value));
		}

		private void AssignRecordCompletion(Entity target, EntityReference ownerid)
		{
			WriteVerbose(string.Format("Record {0}:{1} assigned to {2}", target.LogicalName, target.Id, ownerid.Name));
		}

		private bool UpsertRecord(EntityMetadata entityMetadata, Entity target)
		{
			bool result = true;

			if (NoCreate || NoUpdate || MatchOn != null)
			{
				throw new ArgumentException("-NoCreate, -NoUpdate and -MatchOn are not supported with -Upsert");
			}

			if (entityMetadata.IsIntersect.GetValueOrDefault())
			{
				ManyToManyRelationshipMetadata manyToManyRelationshipMetadata = entityMetadata.ManyToManyRelationships[0];

				EntityReference record1 = new EntityReference(manyToManyRelationshipMetadata.Entity1LogicalName,
														 target.GetAttributeValue<Guid>(
															 manyToManyRelationshipMetadata.Entity1IntersectAttribute));
				EntityReference record2 = new EntityReference(manyToManyRelationshipMetadata.Entity2LogicalName,
													target.GetAttributeValue<Guid>(
														manyToManyRelationshipMetadata.Entity2IntersectAttribute));

				AssociateRequest request = new AssociateRequest()
				{
					Target = record1,
					RelatedEntities =
								new EntityReferenceCollection()
							{
									record2
							},
					Relationship = new Relationship(manyToManyRelationshipMetadata.SchemaName)
					{
						PrimaryEntityRole = EntityRole.Referencing
					},
				};
				ApplyBypassBusinessLogicExecution(request);

				if (_nextBatchItems != null)
				{
					WriteVerbose(string.Format("Added create of new intersect record {0}:{1},{2} to batch", TableName, record1.Id, record2.Id));
					QueueBatchItem(new BatchItem(InputObject, request, (response) => { AssociateUpsertCompletion(true, target, InputObject, manyToManyRelationshipMetadata, record1, record2); }, fault => { return AssociateUpsertError(fault, target, InputObject, manyToManyRelationshipMetadata, record1, record2); }), CallerId);

					if (PassThru.IsPresent)
					{
						QueryExpression getIdQuery = new QueryExpression(TableName);
						getIdQuery.Criteria.AddCondition(manyToManyRelationshipMetadata.Entity1IntersectAttribute, ConditionOperator.Equal, record1.Id);
						getIdQuery.Criteria.AddCondition(manyToManyRelationshipMetadata.Entity2IntersectAttribute, ConditionOperator.Equal, record2.Id);
						QueueBatchItem(new BatchItem(InputObject, new RetrieveMultipleRequest() { Query = getIdQuery }, response =>
						{
							AssociateUpsertGetIdCompletion(response, InputObject);
						}), CallerId);
					}
				}
				else
				{
					throw new NotSupportedException("Upsert not supported for insertsect entities except in batch mode");
				}
			}
			else
			{
				Entity targetUpdate = new Entity(target.LogicalName) { Id = target.Id };
				targetUpdate.Attributes.AddRange(target.Attributes.Where(a => !dontUpdateDirectlyColumnNames.Contains(a.Key, StringComparer.OrdinalIgnoreCase)));

				string columnSummary = GetColumnSummary(targetUpdate);

				UpsertRequest request = new UpsertRequest()
				{
					Target = targetUpdate
				};

				if (_nextBatchItems != null)
				{
					if (target.Id == Guid.Empty)
					{
						targetUpdate.Id = Guid.NewGuid();
					}

					var inputObject = InputObject;

					WriteVerbose(string.Format("Added upsert of new record {0}:{1} to batch - columns:\n{2}", TableName, targetUpdate.Id, columnSummary));
					QueueBatchItem(new BatchItem(InputObject, request, (response) => { UpsertCompletion(targetUpdate, inputObject, columnSummary, (UpsertResponse)response); }), CallerId);
				}
				else
				{
					if (ShouldProcess(string.Format("Upsert record {0}:{1} columns:\n{2}", TableName, targetUpdate.Id, columnSummary)))
					{
						try
						{
							UpsertResponse response = (UpsertResponse)Connection.Execute(request);
							UpsertCompletion(targetUpdate, InputObject, columnSummary, response);
						}
						catch (Exception e)
						{
							result = false;
							WriteError(new ErrorRecord(new Exception(string.Format("Error creating record {0}:{1} {2}, columns: {3}", TableName, targetUpdate.Id, e.Message, columnSummary), e), null, ErrorCategory.InvalidResult, InputObject));
						}
					}
				}
			}

			return result;
		}

		private void AssociateUpsertGetIdCompletion(OrganizationResponse response, PSObject inputObject)
		{
			Guid id = ((RetrieveMultipleResponse)response).EntityCollection.Entities.Single().Id;

			if (inputObject.Properties.Any(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)))
			{
				inputObject.Properties.Remove("Id");
			}
			inputObject.Properties.Add(new PSNoteProperty("Id", id));

			WriteObject(inputObject);
		}

		private void UpsertCompletion(Entity targetUpdate, PSObject inputObject, string columnSummary, UpsertResponse response)
		{
			Guid id = response.Target.Id;

			if (inputObject.Properties.Any(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)))
			{
				inputObject.Properties.Remove("Id");
			}

			inputObject.Properties.Add(new PSNoteProperty("Id", id));

			if (response.RecordCreated)
			{
				WriteVerbose(string.Format("Upsert created new record {0}:{1} columns:\n{2}", TableName, targetUpdate.Id, columnSummary));
			}
			else
			{
				WriteVerbose(string.Format("Upsert updated existing record {0}:{1} columns:\n{2}", TableName, targetUpdate.Id, columnSummary));
			}

			if (PassThru.IsPresent)
			{
				WriteObject(inputObject);
			}
		}

		private void CreateNewRecord(EntityMetadata entityMetadata, Entity target)
		{
			if (NoCreate.IsPresent)
			{
				WriteVerbose(string.Format("Skipped creating new record {0}:{1} - NoCreate enabled", TableName, Id));
				return;
			}

			if (entityMetadata.IsIntersect.GetValueOrDefault())
			{
				ManyToManyRelationshipMetadata manyToManyRelationshipMetadata = entityMetadata.ManyToManyRelationships[0];

				EntityReference record1 = new EntityReference(manyToManyRelationshipMetadata.Entity1LogicalName,
														 target.GetAttributeValue<Guid>(
															 manyToManyRelationshipMetadata.Entity1IntersectAttribute));
				EntityReference record2 = new EntityReference(manyToManyRelationshipMetadata.Entity2LogicalName,
													target.GetAttributeValue<Guid>(
														manyToManyRelationshipMetadata.Entity2IntersectAttribute));

				AssociateRequest request = new AssociateRequest()
				{
					Target = record1,
					RelatedEntities =
								new EntityReferenceCollection()
							{
									record2
							},
					Relationship = new Relationship(manyToManyRelationshipMetadata.SchemaName)
					{
						PrimaryEntityRole = EntityRole.Referencing
					},
				};
				ApplyBypassBusinessLogicExecution(request);

				if (_nextBatchItems != null)
				{
					WriteVerbose(string.Format("Added create of new intersect record {0}:{1},{2} to batch", TableName, record1.Id, record2.Id));
					QueueBatchItem(new BatchItem(InputObject, request, (response) => { AssociateCompletion(target, InputObject, manyToManyRelationshipMetadata, record1, record2); }), CallerId);
				}
				else
				{
					if (ShouldProcess(string.Format("Create new intersect record {0}", TableName)))
					{
						try
						{
							if (CallerId.HasValue)
							{
								Connection.CallerId = CallerId.Value;
							}
							Connection.Execute(request);
							if (CallerId.HasValue)
							{
								Connection.CallerId = Guid.Empty;
							}

							AssociateCompletion(target, InputObject, manyToManyRelationshipMetadata, record1, record2);
						}
						catch (Exception e)
						{
							if (CallerId.HasValue)
							{
								Connection.CallerId = Guid.Empty;
							}
							WriteError(new ErrorRecord(e, null, ErrorCategory.WriteError, InputObject));
						}
					}
				}
			}
			else
			{
				Entity targetCreate = new Entity(target.LogicalName) { Id = target.Id };
				targetCreate.Attributes.AddRange(target.Attributes.Where(a => !dontUpdateDirectlyColumnNames.Contains(a.Key, StringComparer.OrdinalIgnoreCase)));

				string columnSummary = GetColumnSummary(targetCreate);

				CreateRequest request = new CreateRequest()
				{
					Target = targetCreate
				};
				ApplyBypassBusinessLogicExecution(request);

				if (_nextBatchItems != null)
				{
					WriteVerbose(string.Format("Added created of new record {0}:{1} to batch - columns:\n{2}", TableName, targetCreate.Id, columnSummary));
					var inputObject = InputObject;
					QueueBatchItem(new BatchItem(InputObject, request, (response) => { CreateCompletion(target, inputObject, targetCreate, columnSummary, (CreateResponse)response); }), CallerId);
				}
				else
				{
					if (ShouldProcess(string.Format("Create new record {0}:{1} columns:\n{2}", TableName, targetCreate.Id, columnSummary)))
					{
						try
						{
							if (CallerId.HasValue)
							{
								Connection.CallerId = CallerId.Value;
							}

							CreateResponse response = (CreateResponse)Connection.Execute(request);

							if (CallerId.HasValue)
							{
								Connection.CallerId = Guid.Empty;
							}

							CreateCompletion(target, InputObject, targetCreate, columnSummary, response);
						}
						catch (Exception e)
						{
							if (CallerId.HasValue)
							{
								Connection.CallerId = Guid.Empty;
							}

							WriteError(new ErrorRecord(new Exception(string.Format("Error creating record {0}:{1} {2}, columns: {3}", TableName, targetCreate.Id, e.Message, columnSummary), e), null, ErrorCategory.InvalidResult, InputObject));
						}
					}
				}
			}
		}

		private void CreateCompletion(Entity target, PSObject inputObject, Entity targetCreate, string columnSummary, CreateResponse response)
		{
			if (inputObject.Properties.Any(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)))
			{
				inputObject.Properties.Remove("Id");
			}

			inputObject.Properties.Add(new PSNoteProperty("Id", response.id));
			WriteVerbose(string.Format("Created new record {0}:{1} columns:\n{2}", target.LogicalName, targetCreate.Id, columnSummary));

			if (PassThru.IsPresent)
			{
				WriteObject(inputObject);
			}
		}

		private bool AssociateUpsertError(OrganizationServiceFault fault, Entity target, PSObject inputObject, ManyToManyRelationshipMetadata manyToManyRelationshipMetadata, EntityReference record1, EntityReference record2)
		{
			if (fault.ErrorCode != -2147220937)
			{
				return false;
			}

			AssociateUpsertCompletion(false, target, inputObject, manyToManyRelationshipMetadata, record1, record2);

			return true;
		}

		private void AssociateUpsertCompletion(bool recordWasCreated, Entity target, PSObject inputObject, ManyToManyRelationshipMetadata manyToManyRelationshipMetadata, EntityReference record1, EntityReference record2)
		{
			if (recordWasCreated)
			{
				WriteVerbose(string.Format("Created intersect record {0}:{1}:{2}", target.LogicalName, record1.Id, record2.Id));
			}
			else
			{
				WriteVerbose(string.Format("Skipped creating (upsert) intersect record as already exists {0}:{1}:{2}", target.LogicalName, record1.Id, record2.Id));
			}
		}

		private void AssociateCompletion(Entity target, PSObject inputObject, ManyToManyRelationshipMetadata manyToManyRelationshipMetadata, EntityReference record1, EntityReference record2)
		{
			QueryExpression getIdQuery = new QueryExpression(TableName);
			getIdQuery.Criteria.AddCondition(manyToManyRelationshipMetadata.Entity1IntersectAttribute, ConditionOperator.Equal, record1.Id);
			getIdQuery.Criteria.AddCondition(manyToManyRelationshipMetadata.Entity2IntersectAttribute, ConditionOperator.Equal, record2.Id);
			Guid id = Connection.RetrieveMultiple(getIdQuery).Entities.Single().Id;

			if (inputObject.Properties.Any(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)))
			{
				inputObject.Properties.Remove("Id");
			}
			inputObject.Properties.Add(new PSNoteProperty("Id", id));
			WriteVerbose(string.Format("Created new intersect record {0}:{1}", target.LogicalName, id));

			if (PassThru.IsPresent)
			{
				WriteObject(inputObject);
			}
		}

		private string Ellipsis(string value)
		{
			if (value == null)
			{
				return null;
			}

			if (value.Length <= 100)
			{
				return value;
			}

			return value.Substring(0, 100) + "...";
		}

		private string GetColumnSummary(Entity entity)
		{
			DataverseEntityConverter converter = new DataverseEntityConverter(Connection, entityMetadataFactory);
			PSObject psObject = converter.ConvertToPSObject(entity, new ColumnSet(entity.Attributes.Select(a => a.Key).ToArray()), a => ValueType.Raw);

			return string.Join("\n", psObject.Properties.Select(a => a.Name + " = " + Ellipsis((a.Value ?? "<null>").ToString())));
		}

		private string[] dontUpdateDirectlyColumnNames = new[] { "statuscode", "statecode", "ownerid" };

		private void UpdateExistingRecord(EntityMetadata entityMetadata, Entity target, Entity existingRecord)
		{
			if (NoUpdate.IsPresent)
			{
				WriteVerbose(string.Format("Skipped updated existing record {0}:{1} - NoUpdate enabled", TableName, Id));
				return;
			}

			target.Id = existingRecord.Id;
			target[entityMetadata.PrimaryIdAttribute] = existingRecord[entityMetadata.PrimaryIdAttribute];

			if (InputObject.Properties.Any(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)))
			{
				InputObject.Properties.Remove("Id");
			}
			InputObject.Properties.Add(new PSNoteProperty("Id", existingRecord.Id));

			RemoveUnchangedColumns(target, existingRecord);

			if (NoUpdateColumns != null)
			{
				foreach (string noUpdateColumns in NoUpdateColumns)
				{
					target.Attributes.Remove(noUpdateColumns);
				}
			}

			Entity targetUpdate = new Entity(target.LogicalName);
			targetUpdate.Attributes.AddRange(target.Attributes.Where(a => !dontUpdateDirectlyColumnNames.Contains(a.Key, StringComparer.OrdinalIgnoreCase)));

			if (entityMetadata.IsIntersect.GetValueOrDefault())
			{
				if (PassThru.IsPresent)
				{
					WriteObject(InputObject);
				}
			}
			else if (targetUpdate.Attributes.Any())
			{
				DataverseEntityConverter converter = new DataverseEntityConverter(Connection, entityMetadataFactory);

				UpdateRequest request = new UpdateRequest() { Target = target };
				ApplyBypassBusinessLogicExecution(request);
				string updatedColumnSummary = GetColumnSummary(targetUpdate);

				if (_nextBatchItems != null)
				{
					WriteVerbose(string.Format("Added updated of existing record {0}:{1} to batch - columns:\n{2}", TableName, existingRecord.Id, updatedColumnSummary));
					var inputObject = InputObject;
					QueueBatchItem(new BatchItem(InputObject, request, (response) => { UpdateCompletion(target, inputObject, existingRecord, updatedColumnSummary); }), CallerId);
				}
				else
				{
					if (this.ShouldProcess(string.Format("Update existing record {0}:{1} columns:\n{2}", TableName, existingRecord.Id, updatedColumnSummary)))
					{
						try
						{
							Connection.Execute(request);
							UpdateCompletion(target, InputObject, existingRecord, updatedColumnSummary);
						}
						catch (Exception e)
						{
							WriteError(new ErrorRecord(new Exception(string.Format("Error updating record {0}:{1}, {2} columns: {3}", TableName, existingRecord.Id, e.Message, updatedColumnSummary), e), null, ErrorCategory.InvalidResult, InputObject));
						}
					}
				}
			}
			else
			{
				WriteVerbose(string.Format("Skipped updated existing record {0}:{1} - nothing changed", TableName, Id));

				if (PassThru.IsPresent)
				{
					WriteObject(InputObject);
				}
			}
		}

		private void UpdateCompletion(Entity target, PSObject inputObject, Entity existingRecord, string updatedColumnSummary)
		{
			WriteVerbose(string.Format("Updated existing record {0}:{1} columns:\n{2}", target.LogicalName, existingRecord.Id, updatedColumnSummary));

			if (PassThru.IsPresent)
			{
				WriteObject(inputObject);
			}
		}

		private Entity GetExistingRecord(EntityMetadata entityMetadata, Entity target)
		{
			Entity existingRecord = null;

			if (CreateOnly.IsPresent || Upsert.IsPresent)
			{
				return null;
			}

			if (!entityMetadata.IsIntersect.GetValueOrDefault())
			{
				if (Id != Guid.Empty)
				{
					if (UpdateAllColumns.IsPresent)
					{
						existingRecord = new Entity(target.LogicalName) { Id = Id };
						existingRecord[entityMetadata.PrimaryIdAttribute] = Id;
					}
					else
					{
						QueryByAttribute existingRecordQuery = new QueryByAttribute(TableName);
						existingRecordQuery.AddAttributeValue(entityMetadata.PrimaryIdAttribute, Id);
						existingRecordQuery.ColumnSet = target.LogicalName.Equals("calendar", StringComparison.OrdinalIgnoreCase) ? new ColumnSet(true) : new ColumnSet(target.Attributes.Select(a => a.Key).ToArray());

						existingRecord = Connection.RetrieveMultiple(existingRecordQuery
							).Entities.FirstOrDefault();
					}
				}

				if (existingRecord == null && MatchOn != null)
				{
					foreach (string[] matchOnColumnList in MatchOn)
					{
						QueryByAttribute matchOnQuery = new QueryByAttribute(TableName);
						matchOnQuery.TopCount = 2;

						if (entityMetadata.Attributes.Any(a => string.Equals(a.LogicalName, "statecode", StringComparison.OrdinalIgnoreCase)))
						{
							matchOnQuery.AddAttributeValue("statecode", 0);
						}

						if (entityMetadata.Attributes.Any(a => string.Equals(a.LogicalName, "isdisabled", StringComparison.OrdinalIgnoreCase)))
						{
							matchOnQuery.AddAttributeValue("isdisabled", false);
						}

						foreach (string matchOnColumn in matchOnColumnList)
						{
							object queryValue = target.GetAttributeValue<object>(matchOnColumn);

							if (queryValue is EntityReference)
							{
								queryValue = ((EntityReference)queryValue).Id;
							}

							if (queryValue is OptionSetValue)
							{
								queryValue = ((OptionSetValue)queryValue).Value;
							}

							matchOnQuery.AddAttributeValue(matchOnColumn, queryValue);
						}

						matchOnQuery.ColumnSet = new ColumnSet(target.Attributes.Select(a => a.Key).ToArray());

						var existingRecords = Connection.RetrieveMultiple(matchOnQuery
							).Entities;

						if (existingRecords.Count == 1)
						{
							existingRecord = existingRecords[0];
							break;
						}
						else if (existingRecords.Count > 1)
						{
							string matchOnSummary = string.Join("\n", matchOnColumnList.Select(c => c + "='" +
							matchOnQuery.Values[matchOnQuery.Attributes.IndexOf(c)] + "'" ?? "<null>").ToArray());

							throw new Exception(string.Format("Match on values {0} resulted in more than one record to update. Match on values:\n", matchOnSummary));
						}
					}
				}
			}
			else
			{
				if (MatchOn != null)
				{
					throw new ArgumentException("MatchOn not supported for intersect entities", "MatchOn");
				}

				ManyToManyRelationshipMetadata manyToManyRelationshipMetadata = entityMetadata.ManyToManyRelationships[0];

				Guid? entity1Value =
					target.GetAttributeValue<Guid?>(manyToManyRelationshipMetadata.Entity1IntersectAttribute);
				Guid? entity2Value =
					target.GetAttributeValue<Guid?>(manyToManyRelationshipMetadata.Entity2IntersectAttribute);

				if (entity1Value == null || entity2Value == null)
				{
					throw new Exception("For intersect entities (many to many relationships), The input object must contain values for both attributes involved in the relationship.");
				}

				QueryByAttribute existingRecordQuery = new QueryByAttribute(TableName);
				existingRecordQuery.AddAttributeValue(manyToManyRelationshipMetadata.Entity1IntersectAttribute, entity1Value.Value);
				existingRecordQuery.AddAttributeValue(manyToManyRelationshipMetadata.Entity2IntersectAttribute, entity2Value.Value);
				existingRecordQuery.ColumnSet = new ColumnSet(target.Attributes.Select(a => a.Key).ToArray());

				existingRecord = Connection.RetrieveMultiple(existingRecordQuery
					).Entities.FirstOrDefault();
			}
			return existingRecord;
		}

		private void RemoveUnchangedColumns(Entity target, Entity existingRecord)
		{
			foreach (KeyValuePair<string, object> column in target.Attributes.ToArray())
			{
				if ((existingRecord.Contains(column.Key) && Equals(column.Value, existingRecord[column.Key]))
					||
					//Dataverse seems to consider that null and "" string are equal and doesn't include the attribute in retrieve records if the value is either
					((column.Value == null || column.Value as string == "") && !existingRecord.Contains(column.Key)))
				{
					target.Attributes.Remove(column.Key);
				}
			}

			target.Id = existingRecord.Id;
		}
	}
}