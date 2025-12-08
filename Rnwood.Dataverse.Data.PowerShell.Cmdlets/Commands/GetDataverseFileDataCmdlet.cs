using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using System;
using System.IO;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Downloads file data from a Dataverse file column.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseFileData")]
    [OutputType(typeof(byte[]), typeof(FileInfo))]
    public class GetDataverseFileDataCmdlet : OrganizationServiceCmdlet
    {
        private const string PARAMSET_FILEPATH = "FilePath";
        private const string PARAMSET_FOLDER = "Folder";
        private const string PARAMSET_BYTES = "Bytes";
        private const string DOWNLOAD_BLOCK_REQUEST = "DownloadBlock";
        private const int BLOCK_SIZE = 4 * 1024 * 1024; // 4MB blocks

        /// <summary>
        /// Gets or sets the logical name of the table containing the file column.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of table containing the file column")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        [Alias("EntityName", "LogicalName")]
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets the ID of the record containing the file.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the record containing the file")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the logical name of the file column.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of the file column")]
        [ArgumentCompleter(typeof(ColumnNameArgumentCompleter))]
        public string ColumnName { get; set; }

        /// <summary>
        /// Gets or sets the file path where the file will be saved.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_FILEPATH, Mandatory = true, HelpMessage = "File path where the file will be saved")]
        public string FilePath { get; set; }

        /// <summary>
        /// Gets or sets the folder path where the file will be saved. The filename will be taken from the file metadata.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_FOLDER, Mandatory = true, HelpMessage = "Folder path where the file will be saved. The filename will be taken from the file metadata")]
        public string FolderPath { get; set; }

        /// <summary>
        /// Gets or sets whether to return the file content as a byte array.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_BYTES, Mandatory = true, HelpMessage = "Return the file content as a byte array")]
        public SwitchParameter AsBytes { get; set; }

        /// <summary>
        /// Processes each record in the pipeline to download file data.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            try
            {
                // Initialize file blocks download to get file information
                var initRequest = new InitializeFileBlocksDownloadRequest
                {
                    Target = new EntityReference(TableName, Id),
                    FileAttributeName = ColumnName
                };

                var initResponse = (InitializeFileBlocksDownloadResponse)Connection.Execute(initRequest);

                if (initResponse.FileSizeInBytes == 0)
                {
                    WriteWarning($"File column '{ColumnName}' on record {Id} in table '{TableName}' is empty or does not contain a file.");
                    return;
                }

                // Download the file data
                byte[] fileData = DownloadFileData(initResponse.FileContinuationToken);

                // Handle output based on parameter set
                switch (ParameterSetName)
                {
                    case PARAMSET_FILEPATH:
                        SaveToFilePath(fileData, FilePath, initResponse.FileName);
                        break;

                    case PARAMSET_FOLDER:
                        SaveToFolder(fileData, FolderPath, initResponse.FileName);
                        break;

                    case PARAMSET_BYTES:
                        WriteObject(fileData);
                        break;
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "FileDownloadError", ErrorCategory.ReadError, Id));
            }
        }

        private byte[] DownloadFileData(string fileContinuationToken)
        {
            using (var memoryStream = new MemoryStream())
            {
                long offset = 0;

                while (true)
                {
                    var downloadRequest = new OrganizationRequest(DOWNLOAD_BLOCK_REQUEST);
                    downloadRequest.Parameters["FileContinuationToken"] = fileContinuationToken;
                    downloadRequest.Parameters["Offset"] = offset;
                    downloadRequest.Parameters["BlockLength"] = (long)BLOCK_SIZE;

                    var downloadResponse = Connection.Execute(downloadRequest);

                    if (!downloadResponse.Results.Contains("Data"))
                    {
                        break;
                    }

                    var data = downloadResponse.Results["Data"] as byte[];
                    if (data == null || data.Length == 0)
                    {
                        break;
                    }

                    memoryStream.Write(data, 0, data.Length);
                    offset += data.Length;
                }

                return memoryStream.ToArray();
            }
        }

        private void SaveToFilePath(byte[] fileData, string filePath, string originalFileName)
        {
            // Expand any environment variables or relative paths
            string expandedPath = SessionState.Path.GetUnresolvedProviderPathFromPSPath(filePath);
            
            File.WriteAllBytes(expandedPath, fileData);
            WriteVerbose($"File saved to: {expandedPath}");
            
            WriteObject(new FileInfo(expandedPath));
        }

        private void SaveToFolder(byte[] fileData, string folderPath, string fileName)
        {
            // Expand any environment variables or relative paths
            string expandedPath = SessionState.Path.GetUnresolvedProviderPathFromPSPath(folderPath);
            
            if (!Directory.Exists(expandedPath))
            {
                Directory.CreateDirectory(expandedPath);
            }

            string fullPath = Path.Combine(expandedPath, fileName);
            File.WriteAllBytes(fullPath, fileData);
            WriteVerbose($"File saved to: {fullPath}");
            
            WriteObject(new FileInfo(fullPath));
        }
    }
}
