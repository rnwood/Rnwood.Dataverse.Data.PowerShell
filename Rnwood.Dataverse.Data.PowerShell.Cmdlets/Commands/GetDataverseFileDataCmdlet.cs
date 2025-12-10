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
    [OutputType(typeof(byte[]), typeof(FileInfo), typeof(byte))]
    public class GetDataverseFileDataCmdlet : OrganizationServiceCmdlet
    {
        private const string PARAMSET_FILEPATH = "FilePath";
        private const string PARAMSET_FOLDER = "Folder";
        private const string PARAMSET_BYTES = "Bytes";
        private const string PARAMSET_BYTESTREAM = "ByteStream";
        private const string DOWNLOAD_BLOCK_REQUEST = "DownloadBlock";
        private const int DEFAULT_BLOCK_SIZE = 4 * 1024 * 1024; // 4MB blocks

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
        /// Gets or sets whether to output the file content as a byte stream to the pipeline (one byte at a time).
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_BYTESTREAM, Mandatory = true, HelpMessage = "Output the file content as a byte stream to the pipeline (one byte at a time)")]
        public SwitchParameter AsByteStream { get; set; }

        /// <summary>
        /// Gets or sets the block size for downloading files. Default is 4MB.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Block size for downloading files in bytes. Default is 4MB (4194304 bytes)")]
        public int BlockSize { get; set; } = DEFAULT_BLOCK_SIZE;

        /// <summary>
        /// Processes each record in the pipeline to download file data.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            try
            {
                WriteVerbose($"Initializing file download for column '{ColumnName}' on record {Id} in table '{TableName}'");
                
                // Initialize file blocks download to get file information
                var initRequest = new InitializeFileBlocksDownloadRequest
                {
                    Target = new EntityReference(TableName, Id),
                    FileAttributeName = ColumnName
                };

                var initResponse = (InitializeFileBlocksDownloadResponse)Connection.Execute(initRequest);

                WriteVerbose($"File initialized - Size: {initResponse.FileSizeInBytes} bytes, Name: {initResponse.FileName}, Chunking supported: {initResponse.IsChunkingSupported}");

                if (initResponse.FileSizeInBytes == 0)
                {
                    WriteWarning($"File column '{ColumnName}' on record {Id} in table '{TableName}' is empty or does not contain a file.");
                    return;
                }

                // Download the file data
                WriteVerbose($"Starting file download with continuation token using block size of {BlockSize} bytes");
                
                // For byte stream, we handle differently - output bytes as we download
                if (ParameterSetName == PARAMSET_BYTESTREAM)
                {
                    DownloadFileDataAsByteStream(initResponse.FileContinuationToken, initResponse.FileSizeInBytes, initResponse.IsChunkingSupported);
                    WriteVerbose($"File download completed - All bytes streamed to pipeline");
                }
                else
                {
                    byte[] fileData = DownloadFileData(initResponse.FileContinuationToken, initResponse.FileSizeInBytes, initResponse.IsChunkingSupported);
                    WriteVerbose($"File download completed - Downloaded {fileData.Length} bytes");

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
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(
                    new Exception($"Failed to download file from column '{ColumnName}' on record {Id} in table '{TableName}': {ex.Message}", ex),
                    "FileDownloadError",
                    ErrorCategory.ReadError,
                    Id));
                throw;
            }
        }

        private byte[] DownloadFileData(string fileContinuationToken, long fileSizeInBytes, bool isChunkingSupported)
        {
            // If chunking is not supported, block size will be full size of the file.
            long blockSizeDownload = !isChunkingSupported ? fileSizeInBytes : BlockSize;

            // File size may be smaller than defined block size
            if (fileSizeInBytes < blockSizeDownload)
            {
                blockSizeDownload = fileSizeInBytes;
            }

            WriteVerbose($"Using block size: {blockSizeDownload} bytes for download");

            using (var memoryStream = new MemoryStream((int)fileSizeInBytes))
            {
                long offset = 0;
                int blockNumber = 0;

                while (fileSizeInBytes > 0)
                {
                    blockNumber++;
                    WriteVerbose($"Downloading block {blockNumber} at offset {offset}, remaining: {fileSizeInBytes} bytes");
                    
                    var downloadRequest = new OrganizationRequest(DOWNLOAD_BLOCK_REQUEST);
                    downloadRequest.Parameters["FileContinuationToken"] = fileContinuationToken;
                    downloadRequest.Parameters["Offset"] = offset;
                    downloadRequest.Parameters["BlockLength"] = blockSizeDownload;

                    var downloadResponse = Connection.Execute(downloadRequest);

                    if (!downloadResponse.Results.Contains("Data"))
                    {
                        WriteVerbose($"No data in response");
                        break;
                    }

                    var data = downloadResponse.Results["Data"] as byte[];
                    if (data == null || data.Length == 0)
                    {
                        WriteVerbose($"Empty data block received");
                        break;
                    }

                    WriteVerbose($"Downloaded {data.Length} bytes in block {blockNumber}");
                    memoryStream.Write(data, 0, data.Length);

                    // Subtract the amount downloaded,
                    // which may make fileSizeInBytes < 0 and indicate
                    // no further blocks to download
                    fileSizeInBytes -= blockSizeDownload;
                    // Increment the offset to start at the beginning of the next block.
                    offset += blockSizeDownload;
                }

                WriteVerbose($"Total download: {memoryStream.Length} bytes in {blockNumber} blocks");
                return memoryStream.ToArray();
            }
        }

        private void DownloadFileDataAsByteStream(string fileContinuationToken, long fileSizeInBytes, bool isChunkingSupported)
        {
            // If chunking is not supported, block size will be full size of the file.
            long blockSizeDownload = !isChunkingSupported ? fileSizeInBytes : BlockSize;

            // File size may be smaller than defined block size
            if (fileSizeInBytes < blockSizeDownload)
            {
                blockSizeDownload = fileSizeInBytes;
            }

            WriteVerbose($"Using block size: {blockSizeDownload} bytes for byte stream download");

            long offset = 0;
            int blockNumber = 0;
            long totalBytesStreamed = 0;

            while (fileSizeInBytes > 0)
            {
                blockNumber++;
                WriteVerbose($"Downloading block {blockNumber} at offset {offset}, remaining: {fileSizeInBytes} bytes");
                
                var downloadRequest = new OrganizationRequest(DOWNLOAD_BLOCK_REQUEST);
                downloadRequest.Parameters["FileContinuationToken"] = fileContinuationToken;
                downloadRequest.Parameters["Offset"] = offset;
                downloadRequest.Parameters["BlockLength"] = blockSizeDownload;

                var downloadResponse = Connection.Execute(downloadRequest);

                if (!downloadResponse.Results.Contains("Data"))
                {
                    WriteVerbose($"No data in response");
                    break;
                }

                var data = downloadResponse.Results["Data"] as byte[];
                if (data == null || data.Length == 0)
                {
                    WriteVerbose($"Empty data block received");
                    break;
                }

                WriteVerbose($"Downloaded {data.Length} bytes in block {blockNumber}, streaming to pipeline");
                
                // Stream each byte to the pipeline
                foreach (byte b in data)
                {
                    WriteObject(b);
                    totalBytesStreamed++;
                }

                // Subtract the amount downloaded,
                // which may make fileSizeInBytes < 0 and indicate
                // no further blocks to download
                fileSizeInBytes -= blockSizeDownload;
                // Increment the offset to start at the beginning of the next block.
                offset += blockSizeDownload;
            }

            WriteVerbose($"Total streamed: {totalBytesStreamed} bytes in {blockNumber} blocks");
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
