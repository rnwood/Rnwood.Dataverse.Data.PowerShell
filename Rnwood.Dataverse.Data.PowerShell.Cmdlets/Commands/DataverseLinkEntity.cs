using Microsoft.Xrm.Sdk.Query;
using System;
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
    }
}