using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using HeyRed.Mime;
using System;
using System.Collections.Generic;
using System.IO;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Helper class for uploading files to Dataverse file columns.
    /// </summary>
    internal static class FileUploadHelper
    {
        private const string UPLOAD_BLOCK_REQUEST = "UploadBlock";
        private const int BLOCK_SIZE = 4 * 1024 * 1024; // 4MB blocks

        /// <summary>
        /// Uploads a file to a Dataverse file column using block-based upload.
        /// </summary>
        /// <param name="connection">The organization service connection.</param>
        /// <param name="tableName">The logical name of the table.</param>
        /// <param name="recordId">The ID of the record.</param>
        /// <param name="columnName">The logical name of the file column.</param>
        /// <param name="fileData">The file content as a byte array.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="writeVerbose">Action to write verbose messages.</param>
        public static void UploadFile(
            IOrganizationService connection,
            string tableName,
            Guid recordId,
            string columnName,
            byte[] fileData,
            string fileName,
            Action<string> writeVerbose)
        {
            // Step 1: Initialize the upload
            var initRequest = new InitializeFileBlocksUploadRequest
            {
                Target = new EntityReference(tableName, recordId),
                FileAttributeName = columnName,
                FileName = fileName
            };

            var initResponse = (InitializeFileBlocksUploadResponse)connection.Execute(initRequest);
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

                connection.Execute(uploadRequest);

                offset += currentBlockSize;
                blockNumber++;

                writeVerbose?.Invoke($"Uploaded block {blockNumber}/{Math.Ceiling((double)fileData.Length / BLOCK_SIZE)}");
            }

            // Step 3: Commit the upload
            var commitRequest = new CommitFileBlocksUploadRequest
            {
                FileContinuationToken = fileContinuationToken,
                BlockList = blockList.ToArray(),
                FileName = fileName,
                MimeType = MimeTypesMap.GetMimeType(Path.GetExtension(fileName))
            };

            connection.Execute(commitRequest);
        }

        /// <summary>
        /// Finds and validates a file in a FileDirectory subfolder matching the given file ID.
        /// </summary>
        /// <param name="fileDirectory">The base file directory path.</param>
        /// <param name="fileId">The GUID identifying the file subfolder.</param>
        /// <returns>The full path to the single file found in the subfolder.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the subfolder or file is not found.</exception>
        /// <exception cref="InvalidOperationException">Thrown when multiple files are found in the subfolder.</exception>
        public static string FindFileInDirectory(string fileDirectory, Guid fileId)
        {
            string subfolderPath = Path.Combine(fileDirectory, fileId.ToString());

            if (!Directory.Exists(subfolderPath))
            {
                throw new FileNotFoundException($"File directory subfolder not found for file ID '{fileId}'. Expected path: {subfolderPath}");
            }

            string[] files = Directory.GetFiles(subfolderPath);

            if (files.Length == 0)
            {
                throw new FileNotFoundException($"No files found in file directory subfolder for file ID '{fileId}'. Path: {subfolderPath}");
            }

            if (files.Length > 1)
            {
                throw new InvalidOperationException($"Multiple files found in file directory subfolder for file ID '{fileId}'. Expected exactly one file. Path: {subfolderPath}");
            }

            return files[0];
        }

        /// <summary>
        /// Loads a file from a FileDirectory subfolder and returns its content and filename.
        /// </summary>
        /// <param name="fileDirectory">The base file directory path.</param>
        /// <param name="fileId">The GUID identifying the file subfolder.</param>
        /// <returns>A tuple containing the file content as bytes and the filename.</returns>
        public static (byte[] content, string fileName) LoadFileFromDirectory(string fileDirectory, Guid fileId)
        {
            string filePath = FindFileInDirectory(fileDirectory, fileId);
            byte[] content = File.ReadAllBytes(filePath);
            string fileName = Path.GetFileName(filePath);
            return (content, fileName);
        }
    }
}
