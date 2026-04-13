# Binary & File Operations

Fake4Dataverse supports storing and retrieving binary data (images and files) for entity attributes. Both direct helper methods and the block-based upload/download requests used by the Dataverse SDK are supported.

---

## Direct Binary Storage

The simplest approach uses the helper methods on `FakeDataverseEnvironment`:

```csharp
var env = new FakeDataverseEnvironment();
var service = env.CreateOrganizationService();
var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });

// Store a binary attribute (e.g., an entity image)
byte[] imageData = File.ReadAllBytes("logo.png");
env.SetBinaryAttribute("account", accountId, "entityimage", imageData);

// Retrieve the binary attribute
byte[]? retrieved = env.GetBinaryAttribute("account", accountId, "entityimage");

Assert.NotNull(retrieved);
Assert.Equal(imageData.Length, retrieved.Length);
```

- **`SetBinaryAttribute(entityName, entityId, attributeName, byte[] data)`** — stores a copy of the data.
- **`GetBinaryAttribute(entityName, entityId, attributeName)`** — returns a copy of the data, or `null` if not set.
- **`RemoveBinaryAttribute(entityName, entityId, attributeName)`** — removes the stored data and returns `true` if the attribute existed and was removed, `false` otherwise.

Both methods are on `FakeDataverseEnvironment`. Data is deep-cloned on both store and retrieve to prevent aliasing bugs.

```csharp
// Remove example
bool removed = env.RemoveBinaryAttribute("account", accountId, "entityimage");
Assert.True(removed);

// Second removal returns false
bool removedAgain = env.RemoveBinaryAttribute("account", accountId, "entityimage");
Assert.False(removedAgain);
```

---

## File Upload Session (Block-Based)

For larger files or when testing code that uses the Dataverse file upload protocol, use the block-based request handlers:

### 1. Initialize Upload

```csharp
var initResponse = (InitializeFileBlocksUploadResponse)service.Execute(
    new InitializeFileBlocksUploadRequest
    {
        Target = new EntityReference("annotation", annotationId),
        FileAttributeName = "documentbody"
    });

string token = initResponse.FileContinuationToken;
```

### 2. Upload Blocks

```csharp
service.Execute(new UploadBlockRequest
{
    FileContinuationToken = token,
    BlockData = chunkBytes,
    BlockId = Convert.ToBase64String(BitConverter.GetBytes(blockIndex))
});
```

You can upload multiple blocks sequentially. Each block is stored in order.

### 3. Commit Upload

```csharp
var commitResponse = (CommitFileBlocksUploadResponse)service.Execute(
    new CommitFileBlocksUploadRequest
    {
        FileContinuationToken = token,
        FileName = "document.pdf",
        MimeType = "application/pdf",
        BlockList = blockIds.ToArray()
    });
```

All uploaded blocks are assembled and stored as a single binary attribute.

### 4. Download

```csharp
var downloadResponse = (DownloadBlockResponse)service.Execute(
    new DownloadBlockRequest
    {
        FileContinuationToken = token
    });
```

### 5. Delete

```csharp
service.Execute(new OrganizationRequest("DeleteFile")
{
    ["FileAttributeName"] = "documentbody",
    ["Target"] = new EntityReference("annotation", annotationId)
});
```

---

## Complete Example

```csharp
var env = new FakeDataverseEnvironment();
var service = env.CreateOrganizationService();
var id = service.Create(new Entity("account") { ["name"] = "Test" });

// Store binary data
var original = new byte[] { 0x01, 0x02, 0x03, 0x04 };
env.SetBinaryAttribute("account", id, "entityimage", original);

// Retrieve and verify
var result = env.GetBinaryAttribute("account", id, "entityimage");
Assert.Equal(original, result);

// Overwrite
var updated = new byte[] { 0xAA, 0xBB };
env.SetBinaryAttribute("account", id, "entityimage", updated);
result = env.GetBinaryAttribute("account", id, "entityimage");
Assert.Equal(updated, result);
```

---

## Tips

- **Deep cloning** — binary data is always cloned on store and retrieve, so modifying the returned array does not affect stored data.
- **Direct vs. block-based** — use direct methods for simple test setups; use block-based requests when testing code that relies on the SDK file upload protocol.
- **No size limits** — the fake has no file size restrictions, unlike Dataverse which enforces per-attribute limits.

---

## Annotation Block-Based Upload & Download

`annotation` records store arbitrary file attachments in the `documentbody` attribute. The same
block-based protocol used for file columns is available via dedicated annotation request handlers:

### Upload

```csharp
// 1. Initialize
var initResponse = (InitializeAnnotationBlocksUploadResponse)service.Execute(
    new InitializeAnnotationBlocksUploadRequest
    {
        Target = new EntityReference("annotation", annotationId)
    });

string token = initResponse.FileContinuationToken;

// 2. Upload blocks
service.Execute(new UploadBlockRequest
{
    FileContinuationToken = token,
    BlockData  = chunkBytes,
    BlockId    = Convert.ToBase64String(BitConverter.GetBytes(0))
});

// 3. Commit
service.Execute(new CommitAnnotationBlocksUploadRequest
{
    FileContinuationToken = token,
    FileName  = "report.pdf",
    MimeType  = "application/pdf",
    BlockList = new[] { Convert.ToBase64String(BitConverter.GetBytes(0)) }
});
```

### Download

```csharp
var initDl = (InitializeAnnotationBlocksDownloadResponse)service.Execute(
    new InitializeAnnotationBlocksDownloadRequest
    {
        Target = new EntityReference("annotation", annotationId)
    });

var downloadResponse = (DownloadBlockResponse)service.Execute(
    new DownloadBlockRequest
    {
        FileContinuationToken = initDl.FileContinuationToken
    });

byte[] fileData = downloadResponse.Data;
```

---

## Activity MIME Attachment Block-Based Upload & Download

`activitymimeattachment` records (email attachments) store content in the `body` attribute and
also support block-based upload/download:

```csharp
// Initialize upload
var initResponse = (InitializeAttachmentBlocksUploadResponse)service.Execute(
    new InitializeAttachmentBlocksUploadRequest
    {
        Target = new EntityReference("activitymimeattachment", attachmentId)
    });

string token = initResponse.FileContinuationToken;

// Upload + commit (same pattern as annotation/file above)
service.Execute(new UploadBlockRequest
{
    FileContinuationToken = token,
    BlockData  = fileBytes,
    BlockId    = Convert.ToBase64String(BitConverter.GetBytes(0))
});

service.Execute(new CommitAttachmentBlocksUploadRequest
{
    FileContinuationToken = token,
    FileName  = "attachment.txt",
    MimeType  = "text/plain",
    BlockList = new[] { Convert.ToBase64String(BitConverter.GetBytes(0)) }
});

// Download
var initDl = (InitializeAttachmentBlocksDownloadResponse)service.Execute(
    new InitializeAttachmentBlocksDownloadRequest
    {
        Target = new EntityReference("activitymimeattachment", attachmentId)
    });

var downloadResponse = (DownloadBlockResponse)service.Execute(
    new DownloadBlockRequest { FileContinuationToken = initDl.FileContinuationToken });

byte[] content = downloadResponse.Data;
```

All three block-based pipelines (file column, annotation, attachment) share the same
`UploadBlockRequest` / `DownloadBlockRequest` handler under the hood and produce identical
behaviour: blocks are assembled in order and stored as a single binary attribute.
