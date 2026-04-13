using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class MessagingAndFileRequestTests
    {
        [Fact]
        public void SendEmailRequest_MarksEmailAsSent()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var emailId = service.Create(new Entity("email")
            {
                ["subject"] = "Test Email",
                ["description"] = "Body"
            });

            service.Execute(new SendEmailRequest
            {
                EmailId = emailId,
                IssueSend = true
            });

            var email = service.Retrieve("email", emailId, new ColumnSet("statecode", "statuscode"));
            Assert.Equal(1, email.GetAttributeValue<OptionSetValue>("statecode").Value);
            Assert.Equal(3, email.GetAttributeValue<OptionSetValue>("statuscode").Value);
        }

        [Fact]
        public void InstantiateTemplate_CreatesEmailFromTemplate()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var templateId = service.Create(new Entity("template") { ["subject"] = "Welcome {!contact:fullname;}", ["body"] = "Hello {!contact:fullname;}!" });
            var contactId = service.Create(new Entity("contact") { ["fullname"] = "John Doe" });

            var response = (InstantiateTemplateResponse)service.Execute(new InstantiateTemplateRequest
            {
                TemplateId = templateId,
                ObjectId = contactId,
                ObjectType = "contact"
            });

            var collection = (EntityCollection)response["EntityCollection"];
            Assert.Single(collection.Entities);
            Assert.Equal("Welcome John Doe", collection.Entities[0].GetAttributeValue<string>("subject"));
            Assert.Equal("Hello John Doe!", collection.Entities[0].GetAttributeValue<string>("description"));
        }

        [Fact]
        public void SendEmailFromTemplate_CreatesEmail()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var templateId = service.Create(new Entity("template") { ["subject"] = "Welcome {!contact:fullname;}", ["body"] = "Body {!contact:fullname;}" });
            var contactId = service.Create(new Entity("contact") { ["fullname"] = "Test User" });

            var response = (SendEmailFromTemplateResponse)service.Execute(new SendEmailFromTemplateRequest
            {
                TemplateId = templateId,
                RegardingId = contactId,
                RegardingType = "contact",
                Target = new Entity("email")
            });

            var emailId = (Guid)response["Id"];
            Assert.NotEqual(Guid.Empty, emailId);

            var email = service.Retrieve("email", emailId, new ColumnSet("subject", "description", "statecode", "statuscode"));
            Assert.Equal("Welcome Test User", email.GetAttributeValue<string>("subject"));
            Assert.Equal("Body Test User", email.GetAttributeValue<string>("description"));
            Assert.Equal(1, email.GetAttributeValue<OptionSetValue>("statecode").Value);
            Assert.Equal(3, email.GetAttributeValue<OptionSetValue>("statuscode").Value);
        }

        [Fact]
        public void SendFax_ExecutesWithoutError()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var response = service.Execute(new OrganizationRequest("SendFax"));
            Assert.NotNull(response);
        }

        [Fact]
        public void DeleteFile_ExecutesWithoutError()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var request = new OrganizationRequest("DeleteFile");
            request["FileId"] = Guid.NewGuid();

            var response = service.Execute(request);
            Assert.NotNull(response);
        }

        [Fact]
        public void DownloadBlock_ReturnsEmptyData()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var request = new OrganizationRequest("DownloadBlock");
            request["FileContinuationToken"] = "token123";

            var response = service.Execute(request);
            Assert.NotNull(response["Data"]);
        }

        [Fact]
        public void FileUpload_FullFlow_StoresBinaryData()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var recordId = service.Create(new Entity("annotation") { ["subject"] = "Test" });

            var initResponse = (InitializeFileBlocksUploadResponse)service.Execute(
                new InitializeFileBlocksUploadRequest
                {
                    Target = new EntityReference("annotation", recordId),
                    FileAttributeName = "documentbody"
                });

            var token = (string)initResponse.Results["FileContinuationToken"];
            Assert.False(string.IsNullOrEmpty(token));

            var block1 = new byte[] { 1, 2, 3, 4, 5 };
            var block2 = new byte[] { 6, 7, 8, 9, 10 };

            service.Execute(new UploadBlockRequest
            {
                FileContinuationToken = token,
                BlockData = block1,
                BlockId = "block1"
            });

            service.Execute(new UploadBlockRequest
            {
                FileContinuationToken = token,
                BlockData = block2,
                BlockId = "block2"
            });

            service.Execute(new CommitFileBlocksUploadRequest
            {
                FileContinuationToken = token,
                FileName = "test.bin",
                BlockList = new[] { "block1", "block2" }
            });

            var stored = env.GetBinaryAttribute("annotation", recordId, "documentbody");
            Assert.NotNull(stored);
            Assert.Equal(10, stored!.Length);
            Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, stored);
        }

        [Fact]
        public void DownloadBlock_ReturnsUploadedFileData()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var recordId = service.Create(new Entity("annotation") { ["subject"] = "Test" });
            var fileData = new byte[] { 42, 43, 44, 45 };
            env.SetBinaryAttribute("annotation", recordId, "documentbody", fileData);

            var request = new OrganizationRequest("DownloadBlock");
            request["Target"] = new EntityReference("annotation", recordId);
            request["FileAttributeName"] = "documentbody";
            var response = service.Execute(request);
            var data = (byte[])response["Data"];

            Assert.Equal(fileData, data);
        }

        [Fact]
        public void DeleteFile_RemovesBinaryData()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var recordId = service.Create(new Entity("annotation") { ["subject"] = "Test" });
            env.SetBinaryAttribute("annotation", recordId, "documentbody", new byte[] { 1, 2, 3 });

            var request = new OrganizationRequest("DeleteFile");
            request["Target"] = new EntityReference("annotation", recordId);
            request["FileAttributeName"] = "documentbody";
            service.Execute(request);

            var data = env.GetBinaryAttribute("annotation", recordId, "documentbody");
            Assert.Null(data);
        }

        [Fact]
        public void SendFax_MarksFaxAsSent()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var faxId = service.Create(new Entity("fax") { ["subject"] = "Test Fax" });

            service.Execute(new SendFaxRequest { FaxId = faxId });

            var fax = service.Retrieve("fax", faxId, new ColumnSet("statecode", "statuscode"));
            Assert.Equal(1, fax.GetAttributeValue<OptionSetValue>("statecode").Value);
            Assert.Equal(3, fax.GetAttributeValue<OptionSetValue>("statuscode").Value);
        }

        [Fact]
        public void SendTemplate_CreatesEmailRecord()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var senderId = service.Create(new Entity("systemuser") { ["fullname"] = "Sender" });
            var contactId = service.Create(new Entity("contact") { ["fullname"] = "Template Contact" });
            var templateId = service.Create(new Entity("template")
            {
                ["subject"] = "Hello {!contact:fullname;}",
                ["body"] = "Regarding {!contact:fullname;}"
            });

            service.Execute(new SendTemplateRequest
            {
                TemplateId = templateId,
                RegardingId = contactId,
                RegardingType = "contact",
                RecipientIds = new[] { contactId },
                RecipientType = "contact",
                Sender = new EntityReference("systemuser", senderId)
            });

            var emails = service.RetrieveMultiple(new QueryExpression("email") { ColumnSet = new ColumnSet(true) });
            Assert.Single(emails.Entities);
            Assert.Equal("Hello Template Contact", emails.Entities[0].GetAttributeValue<string>("subject"));
            Assert.Equal("Regarding Template Contact", emails.Entities[0].GetAttributeValue<string>("description"));
            Assert.Equal(1, emails.Entities[0].GetAttributeValue<OptionSetValue>("statecode").Value);
            Assert.Equal(3, emails.Entities[0].GetAttributeValue<OptionSetValue>("statuscode").Value);
            Assert.Single(emails.Entities[0].GetAttributeValue<EntityCollection>("to").Entities);
            Assert.Single(emails.Entities[0].GetAttributeValue<EntityCollection>("from").Entities);
        }

        [Fact]
        public void BackgroundSendEmail_MarksEmailAsSent()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var emailId = service.Create(new Entity("email") { ["subject"] = "Background Email" });

            var request = new OrganizationRequest("BackgroundSendEmail");
            request["EntityId"] = emailId;
            var response = service.Execute(request);

            var asyncOperationId = (Guid)response["EntityId"];
            Assert.NotEqual(Guid.Empty, asyncOperationId);
            var email = service.Retrieve("email", emailId, new ColumnSet("statecode"));
            Assert.Equal(1, email.GetAttributeValue<OptionSetValue>("statecode").Value);

            var asyncOperation = service.Retrieve("asyncoperation", asyncOperationId, new ColumnSet("name", "message"));
            Assert.Equal("Background Send Email", asyncOperation.GetAttributeValue<string>("name"));
            Assert.Equal("BackgroundSendEmail", asyncOperation.GetAttributeValue<string>("message"));
        }

        [Fact]
        public void GetTrackingTokenEmail_ReturnsToken()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var request = new OrganizationRequest("GetTrackingTokenEmail");
            var response = service.Execute(request);

            var token = (string)response["TrackingToken"];
            Assert.StartsWith("CRM:", token);
        }

        [Fact]
        public void CheckIncomingEmail_ReturnsTrue()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var request = new OrganizationRequest("CheckIncomingEmail");
            request["MessageId"] = "test@example.com";
            request["Subject"] = "Test";
            var response = service.Execute(request);

            Assert.True((bool)response["ShouldDeliver"]);
        }

        [Fact]
        public void CheckPromoteEmail_ReturnsTrue()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var request = new OrganizationRequest("CheckPromoteEmail");
            request["MessageId"] = "test@example.com";
            request["Subject"] = "Test";
            var response = service.Execute(request);

            Assert.True((bool)response["ShouldPromote"]);
        }
    }
}
