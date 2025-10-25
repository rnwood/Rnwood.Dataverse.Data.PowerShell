using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Parses a Dataverse solution file and returns metadata information.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseSolutionFile")]
    [OutputType(typeof(SolutionInfo))]
    public class GetDataverseSolutionFileCmdlet : PSCmdlet
    {
        /// <summary>
        /// Gets or sets the path to the solution file to parse.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "FromFile", HelpMessage = "Path to the solution file (.zip) to parse.")]
        [ValidateNotNullOrEmpty]
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the solution file bytes to parse.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "FromBytes", HelpMessage = "Solution file bytes to parse.")]
        public byte[] SolutionFile { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Load solution file
            byte[] solutionBytes;
            if (ParameterSetName == "FromFile")
            {
                var filePath = GetUnresolvedProviderPathFromPSPath(Path);
                if (!File.Exists(filePath))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new FileNotFoundException($"Solution file not found: {filePath}"),
                        "FileNotFound",
                        ErrorCategory.ObjectNotFound,
                        filePath));
                    return;
                }

                WriteVerbose($"Loading solution file from: {filePath}");
                solutionBytes = File.ReadAllBytes(filePath);
            }
            else
            {
                solutionBytes = SolutionFile;
            }

            WriteVerbose($"Solution file size: {solutionBytes.Length} bytes");

            // Parse solution file and extract metadata
            var solutionInfo = ParseSolutionFile(solutionBytes);

            WriteObject(solutionInfo);
        }

        private SolutionInfo ParseSolutionFile(byte[] solutionBytes)
        {
            var solutionInfo = new SolutionInfo();

            using (var memoryStream = new MemoryStream(solutionBytes))
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
            {
                // Find the solution.xml file in the solution
                var solutionXmlEntry = archive.Entries.FirstOrDefault(e =>
                    e.FullName.Equals("solution.xml", StringComparison.OrdinalIgnoreCase));

                if (solutionXmlEntry == null)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidDataException("solution.xml not found in solution file"),
                        "SolutionXmlNotFound",
                        ErrorCategory.InvalidData,
                        null));
                    return null;
                }

                using (var stream = solutionXmlEntry.Open())
                using (var reader = new StreamReader(stream))
                {
                    var xmlContent = reader.ReadToEnd();
                    var xdoc = XDocument.Parse(xmlContent);

                    // Navigate to the SolutionManifest element
                    var solutionManifest = xdoc.Root.Element("SolutionManifest");
                    if (solutionManifest == null)
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new InvalidDataException("SolutionManifest element not found in solution.xml"),
                            "SolutionManifestNotFound",
                            ErrorCategory.InvalidData,
                            null));
                        return null;
                    }

                    // Extract the UniqueName
                    var uniqueNameElement = solutionManifest.Element("UniqueName");
                    if (uniqueNameElement != null)
                    {
                        solutionInfo.UniqueName = uniqueNameElement.Value;
                        WriteVerbose($"Solution unique name: {solutionInfo.UniqueName}");
                    }

                    // Extract the LocalizedNames for friendly name
                    var localizedNamesElement = solutionManifest.Element("LocalizedNames");
                    if (localizedNamesElement != null)
                    {
                        var localizedName = localizedNamesElement.Elements("LocalizedName").FirstOrDefault();
                        if (localizedName != null)
                        {
                            solutionInfo.Name = localizedName.Attribute("description")?.Value;
                            WriteVerbose($"Solution name: {solutionInfo.Name}");
                        }
                    }

                    // Extract the Descriptions
                    var descriptionsElement = solutionManifest.Element("Descriptions");
                    if (descriptionsElement != null)
                    {
                        var description = descriptionsElement.Elements("Description").FirstOrDefault();
                        if (description != null)
                        {
                            solutionInfo.Description = description.Attribute("description")?.Value;
                            WriteVerbose($"Solution description: {solutionInfo.Description}");
                        }
                    }

                    // Extract the Version
                    var versionElement = solutionManifest.Element("Version");
                    if (versionElement != null && !string.IsNullOrEmpty(versionElement.Value))
                    {
                        if (Version.TryParse(versionElement.Value, out var version))
                        {
                            solutionInfo.Version = version;
                            WriteVerbose($"Solution version: {solutionInfo.Version}");
                        }
                    }

                    // Extract the Managed flag
                    var managedElement = solutionManifest.Element("Managed");
                    if (managedElement != null && !string.IsNullOrEmpty(managedElement.Value))
                    {
                        solutionInfo.IsManaged = managedElement.Value == "1";
                        WriteVerbose($"Solution is {(solutionInfo.IsManaged ? "managed" : "unmanaged")}");
                    }

                    // Extract Publisher information
                    var publisherElement = solutionManifest.Element("Publisher");
                    if (publisherElement != null)
                    {
                        // Publisher UniqueName
                        var publisherUniqueNameElement = publisherElement.Element("UniqueName");
                        if (publisherUniqueNameElement != null)
                        {
                            solutionInfo.PublisherUniqueName = publisherUniqueNameElement.Value;
                            WriteVerbose($"Publisher unique name: {solutionInfo.PublisherUniqueName}");
                        }

                        // Publisher LocalizedNames for friendly name
                        var publisherLocalizedNamesElement = publisherElement.Element("LocalizedNames");
                        if (publisherLocalizedNamesElement != null)
                        {
                            var publisherLocalizedName = publisherLocalizedNamesElement.Elements("LocalizedName").FirstOrDefault();
                            if (publisherLocalizedName != null)
                            {
                                solutionInfo.PublisherName = publisherLocalizedName.Attribute("description")?.Value;
                                WriteVerbose($"Publisher name: {solutionInfo.PublisherName}");
                            }
                        }

                        // Publisher CustomizationPrefix
                        var customizationPrefixElement = publisherElement.Element("CustomizationPrefix");
                        if (customizationPrefixElement != null)
                        {
                            solutionInfo.PublisherPrefix = customizationPrefixElement.Value;
                            WriteVerbose($"Publisher prefix: {solutionInfo.PublisherPrefix}");
                        }
                    }
                }
            }

            return solutionInfo;
        }
    }
}
