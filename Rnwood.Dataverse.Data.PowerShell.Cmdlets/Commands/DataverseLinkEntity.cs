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
                    // Use the shared filter parsing logic so link filters stay
                    // consistent with -FilterValues parsing in Get-DataverseRecord.
                    // For qualified keys (e.g. 'publisher.uniquename') the
                    // FilterHelpers will pass the entity name to AddCondition so
                    // attribute lookups behave correctly for linked conditions.
                    linkEntity.LinkCriteria = new FilterExpression(LogicalOperator.And);
                    FilterHelpers.ProcessHashFilterValues(linkEntity.LinkCriteria, new[] { filterHash }, false);
                }
                else
                {
                    throw new ArgumentException("Filter must be a hashtable");
                }
            }

            // Process optional child links (nested joins)
            // The 'links' key may contain a single hashtable, an array/list of hashtables,
            // or DataverseLinkEntity/PSObject representations. Each child link is attached
            // to this link as a nested LinkEntity.
            if (hashtable.ContainsKey("links"))
            {
                var linksObj = hashtable["links"];

                // Helper to attach a child DataverseLinkEntity to the parent LinkEntity.
                void AttachChild(object childObj)
                {
                    // If a DictionaryEntry was provided (for example by iterating a Hashtable)
                    // use the Value as the intended child object.
                    if (childObj is DictionaryEntry de)
                    {
                        childObj = de.Value;
                    }
                    DataverseLinkEntity childDle = null;

                    if (childObj is DataverseLinkEntity dle)
                    {
                        childDle = dle;
                    }
                    else if (childObj is Hashtable childHash)
                    {
                        // Reuse the hashtable conversion (this will recurse for grandchildren)
                        childDle = (DataverseLinkEntity)childHash;
                    }
                    else if (childObj is PSObject pso)
                    {
                        // PSObjects that were deserialized (or created) can be converted too
                        if (pso.BaseObject is Hashtable bh)
                        {
                            childDle = (DataverseLinkEntity)bh;
                        }
                        else if (pso.BaseObject is LinkEntity le)
                        {
                            childDle = new DataverseLinkEntity(le);
                        }
                        else
                        {
                            // Try direct implicit conversion where possible
                            try
                            {
                                childDle = (DataverseLinkEntity)pso;
                            }
                            catch
                            {
                                throw new ArgumentException("Invalid child link type in 'links' array");
                            }
                        }
                    }
                    else
                    {
                        throw new ArgumentException("Invalid child link type in 'links' array");
                    }

                    if (childDle == null)
                    {
                        throw new ArgumentException("Invalid child link specification");
                    }

                    // Prefer to add the fully-built child LinkEntity to preserve any
                    // nested descendants that were built by recursive conversion.
                    if (linkEntity.LinkEntities != null)
                    {
                        linkEntity.LinkEntities.Add(childDle.LinkEntity);
                    }
                    else
                    {
                        // Fallback: reconstruct the child on the parent using AddLink
                        var created = linkEntity.AddLink(childDle.LinkEntity.LinkToEntityName,
                            childDle.LinkEntity.LinkFromAttributeName,
                            childDle.LinkEntity.LinkToAttributeName,
                            childDle.LinkEntity.JoinOperator);

                        created.EntityAlias = childDle.LinkEntity.EntityAlias;
                        created.LinkCriteria = childDle.LinkEntity.LinkCriteria;

                        if (childDle.LinkEntity.LinkEntities != null)
                        {
                            foreach (var grandChild in childDle.LinkEntity.LinkEntities)
                            {
                                created.LinkEntities.Add(grandChild);
                            }
                        }
                    }
                }

                // If linksObj is a Hashtable it represents a single child link and should
                // not be iterated (Hashtable implements IEnumerable and iterating would
                // enumerate its entries rather than the intended child object).
                if (linksObj is Hashtable)
                {
                    AttachChild(linksObj);
                }
                // If linksObj is a DataverseLinkEntity or PSObject treat as single item
                else if (linksObj is DataverseLinkEntity || linksObj is PSObject || linksObj is LinkEntity)
                {
                    AttachChild(linksObj);
                }
                // Otherwise if it's an enumerable (e.g. array/list of child specs) iterate
                else if (linksObj is IEnumerable enumerable && !(linksObj is string))
                {
                    foreach (var item in enumerable)
                    {
                        AttachChild(item);
                    }
                }
                else
                {
                    AttachChild(linksObj);
                }
            }

            return new DataverseLinkEntity(linkEntity);
        }

        // Normalization of link filters has been removed. Link filter
        // hashtables are processed directly by FilterHelpers so callers
        // must provide keys that target the linked/to entity.
    }
}