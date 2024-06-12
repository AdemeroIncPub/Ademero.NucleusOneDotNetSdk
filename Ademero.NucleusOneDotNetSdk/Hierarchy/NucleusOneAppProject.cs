using Ademero.NucleusOneDotNetSdk.Common;
using Ademero.NucleusOneDotNetSdk.Common.Strings;
using Ademero.NucleusOneDotNetSdk.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        public NucleusOneAppOrganization Organization
        {
            [DebuggerStepThrough]
            get;
        }

        /// <summary>
        /// The project's ID.
        /// </summary>
        public string Id
        {
            [DebuggerStepThrough]
            get;
        }

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
        /// Gets a NucleusOneAppField instance, which can be used to perform field operations for this project.
        /// </summary>
        public NucleusOneAppField Field(string fieldId)
        {
            return new NucleusOneAppField(this, fieldId);
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
        private static async Task UploadFileToGcsFromUrl(string gcsPublicReservationUrl, string contentType, byte[] file)
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
        /// Creates a folder.
        /// </summary>
        /// <param name="name">The folder's name.</param>
        /// <param name="parentId">The parent folder's ID.</param>
        /// <returns>The created folder, if successful; otherwise, null.</returns>
        public async Task<Model.DocumentFolder> CreateDocumentFolder(string name, string parentId = null)
        {
            var qp = StandardQueryParams.Get();
            qp["documentFolderPathPrefix"] = PathHelper.GetOrganizationLink(Organization.Id,
                PathHelper.GetWorkspaceDocumentFoldersPath(Id));

            var docFolder = new
            {
                ParentID = parentId,
                Name = name,
                AssignmentUserEmails = Array.Empty<string>(),
                HexColor = (string)null
            };

            var responseBody = await Http.ExecutePostRequestWithTextResponse(
                    apiRelativeUrlPath: ApiPaths.OrganizationsProjectsDocumentFoldersFormat
                        .ReplaceOrgIdAndProjectIdPlaceholdersUsingProject(this),
                    app: App,
                    body: Util.JsonSerializeObject(new[] { docFolder }, null),
                    queryParams: qp
                )
                .ConfigureAwait(true);

            var apiModel = ApiModel.DocumentFolderCollection.FromJsonArray(
                arrayItemsJson: responseBody,
                instance: new ApiModel.DocumentFolderCollection(),
                entityFromJsonCallback: (x) => ApiModel.DocumentFolder.FromJson(x)
            );

            return Util.DefineN1AppInScope(App, () =>
            {
                var createdFolders = Model.DocumentFolderCollection.FromApiModel(apiModel);
                if ((createdFolders == null) || (createdFolders.Items.Length == 0))
                    return null;
                return createdFolders.Items[0];
            });
        }

        /// <summary>
        /// Gets document folders for the current project. Only one folder hierarchy level is returned.
        /// </summary>
        /// <param name="parentId">The ID of the parent folder in the hierarchy.</param>
        /// <returns></returns>
        public async Task<DocumentFolderCollection> GetAllDocumentFolders(string parentId = null)
        {
            Func<string, Task<dynamic>> getNextPageHandler = async (string cursor) =>
                await GetDocumentFoldersPaged(parentId, cursor);
            DocumentFolder[] allResults = await GetAllEntitiesByPages<DocumentFolder, DocumentFolderCollection>(
                getNextPageHandler
            );
            return new DocumentFolderCollection(allResults, App);
        }

        /// <summary>
        /// Gets document folders for the current project. Only one folder hierarchy level is returned.
        /// </summary>
        /// <param name="parentId">The ID of the parent folder in the hierarchy.</param>
        /// <param name="cursor">The ID of the cursor, from a previous query. Used for paging results.</param>
        /// <returns></returns>
        public async Task<QueryResult<DocumentFolderCollection, DocumentFolder, ApiModel.DocumentFolderCollection, ApiModel.DocumentFolder>>
            GetDocumentFoldersPaged(string parentId = null, string cursor = null)
        {
            string apiRelativeUrlPath = ApiPaths.OrganizationsProjectsDocumentFoldersFormat
                    .ReplaceOrgIdAndProjectIdPlaceholdersUsingProject(this);
            var qpCallback = string.IsNullOrEmpty(parentId)
                ? (Action<Dictionary<string, dynamic>>)null
                : (qp) => qp["parentId"] = parentId;
            return await GetItemsPaged<DocumentFolderCollection, DocumentFolder, ApiModel.DocumentFolderCollection, ApiModel.DocumentFolder>(
                apiRelativeUrlPath, qpCallback, cursor
            );
        }

        /// <summary>
        /// Deletes a document folder in this project.
        /// </summary>
        /// <param name="documentFolderId">The ID of the document folder to delete.</param>
        public async Task DeleteDocumentFolder(string documentFolderId)
        {
            string body = Util.JsonSerializeObject(
                new
                {
                    IDs = new string[] { documentFolderId }
                });

            await Http.ExecuteDeleteRequest(
                    apiRelativeUrlPath: ApiPaths.OrganizationsProjectsDocumentFoldersFormat
                        .ReplaceOrgIdAndProjectIdPlaceholdersUsingProject(this),
                    app: App,
                    body: body
                )
                .ConfigureAwait(true);
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
        /// <param name="tags">Document tags.</param>
        /// <param name="skipOcr">Whether or not the file should be skipped.</param>
        /// <returns>A task representing the asynchronous upload operation.</returns>
        public async Task UploadDocument(string userEmail, string fileName, string contentType, byte[] file,
            string documentFolderId = null, Dictionary<string, List<string>> fieldIDsAndValues = null,
            HashSet<string> tags = null, bool skipOcr = false)
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

            if (skipOcr)
                qp["skipOCR"] = true;

            if (tags?.Count > 0)
                docUploadReservation.Tags = Util.MakeHashSetCaseInsensitive(tags);

            await Http.ExecutePutRequest(
                apiRelativeUrlPath: ApiPaths.OrganizationsProjectsDocumentUploadsFormat.ReplaceOrgIdAndProjectIdPlaceholdersUsingProject(this),
                app: App,
                queryParams: qp,
                body: Util.JsonSerializeObject(new[] { docUploadReservation.ToApiModel() })
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
        /// Gets a field in this project.
        /// </summary>
        /// <param name="cursor">The field's ID.</param>
        public async Task<Field> GetField(string fieldId)
        {
            var responseBody = await Http.ExecuteGetRequestWithTextResponse(
                apiRelativeUrlPath: ApiPaths.OrganizationsProjectsFieldsFieldFormat
                    .ReplaceOrgIdAndProjectIdPlaceholdersUsingProject(this)
                    .ReplaceFieldIdPlaceholder(fieldId),
                app: App
            );

            var apiModel = ApiModel.Field.FromJson(responseBody);

            return Util.DefineN1AppInScope(App, () =>
            {
                return Model.Field.FromApiModel(apiModel);
            });
        }

        /// <summary>
        /// Updates a field in this project.
        /// </summary>
        /// <param name="field">The existing field to update.</param>
        /// <returns>The updated field, as it exists in Nucleus One.</returns>
        public async Task<Model.Field> UpdateField(Model.Field field)
        {
            string body = Util.JsonSerializeObject(field);

            string responseBody = await Http.ExecutePutRequestWithTextResponse(
                    apiRelativeUrlPath: ApiPaths.OrganizationsProjectsFieldsFieldFormat
                        .ReplaceOrgIdAndProjectIdPlaceholdersUsingProject(this)
                        .ReplaceFieldIdPlaceholder(field.Id),
                    app: App,
                    body: body
                )
                .ConfigureAwait(true);

            var apiModel = ApiModel.Field.FromJson(responseBody);

            return Util.DefineN1AppInScope(App, () =>
            {
                return Model.Field.FromApiModel(apiModel);
            });
        }

        /// <summary>
        /// Deletes a field in this project.
        /// </summary>
        /// <param name="fieldId">The ID of the field to delete.</param>
        public async Task DeleteField(string fieldId)
        {
            string body = Util.JsonSerializeObject(
                new
                {
                    IDs = new string[] { fieldId }
                });

            await Http.ExecuteDeleteRequest(
                    apiRelativeUrlPath: ApiPaths.OrganizationsProjectsFieldsFormat
                        .ReplaceOrgIdAndProjectIdPlaceholdersUsingProject(this),
                    app: App,
                    body: body
                )
                .ConfigureAwait(true);
        }

        /// <summary>
        /// Gets this project's fields.
        /// </summary>
        /// <returns></returns>
        public async Task<FieldCollection> GetAllFields()
        {
            Func<string, Task<dynamic>> getNextPageHandler = async (string cursor) =>
                await GetFieldsPaged(cursor);
            Field[] allResults = await GetAllEntitiesByPages<Field, FieldCollection>(
                getNextPageHandler
            );
            return new FieldCollection(allResults, App);
        }

        /// <summary>
        /// Gets this project's fields, by page.
        /// </summary>
        /// <param name="cursor">The ID of the cursor, from a previous query. Used for paging results.</param>
        public async Task<QueryResult<FieldCollection, Field, ApiModel.FieldCollection, ApiModel.Field>> GetFieldsPaged(string cursor)
        {
            var apiRelativeUrlPath = ApiPaths.OrganizationsProjectsFieldsFormat
                .ReplaceOrgIdAndProjectIdPlaceholdersUsingProject(this);
            return await GetItemsPaged<FieldCollection, Field, ApiModel.FieldCollection, ApiModel.Field>(
                apiRelativeUrlPath, null, cursor
            );
        }

        /// <summary>
        /// Creates fields in this project.
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public async Task<Model.FieldCollection> CreateFields(Model.FieldCollection fields)
        {
            string body = Util.JsonSerializeObject(fields);
            
            string responseBody = await Http.ExecutePostRequestWithTextResponse(
                    apiRelativeUrlPath: ApiPaths.OrganizationsProjectsFieldsFormat.ReplaceOrgIdAndProjectIdPlaceholdersUsingProject(this),
                    app: App,
                    body: body
                )
                .ConfigureAwait(true);

            var apiModel = ApiModel.FieldCollection.FromJsonArray(
                arrayItemsJson: responseBody,
                instance: new ApiModel.FieldCollection(),
                entityFromJsonCallback: (x) => ApiModel.Field.FromJson(x)
            );

            return Util.DefineN1AppInScope(App, () =>
            {
                var createdFields = Model.FieldCollection.FromApiModel(apiModel);
                if ((createdFields == null) || (createdFields.Items.Length == 0))
                    return null;
                return createdFields;
            });
        }

        /// <summary>
        /// Gets a project member by email address.
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        public async Task<ProjectMember> GetMemberByEmailAddess(string emailAddress)
        {
            // Convert each input search field into the expected JSON object
            var metaFieldFiltersJsonObjects = new dynamic[] {
                // Find members with this exact email address
                new {
                    FieldID = "Meta_text_kw256lc[UserEmail].keyword",
                    FieldType = "FieldType_Text",
                    FieldValue = emailAddress,
                    Operator = "equals"
                }
            };

            var qp = StandardQueryParams.Get();
            qp["metaFieldFilters_json"] = Common.Util.JsonSerializeObject(metaFieldFiltersJsonObjects);

            string responseBody = await Http.ExecuteGetRequestWithTextResponse(
                    apiRelativeUrlPath: ApiPaths.OrganizationsProjectsMembersFormat
                        .ReplaceOrgIdAndProjectIdPlaceholdersUsingProject(this),
                    app: App,
                    queryParams: qp
                )
                .ConfigureAwait(true);

            var apiModel = ApiModel.QueryResult<ApiModel.ProjectMemberCollection>.FromJson(responseBody);
            var projectMembers = apiModel.Results.ProjectMembers;

            if (projectMembers.Length == 0)
                return null;

            return Util.DefineN1AppInScope(App, () =>
            {
                return ProjectMember.FromApiModel(apiModel.Results.ProjectMembers[0]);
            });
        }

        /// <summary>
        /// Gets all tags in this project.
        /// </summary>
        /// <param name="includeAssetItems">Whether to include asset items in the results.</param>
        /// <returns></returns>
        public async Task<Model.TagCollection> GetAllTags(bool? includeAssetItems = null)
        {
            Func<string, Task<dynamic>> getNextPageHandler = async (string cursor) =>
                await GetTagsPaged(cursor, includeAssetItems);
            Tag[] allResults = await GetAllEntitiesByPages<Tag, TagCollection>(
                getNextPageHandler
            );
            return new TagCollection(allResults, App);
        }

        /// <summary>
        /// Gets tags in this project, by page.
        /// </summary>
        /// <inheritdoc cref="GetAllTags" />
        public async Task<QueryResult<TagCollection, Tag, ApiModel.TagCollection, ApiModel.Tag>> GetTagsPaged(
            string cursor = null, bool? includeAssetItems = null)
        {
            var apiRelativeUrlPath = ApiPaths.OrganizationsProjectsTagsFormat.ReplaceOrgIdAndProjectIdPlaceholdersUsingProject(this);
            Action<Dictionary<string, dynamic>> qpCallback = (qp) =>
            {
                Util.SetDictionaryValueIfNotNull(qp, "includeAssetItems", includeAssetItems);
            };
            return await GetItemsPaged<TagCollection, Tag, ApiModel.TagCollection, ApiModel.Tag>(
                apiRelativeUrlPath, qpCallback, cursor
            );
        }

        /// <summary>
        /// Searches for documents in this project.
        /// </summary>
        /// <returns></returns>
        public async Task<SearchResultCollection> SearchAllDocuments(Dictionary<string, string> fieldIdsAndValues)
        {
            Func<string, Task<dynamic>> getNextPageHandler = async (string cursor) =>
                await SearchDocumentsPaged(fieldIdsAndValues, cursor);
            SearchResult[] allResults = await GetAllEntitiesByPages<SearchResult, SearchResultCollection>(
                getNextPageHandler
            );
            return new SearchResultCollection(allResults, App);
        }

        /// <summary>
        /// Searches for documents in this project, by page.
        /// </summary>
        /// <param name="cursor">The ID of the cursor, from a previous query. Used for paging results.</param>
        public async Task<QueryResult<SearchResultCollection, SearchResult, ApiModel.SearchResultCollection, ApiModel.SearchResult>>
            SearchDocumentsPaged(Dictionary<string, string> fieldIdsAndValues, string cursor)
        {
            // Convert each input search field into the expected JSON object
            List<dynamic> fieldIdsAndValuesJsonObjects = fieldIdsAndValues.Select(x =>
                {
                    return new
                    {
                        FieldID = "IxF_Text[" + x.Key + "]",
                        FieldType = "FieldType_Text",
                        FieldValue = x.Value,
                        Operator = "equals"
                    };
                })
                .ToList<dynamic>();

            fieldIdsAndValuesJsonObjects.Add(
                new
                {
                    FieldID = "Meta_kw256[ProjectID]",
                    FieldType = "FieldType_Text",
                    FieldValue = Id,
                    Operator = "equals"
                });

            Action<Dictionary<string, dynamic>> qpCallback = (qp) =>
            {
                // Only search documents
                qp["contentType"] = "Document";
                qp["metaFieldFilters_json"] = Common.Util.JsonSerializeObject(fieldIdsAndValuesJsonObjects);
            };

            var apiRelativeUrlPath = ApiPaths.OrganizationSearchResults
                .ReplaceOrgIdPlaceholder(Organization.Id);
            return await GetItemsPaged<SearchResultCollection, SearchResult, ApiModel.SearchResultCollection, ApiModel.SearchResult>(
                apiRelativeUrlPath, qpCallback, cursor
            );
        }

        /// <summary>
        /// Sends a document in this project to the Recycle Bin.
        /// </summary>
        /// <param name="documentId">The document's ID.</param>
        /// <returns></returns>
        public async Task SendDocumentToRecycleBin(string documentId)
        {
            string body = Util.JsonSerializeObject(
                new
                {
                    IDs = new string[] { documentId }
                });

            string responseBody = await Http.ExecutePostRequestWithTextResponse(
                    apiRelativeUrlPath: ApiPaths.OrganizationsProjectsDocumentActionsSendToRecycleBinFormat
                        .ReplaceOrgIdAndProjectIdPlaceholdersUsingProject(this),
                    app: App,
                    body: body
                )
                .ConfigureAwait(true);

            // This technically returns a document object, but we're not returning it, for now.
            // If needed, this can be changed in the future.
        }

        /// <summary>
        /// Gets members of the current project.
        /// </summary>
        /// <returns></returns>
        public async Task<ProjectMemberCollection> GetAllMembers()
        {
            Func<string, Task<dynamic>> getNextPageHandler = async (string cursor) =>
                await GetMembersPaged(cursor);
            ProjectMember[] allResults = await GetAllEntitiesByPages<ProjectMember, ProjectMemberCollection>(
                getNextPageHandler
            );
            return new ProjectMemberCollection(allResults, App);
        }

        /// <summary>
        /// Gets members of the current project, by page.
        /// </summary>
        /// <param name="cursor">The ID of the cursor, from a previous query. Used for paging results.</param>
        public async Task<QueryResult<ProjectMemberCollection, ProjectMember, ApiModel.ProjectMemberCollection, ApiModel.ProjectMember>> GetMembersPaged(string cursor)
        {
            var apiRelativeUrlPath = ApiPaths.OrganizationsProjectsMembersFormat
                .ReplaceOrgIdAndProjectIdPlaceholdersUsingProject(this);
            return await GetItemsPaged<ProjectMemberCollection, ProjectMember, ApiModel.ProjectMemberCollection, ApiModel.ProjectMember>(
                apiRelativeUrlPath, null, cursor
            );
        }

        /// <summary>
        /// Adds organization members to this project.
        /// </summary>
        /// <param name="members"></param>
        /// <returns></returns>
        public async Task<Model.ProjectMemberCollection> AddMembers(Model.ProjectMemberCollection members)
        {
            var qp = StandardQueryParams.Get();
            qp["homePath"] = Common.PathHelper.GetOrganizationLink(Id, Common.PathHelper.GetHomePath());
            qp["projectPath"] = Common.PathHelper.GetOrganizationLink(Id, Common.PathHelper.GetProjectPath(Id));

            string body = Util.JsonSerializeObject(members);

            string responseBody = await Http.ExecutePostRequestWithTextResponse(
                    apiRelativeUrlPath: ApiPaths.OrganizationsProjectsMembersFormat
                    .ReplaceOrgIdAndProjectIdPlaceholdersUsingProject(this),
                    app: App,
                    queryParams: qp,
                    body: body
                )
                .ConfigureAwait(true);

            var apiModel = ApiModel.ProjectMemberCollection.FromJsonArray(
                arrayItemsJson: responseBody,
                instance: new ApiModel.ProjectMemberCollection(),
                entityFromJsonCallback: (x) => ApiModel.ProjectMember.FromJson(x)
            );

            return Util.DefineN1AppInScope(App, () =>
            {
                var createdProjectMembers = Model.ProjectMemberCollection.FromApiModel(apiModel);
                if ((createdProjectMembers == null) || (createdProjectMembers.Items.Length == 0))
                    return null;
                return createdProjectMembers;
            });
        }
    }

    public enum AssignmentType
    {
        World,
        ProjectMember
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