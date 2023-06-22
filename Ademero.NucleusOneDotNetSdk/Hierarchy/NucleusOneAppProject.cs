﻿using Ademero.NucleusOneDotNetSdk.Common;
using Ademero.NucleusOneDotNetSdk.Common.Strings;
using Ademero.NucleusOneDotNetSdk.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Ademero.NucleusOneDotNetSdk.Hierarchy
{
    /// <summary>
    /// The organization that this project belongs to.
    /// </summary>
    public class NucleusOneAppProject : NucleusOneAppDependent
    {
        /// <summary>
        /// The organization to perform task operations on.
        /// </summary>
        public NucleusOneAppOrganization Organization { get; }

        /// <summary>
        /// The project's ID.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Creates an instance of the <see cref="NucleusOneAppProject"/> class.
        /// </summary>
        /// <param name="organization">The organization to perform task operations on.</param>
        /// <param name="id">The project's ID.</param>
        public NucleusOneAppProject(NucleusOneAppOrganization organization, string id) : base(organization.App)
        {
            Organization = organization;
            Id = id;

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Value cannot be blank.", nameof(id));
            }
        }

        /// <summary>
        /// Gets a Document Upload reservation for this project.
        ///
        /// Call this *only* if you want to handle the upload process of a document manually; otherwise,
        /// you likely want to call <see cref="UploadDocument"/>, instead, which handles the entire process.
        /// </summary>
        public async Task<DocumentUpload> GetDocumentUploadReservation()
        {
            var responseBody = await Http.ExecuteGetRequestWithTextResponse(
                apiRelativeUrlPath: ApiPaths.OrganizationsProjectsDocumentUploadsFormat
                    .ReplaceOrgIdAndProjectIdPlaceholdersUsingProject(this),
                app: Organization.App
            );
            var apiModel = ApiModel.DocumentUpload.FromJson(responseBody);
            return Util.DefineN1AppInScope(App, () => DocumentUpload.FromApiModel(apiModel));
        }

        /// <summary>
        /// Uploads a file to Google Cloud Storage, in chunks.
        /// </summary>
        /// <param name="gcsPublicReservationUrl">The URL for reserving space in Google Cloud Storage.</param>
        /// <param name="contentType">The content type of the file.</param>
        /// <param name="file">The file to upload.</param>
        /// <returns>A task representing the asynchronous upload operation.</returns>
        private async Task UploadFileToGcsFromUrl(string gcsPublicReservationUrl, string contentType, byte[] file)
        {
            var apiUri = new Uri(gcsPublicReservationUrl);
            var httpClient = Http.GetStandardHttpClient();
            var initialReq = new HttpRequestMessage(HttpMethod.Put, apiUri)
            {
                Content = new StringContent("{}"),
            };
            if (initialReq.Content != null)
            {
                //initialReq.Headers.Add("Content-Type", "application/octet-stream");
                initialReq.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            }

            initialReq.Headers.Add("x-goog-resumable", "start");

            var initialResp = await httpClient.SendAsync(initialReq);
            if (!initialResp.IsSuccessStatusCode)
            {
                throw new Exception($"Error initializing upload to cloud storage. HTTP {(int)initialResp.StatusCode}: {await initialResp.Content.ReadAsStringAsync()}");
            }

            var sessionUriString = initialResp.Headers.Location.ToString();
            if (string.IsNullOrEmpty(sessionUriString))
            {
                throw new Exception("Unable to get upload URL.");
            }
            var sessionUri = new Uri(sessionUriString);

            int offset = 0;
            var fileSize = file.Length;

            // Read each chunk and upload synchronously
            while (offset < fileSize || fileSize == 0)
            {
                // Get chunk
                var currentChunkSize = Math.Min(offset + NucleusOneOptions.UploadChunkSize, fileSize);
                var binaryChunk = new byte[currentChunkSize - offset];
                Array.Copy(file, offset, binaryChunk, 0, currentChunkSize - offset);
                var byteEnd = (fileSize == 0)
                    ? 0
                    : Math.Min(offset + NucleusOneOptions.UploadChunkSize - 1, fileSize - 1);

                var chunkReqHttpClient = Http.GetStandardHttpClient();
                var chunkReq = new HttpRequestMessage(HttpMethod.Put, sessionUri)
                {
                    Content = new ByteArrayContent(binaryChunk),
                };
                if (chunkReq.Content != null)
                {
                    //chunkReq.Headers.Add("Content-Type", contentType);
                    chunkReq.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                    chunkReq.Content.Headers.ContentRange = new ContentRangeHeaderValue(offset, byteEnd, fileSize);
                }

                var chunkResp = await chunkReqHttpClient.SendAsync(chunkReq);

                // A 308 is desired after uploading a chunk
                if (chunkResp.StatusCode != System.Net.HttpStatusCode.OK && (int)chunkResp.StatusCode != 308)
                {
                    throw new Exception($"Error uploading to cloud storage. HTTP {(int)chunkResp.StatusCode}: {await chunkResp.Content.ReadAsStringAsync()}");
                }

                // Stop here if this is a 0-byte binary
                if (fileSize == 0)
                {
                    break;
                }
                // Increment offset
                offset += binaryChunk.Length;
            }
        }

        /// <summary>
        /// Uploads a new document into this project.
        /// </summary>
        /// <param name="userEmail">The email address of the user by whom the document will be uploaded.</param>
        /// <param name="fileName">The file name to use when uploading the file.</param>
        /// <param name="contentType">The MIME type of the file.</param>
        /// <param name="file">The file to upload.</param>
        /// <param name="documentFolderId">The ID of the folder to place the document in.</param>
        /// <param name="fieldIDsAndValues">Document field IDs and values.</param>
        /// <returns>A task representing the asynchronous upload operation.</returns>
        public async Task UploadDocument(string userEmail, string fileName, string contentType, byte[] file,
            string documentFolderId = null, Dictionary<string, List<string>> fieldIDsAndValues = null)
        {
            var docUploadReservation = await GetDocumentUploadReservation();
            var fileSize = file.Length;
            await UploadFileToGcsFromUrl(docUploadReservation.SignedUrl, contentType, file);

            docUploadReservation.OriginalFilename = fileName;
            docUploadReservation.OriginalFileSize = fileSize;
            docUploadReservation.DocumentFolderID = documentFolderId;
            docUploadReservation.ContentType = contentType;
            docUploadReservation.FieldIDsAndValues = fieldIDsAndValues;

            var qp = StandardQueryParams.Get();
            qp["uniqueId"] = docUploadReservation.UniqueId;
            qp["captureOriginal"] = false;

            await Http.ExecutePutRequestWithTextResponse(
                apiRelativeUrlPath: ApiPaths.OrganizationsProjectsDocumentUploadsFormat.ReplaceOrgIdAndProjectIdPlaceholdersUsingProject(this),
                app: App,
                queryParams: qp,
                body: Common.Util.SerializeObject(new List<ApiModel.DocumentUpload> { docUploadReservation.ToApiModel() })
            );
        }

        /// <summary>
        /// Gets a NucleusOneAppDocumentFolder instance, which can be used to perform project operations for this organization.
        /// </summary>
        public NucleusOneAppDocumentFolder DocumentFolder(string documentFolderId)
        {
            return new NucleusOneAppDocumentFolder(this, documentFolderId);
        }

        /// <summary>
        /// Gets this project's field, by page.
        /// </summary>
        /// <param name="cursor">The ID of the cursor, from a previous query. Used for paging results.</param>
        public async Task<QueryResult<FieldCollection, Field, ApiModel.FieldCollection, ApiModel.Field>> GetFields(
            string cursor = null)
        {
            var qp = StandardQueryParams.Get(
                callbacks: new Action<StandardQueryParams>[] {
                    (sqp) => sqp.Cursor(cursor)
                }
            );

            var responseBody = await Http.ExecuteGetRequestWithTextResponse(
                apiRelativeUrlPath: ApiPaths.OrganizationsProjectsFieldsFormat.ReplaceOrgIdAndProjectIdPlaceholdersUsingProject(this),
                app: App,
                queryParams: qp
            );

            var apiModel = ApiModel.QueryResult<ApiModel.FieldCollection>.FromJson(responseBody);

            return Common.Util.DefineN1AppInScope(App, () =>
            {
                return QueryResult<FieldCollection, Field, ApiModel.FieldCollection, ApiModel.Field>
                    .FromApiModel(apiModel);
            });
        }

        /// <summary>
        /// Creates fields in this project.
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public async Task<Model.FieldCollection> CreateFields(Model.FieldCollection fields)
        {
            string body = Common.Util.SerializeObject(fields);
            
            string responseBody = await Http.ExecutePostRequestWithTextResponse(
                    apiRelativeUrlPath: ApiPaths.OrganizationsProjectsFieldsFormat.ReplaceOrgIdPlaceholder(Id),
                    app: App,
                    body: body
                )
                .ConfigureAwait(true);

            var apiModel = ApiModel.FieldCollection.FromJsonArray(
                arrayItemsJson: responseBody,
                instance: new ApiModel.FieldCollection(),
                entityFromJsonCallback: (x) => ApiModel.Field.FromJson(x)
            );

            return Common.Util.DefineN1AppInScope(App, () =>
            {
                var createdFields = Model.FieldCollection.FromApiModel(apiModel);
                if ((createdFields == null) || (createdFields.Items.Length == 0))
                    return null;
                return createdFields;
            });
        }
    }

    public enum ProjectAccessType
    {
        Restrictive,
        Permissive
    }

    public enum FieldType
    {
        FieldType_Text,
        FieldType_Number,
        FieldType_Currency,
        FieldType_Date,
        FieldType_Bool
    }
}