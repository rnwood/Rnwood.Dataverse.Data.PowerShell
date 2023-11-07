using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    public class SolutionFileInfo
    {
        public Version Version { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsManaged { get; set; }

        public SolutionFileComponent[] Components { get; set; }

        public static SolutionFileInfo FromZipFile(string solutionFile)
        {
            using (ZipFile zipfile = ZipFile.Read(solutionFile))
            {
                using (Stream stream = zipfile["solution.xml"].OpenReader())
                {
                    XDocument solutionDoc = XDocument.Load(stream);

                    SolutionFileInfo result = new SolutionFileInfo()
                    {
                        IsManaged = (bool)solutionDoc.Root.Element("SolutionManifest").Element("Managed"),
                        Version = Version.Parse((string)solutionDoc.Root.Element("SolutionManifest").Element("Version")),
                        Name = (string)solutionDoc.Root.Element("SolutionManifest").Element("UniqueName"),
                        Description = (string)solutionDoc.Root.Element("SolutionManifest").Element("LocalizedNames").Element("LocalizedName").Attribute("description")
                    };

                    result.Components = solutionDoc.Root.Element("SolutionManifest").Element("RootComponents").Elements("RootComponent").Select(e => new SolutionFileComponent()
                    {
                        Name = (string)e.Attribute("schemaName"),
                        ComponentTypeId = (int)e.Attribute("type"),
                        Id = ((Guid?)e.Attribute("id")) ?? ((Guid?)e.Attribute("parentId"))
                    }).ToArray();

                    return result;
                }
            }
        }
    }

    public class SolutionFileComponent
    {
        public string Name { get; set; }
        public Guid? Id { get; set; }
        public int ComponentTypeId { get; set; }
    }
}