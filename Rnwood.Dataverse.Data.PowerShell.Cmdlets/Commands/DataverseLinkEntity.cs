using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
	/// <summary>
	/// Wrapper class for LinkEntity to enable pipeline serialization.
	/// </summary>
    public class DataverseLinkEntity
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="DataverseLinkEntity"/> class.
		/// </summary>
		/// <param name="linkEntity">The link entity to wrap.</param>
        public DataverseLinkEntity(LinkEntity linkEntity)
        {
            LinkEntity = linkEntity;
        }

		/// <summary>
		/// Gets the wrapped LinkEntity.
		/// </summary>
        public LinkEntity LinkEntity
        {
            get; private set;
        }

		/// <summary>
		/// Returns an XML string representation of the LinkEntity.
		/// </summary>
		/// <returns>An XML string.</returns>
        public override string ToString()
        {
            DataContractSerializer ser = new DataContractSerializer(typeof(LinkEntity));

            using (StringWriter wri = new StringWriter())
            {
                using (XmlWriter x = XmlWriter.Create(wri))
                {
                    ser.WriteObject(x, LinkEntity);
                }

                return wri.ToString();
            }
        }

		/// <summary>
		/// Implicitly converts a PSObject to a DataverseLinkEntity.
		/// </summary>
		/// <param name="obj">The PSObject to convert.</param>
        public static implicit operator DataverseLinkEntity(PSObject obj)
        {
            if (obj.TypeNames.Contains("Deserialized.Rnwood.Dataverse.Data.PowerShell.Commands.DataverseLinkEntity"))
            {
                string stringVal = obj.ToString();

                DataContractSerializer ser = new DataContractSerializer(typeof(LinkEntity));
                using (StringReader rea = new StringReader(stringVal))
                {
                    using (XmlReader x = XmlReader.Create(rea))
                    {
                        return new DataverseLinkEntity((LinkEntity)ser.ReadObject(x));
                    }
                }
            }

            throw new ArgumentException("Cannot convert from this type");
        }

		/// <summary>
		/// Implicitly converts a LinkEntity to a DataverseLinkEntity.
		/// </summary>
		/// <param name="linkEntity">The LinkEntity to convert.</param>
        public static implicit operator DataverseLinkEntity(LinkEntity linkEntity)
        {
            return new DataverseLinkEntity(linkEntity);
        }

		/// <summary>
		/// Implicitly converts a Hashtable to a DataverseLinkEntity using simplified syntax.
		/// The hashtable should contain a single key-value pair where:
		/// - Key format: "fromEntity.fromAttribute" = "toEntity.toAttribute"
		/// - Optional keys: "type" (Inner or LeftOuter), "alias" (string), "filter" (hashtable)
		/// Example: @{ 'contact.accountid' = 'account.accountid'; type = 'LeftOuter'; alias = 'linkedAccount' }
		/// </summary>
		/// <param name="hashtable">The hashtable to convert.</param>
        public static implicit operator DataverseLinkEntity(Hashtable hashtable)
        {
            if (hashtable == null)
            {
                throw new ArgumentNullException(nameof(hashtable));
            }

            // Find the link key (the one that contains a dot on both sides of the =)
            string linkKey = null;
            string linkValue = null;
            
            foreach (DictionaryEntry entry in hashtable)
            {
                string key = entry.Key.ToString();
                if (key.Contains('.') && entry.Value is string val && val.Contains('.'))
                {
                    linkKey = key;
                    linkValue = val;
                    break;
                }
            }

            if (string.IsNullOrEmpty(linkKey) || string.IsNullOrEmpty(linkValue))
            {
                throw new ArgumentException("Hashtable must contain a link specification in format 'fromEntity.fromAttribute' = 'toEntity.toAttribute'");
            }

            // Parse the link specification
            string[] fromParts = linkKey.Split('.');
            string[] toParts = linkValue.Split('.');

            if (fromParts.Length != 2 || toParts.Length != 2)
            {
                throw new ArgumentException($"Invalid link format. Expected 'entity.attribute' = 'entity.attribute', got '{linkKey}' = '{linkValue}'");
            }

            string fromEntity = fromParts[0];
            string fromAttribute = fromParts[1];
            string toEntity = toParts[0];
            string toAttribute = toParts[1];

            // Create the LinkEntity
            LinkEntity linkEntity = new LinkEntity
            {
                LinkFromEntityName = fromEntity,
                LinkFromAttributeName = fromAttribute,
                LinkToEntityName = toEntity,
                LinkToAttributeName = toAttribute,
                JoinOperator = JoinOperator.Inner
            };

            // Process optional parameters
            if (hashtable.ContainsKey("type"))
            {
                string joinType = hashtable["type"].ToString();
                if (string.Equals(joinType, "LeftOuter", StringComparison.OrdinalIgnoreCase))
                {
                    linkEntity.JoinOperator = JoinOperator.LeftOuter;
                }
                else if (string.Equals(joinType, "Inner", StringComparison.OrdinalIgnoreCase))
                {
                    linkEntity.JoinOperator = JoinOperator.Inner;
                }
                else
                {
                    throw new ArgumentException($"Invalid join type '{joinType}'. Valid values are 'Inner' or 'LeftOuter'");
                }
            }

            if (hashtable.ContainsKey("alias"))
            {
                linkEntity.EntityAlias = hashtable["alias"].ToString();
            }

            if (hashtable.ContainsKey("filter"))
            {
                if (hashtable["filter"] is Hashtable filterHash)
                {
                    // Apply filter conditions to the link entity
                    linkEntity.LinkCriteria = new FilterExpression(LogicalOperator.And);
                    
                    foreach (DictionaryEntry filterEntry in filterHash)
                    {
                        string fieldName = filterEntry.Key.ToString();
                        object value = filterEntry.Value;

                        ConditionOperator op = ConditionOperator.Equal;
                        object conditionValue = value;

                        // Support the same nested hashtable format as FilterValues
                        if (value is Hashtable conditionHash)
                        {
                            if (conditionHash.ContainsKey("operator"))
                            {
                                string operatorStr = conditionHash["operator"].ToString();
                                try
                                {
                                    op = (ConditionOperator)Enum.Parse(typeof(ConditionOperator), operatorStr);
                                }
                                catch (ArgumentException e)
                                {
                                    throw new ArgumentException($"Invalid operator '{operatorStr}' for filter field '{fieldName}'. {e.Message}");
                                }
                            }

                            if (conditionHash.ContainsKey("value"))
                            {
                                conditionValue = conditionHash["value"];
                            }
                        }

                        // Add the condition
                        if (conditionValue is Array array)
                        {
                            linkEntity.LinkCriteria.AddCondition(fieldName, op, (object[])array);
                        }
                        else
                        {
                            linkEntity.LinkCriteria.AddCondition(fieldName, op, conditionValue);
                        }
                    }
                }
                else
                {
                    throw new ArgumentException("Filter must be a hashtable");
                }
            }

            return new DataverseLinkEntity(linkEntity);
        }
    }
}