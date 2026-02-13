using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using HeyRed.Mime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Uploads file data to a Dataverse file column.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseFileData", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(void))]
    public class SetDataverseFileDataCmdlet : OrganizationServiceCmdlet
    {
        private const string PARAMSET_FILEPATH = "FilePath";
        private const string PARAMSET_BYTES = "Bytes";
        private const string PARAMSET_BYTESTREAM = "ByteStream";
        private const string UPLOAD_BLOCK_REQUEST = "UploadBlock";
        private const int BLOCK_SIZE = 4 * 1024 * 1024; // 4MB blocks

        // For byte stream mode, we upload blocks incrementally
        private List<byte> _byteStreamBuffer;
        private bool _byteStreamMode;
        private string _fileContinuationToken;
        private List<string> _blockList;
        private int _blockNumber;

        /// <summary>
        /// Gets or sets the logical name of the table containing the file column.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of table containing the file column")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        [Alias("EntityName", "LogicalName")]
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets the ID of the record to update with the file.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the record to update with the file")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the logical name of the file column.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of the file column")]
        [ArgumentCompleter(typeof(ColumnNameArgumentCompleter))]
        public string ColumnName { get; set; }

        /// <summary>
        /// Gets or sets the file path of the file to upload.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_FILEPATH, Mandatory = true, HelpMessage = "File path of the file to upload")]
        public string FilePath { get; set; }

        /// <summary>
        /// Gets or sets the file content as a byte array.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_BYTES, Mandatory = true, ValueFromPipeline = true, HelpMessage = "File content as a byte array")]
        public byte[] FileContent { get; set; }

        /// <summary>
        /// Gets or sets the byte stream input from the pipeline.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_BYTESTREAM, Mandatory = true, ValueFromPipeline = true, HelpMessage = "Byte stream input from the pipeline")]
        public byte InputByte { get; set; }

        /// <summary>
        /// Gets or sets the filename to use when uploading from byte array.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_BYTES, HelpMessage = "Filename to use when uploading from byte array")]
        [Parameter(ParameterSetName = PARAMSET_BYTESTREAM, HelpMessage = "Filename to use when uploading from byte stream")]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the MIME type for the file. If not specified, it will be automatically determined from the file extension.
        /// </summary>
        [Parameter(HelpMessage = "MIME type for the file (e.g., 'application/pdf', 'image/png'). If not specified, automatically determined from file extension.")]
        public string MimeType { get; set; }

        /// <summary>
        /// Begins processing - initialize byte stream buffer if in ByteStream mode.
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            if (ParameterSetName == PARAMSET_BYTESTREAM)
            {
                _byteStreamMode = true;
                _byteStreamBuffer = new List<byte>();
                _blockList = new List<string>();
                _blockNumber = 0;
                
                string fileName = FileName ?? "file.bin";
                WriteVerbose($"Initializing byte stream upload for file '{fileName}'");
                
                // Initialize the upload at the beginning
                var initRequest = new InitializeFileBlocksUploadRequest
                {
                    Target = new EntityReference(TableName, Id),
                    FileAttributeName = ColumnName,
                    FileName = fileName
                };

                var initResponse = (InitializeFileBlocksUploadResponse)Connection.Execute(initRequest);
                _fileContinuationToken = initResponse.FileContinuationToken;
                
                WriteVerbose($"Upload initialized with continuation token");
            }
        }

        /// <summary>
        /// Processes each record in the pipeline to upload file data.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // In byte stream mode, accumulate bytes and upload when we reach block size
            if (_byteStreamMode)
            {
                _byteStreamBuffer.Add(InputByte);
                
                // Upload block when we reach block size
                if (_byteStreamBuffer.Count >= BLOCK_SIZE)
                {
                    UploadBlock();
                }
                
                return;
            }

            byte[] fileData;
            string fileName;

            // Get file data and name based on parameter set
            switch (ParameterSetName)
            {
                case PARAMSET_FILEPATH:
                    string expandedPath = SessionState.Path.GetUnresolvedProviderPathFromPSPath(FilePath);
                    if (!File.Exists(expandedPath))
                    {
                        WriteError(new ErrorRecord(
                            new FileNotFoundException($"File not found: {expandedPath}"),
                            "FileNotFound",
                            ErrorCategory.ObjectNotFound,
                            expandedPath));
                        return;
                    }
                    fileData = File.ReadAllBytes(expandedPath);
                    fileName = Path.GetFileName(expandedPath);
                    break;

                case PARAMSET_BYTES:
                    fileData = FileContent;
                    fileName = FileName ?? "file.bin";
                    break;

                default:
                    return;
            }

            if (!ShouldProcess($"{TableName} record {Id}, column {ColumnName}", $"Upload file '{fileName}' ({fileData.Length} bytes)"))
            {
                return;
            }

            try
            {
                UploadFile(fileData, fileName);
                WriteVerbose($"Successfully uploaded file '{fileName}' ({fileData.Length} bytes) to {TableName} record {Id}, column {ColumnName}");
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "FileUploadError", ErrorCategory.WriteError, Id));
            }
        }

        /// <summary>
        /// Ends processing - upload any remaining bytes and commit the upload if in ByteStream mode.
        /// </summary>
        protected override void EndProcessing()
        {
            base.EndProcessing();

            if (_byteStreamMode)
            {
                string fileName = FileName ?? "file.bin";
                
                // Check ShouldProcess before committing
                if (!ShouldProcess($"{TableName} record {Id}, column {ColumnName}", $"Upload file '{fileName}' from byte stream"))
                {
                    return;
                }
                
                try
                {
                    // Upload any remaining bytes in buffer (last partial block)
                    if (_byteStreamBuffer != null && _byteStreamBuffer.Count > 0)
                    {
                        UploadBlock();
                    }
                    
                    // Commit the upload
                    WriteVerbose($"Committing upload with {_blockList.Count} blocks");
                    
                    var commitRequest = new CommitFileBlocksUploadRequest
                    {
                        FileContinuationToken = _fileContinuationToken,
                        BlockList = _blockList.ToArray(),
                        FileName = fileName,
                        MimeType = DetermineMimeType(fileName)
                    };

                    Connection.Execute(commitRequest);
                    
                    WriteVerbose($"Successfully uploaded file '{fileName}' from byte stream to {TableName} record {Id}, column {ColumnName}");
                }
                catch (Exception ex)
                {
                    WriteError(new ErrorRecord(ex, "FileUploadError", ErrorCategory.WriteError, Id));
                }
            }
        }
        
        /// <summary>
        /// Uploads the current buffer as a block.
        /// </summary>
        private void UploadBlock()
        {
            if (_byteStreamBuffer.Count == 0)
            {
                return;
            }
            
            byte[] blockData = _byteStreamBuffer.ToArray();
            string blockId = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"block-{_blockNumber:D6}"));
            _blockList.Add(blockId);

            var uploadRequest = new OrganizationRequest(UPLOAD_BLOCK_REQUEST);
            uploadRequest.Parameters["BlockId"] = blockId;
            uploadRequest.Parameters["BlockData"] = blockData;
            uploadRequest.Parameters["FileContinuationToken"] = _fileContinuationToken;

            Connection.Execute(uploadRequest);
            
            _blockNumber++;
            WriteVerbose($"Uploaded block {_blockNumber} with {blockData.Length} bytes");
            
            // Clear buffer for next block
            _byteStreamBuffer.Clear();
        }

        private void UploadFile(byte[] fileData, string fileName)
        {
            // Step 1: Initialize the upload
            var initRequest = new InitializeFileBlocksUploadRequest
            {
                Target = new EntityReference(TableName, Id),
                FileAttributeName = ColumnName,
                FileName = fileName
            };

            var initResponse = (InitializeFileBlocksUploadResponse)Connection.Execute(initRequest);
            string fileContinuationToken = initResponse.FileContinuationToken;

            // Step 2: Upload blocks
            var blockList = new List<string>();
            int blockNumber = 0;
            int offset = 0;

            while (offset < fileData.Length)
            {
                int currentBlockSize = Math.Min(BLOCK_SIZE, fileData.Length - offset);
                byte[] blockData = new byte[currentBlockSize];
                Array.Copy(fileData, offset, blockData, 0, currentBlockSize);

                string blockId = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"block-{blockNumber:D6}"));
                blockList.Add(blockId);

                var uploadRequest = new OrganizationRequest(UPLOAD_BLOCK_REQUEST);
                uploadRequest.Parameters["BlockId"] = blockId;
                uploadRequest.Parameters["BlockData"] = blockData;
                uploadRequest.Parameters["FileContinuationToken"] = fileContinuationToken;

                Connection.Execute(uploadRequest);

                offset += currentBlockSize;
                blockNumber++;

                WriteVerbose($"Uploaded block {blockNumber}/{Math.Ceiling((double)fileData.Length / BLOCK_SIZE)}");
            }

            // Step 3: Commit the upload
            var commitRequest = new CommitFileBlocksUploadRequest
            {
                FileContinuationToken = fileContinuationToken,
                BlockList = blockList.ToArray(),
                FileName = fileName,
                MimeType = DetermineMimeType(fileName)
            };

            Connection.Execute(commitRequest);
        }

        /// <summary>
        /// Determines the MIME type for a file. Uses the manually specified MimeType parameter if provided,
        /// otherwise automatically determines it from the file extension using MimeTypesMap.
        /// </summary>
        private string DetermineMimeType(string fileName)
        {
            // If MimeType parameter was explicitly set, use it
            if (!string.IsNullOrEmpty(MimeType))
            {
                WriteVerbose($"Using manually specified MIME type: {MimeType}");
                return MimeType;
            }

            // Otherwise, automatically determine from file extension
            string mimeType = MimeTypesMap.GetMimeType(Path.GetExtension(fileName));
            WriteVerbose($"Automatically determined MIME type for '{fileName}': {mimeType}");
            return mimeType;
        }
    }
}
