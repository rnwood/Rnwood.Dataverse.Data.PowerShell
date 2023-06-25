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
    public class DataverseLinkEntity
    {
        public DataverseLinkEntity(LinkEntity linkEntity)
        {
            LinkEntity = linkEntity;
        }

        public LinkEntity LinkEntity
        {
            get; private set;
        }

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

        public static implicit operator DataverseLinkEntity(LinkEntity linkEntity)
        {
            return new DataverseLinkEntity(linkEntity);
        }
    }
}