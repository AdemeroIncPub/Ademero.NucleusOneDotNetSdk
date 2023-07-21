using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Ademero.NucleusOneDotNetSdk
{
    internal static class Http
    {
        /// <summary>
        /// Gets a standard <see cref="HttpClient"/> instance.
        /// </summary>
        public static HttpClient GetStandardHttpClient()
        {
            // https://stackoverflow.com/a/27327208
#pragma warning disable CA2000 // Dispose objects before losing scope
            var handler = new HttpClientHandler()
            {
                CheckCertificateRevocationList = true,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
#pragma warning restore CA2000 // Dispose objects before losing scope
            return new HttpClient(handler);
        }

        /// <summary>
        /// Sets the standard HTTP headers applicable to all HTTP requests to the Nucleus One API.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to configure.</param>
        private static void SetRequestHeadersCommon(HttpRequestMessage request)
        {
            var headers = request.Headers;

            // headers.clear();
            // headers.Add("Host", "localhost:8080");
            // headers.Add("Connection", "keep-alive");
            // headers.Add("Content-Length", "1317");
            headers.Add("Pragma", "no-cache");
            headers.Add("Cache-Control", "no-cache");
            headers.Add("Accept", "application/json, text/plain, */*");
            // headers.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.141 Safari/537.36");

            if (request.Content != null)
            {
                //headers.Add("Content-Type", "application/json;charset=UTF-8");
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }
            // headers.Add("Origin", "http://localhost:3000");
            // headers.Add("Sec-Fetch-Site", "same-site");
            // headers.Add("Sec-Fetch-Mode", "cors");
            // headers.Add("Sec-Fetch-Dest", "empty");
            // headers.Add("Referer", "http://localhost:3000/login");
            headers.Add("Accept-Encoding", "gzip, deflate, br");
            // headers.Add("Accept-Language", "en-US,en;q=0.9");
            // headers.Add("Cookie", "G_AUTHUSER_H=0");
        }

        /// <summary>
        /// Sets the standard HTTP headers applicable to all HTTP requests to the Nucleus One API.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to configure.</param>
        /// <param name="app">The application to use when connecting to Nucleus One.</param>
        private static void SetAuthenticatedRequestHeaders(
            HttpRequestMessage request,
            NucleusOneApp app
        )
        {
            SetRequestHeadersCommon(request);

            request.Headers.Add("Authorization", $"Bearer {app.Options.ApiKey ?? ""}");
        }

        /// <summary>
        /// Converts a map to a query string.
        /// </summary>
        /// <param name="queryParams">The parameters to convert.</param>
        private static string GetQueryParamsString(Dictionary<string, dynamic> queryParams)
        {
            var sb = new StringBuilder();

            foreach (var pair in queryParams)
            {
                var qpValue = pair.Value;

                if (!(qpValue is string))
                {
                    if ((qpValue is bool) || (qpValue is int) || (qpValue is double))
                    {
                        qpValue = qpValue.ToString();
                    }
                    else
                    {
                        throw new NotSupportedException("Unsupported value type provided in query parameters.");
                    }
                }

                if (sb.Length > 0)
                    sb.Append('&');

                sb.Append(Uri.EscapeDataString(pair.Key));
                sb.Append('=');
                sb.Append(Uri.EscapeDataString(qpValue));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Contains core logic for executing HTTP requests.
        /// </summary>
        /// <param name="apiRelativeUrlPath">The relative Nucleus One API path to use when call the API.</param>
        /// <param name="queryParams">The query string parameters to include in the URL.</param>
        /// <param name="method">The HTTP method to use.</param>
        /// <param name="body">The request body.</param>
        /// <param name="authenticated">Whether this request should include authentication information.</param>
        /// <param name="app">The application to use when connecting to Nucleus One.</param>
        /// <returns></returns>
        private static async Task<HttpResponseMessage> ExecuteStandardHttpRequest(
            string apiRelativeUrlPath,
            HttpMethod method,
            Dictionary<string, dynamic> queryParams = null,
            string body = null,
            bool authenticated = true,
            NucleusOneApp app = null
        )
        {
            app = app ?? GetIt.Get<NucleusOneApp>();

            var qpAsString = (queryParams?.Count > 0) ? $"?{GetQueryParamsString(queryParams)}" : "";
            var fullUrl = app.GetFullUrl(apiRelativeUrlPath) + qpAsString;
            HttpResponseMessage resp;

            using (var clientReq = new HttpRequestMessage(method, fullUrl))
            {
                HttpClient httpClient = null;

                try
                {
                    httpClient = GetStandardHttpClient();

                    if (!string.IsNullOrEmpty(body))
                    {
                        clientReq.Content = new StringContent(body, Encoding.UTF8/*, "application/json"*/);
                    }

                    if (authenticated)
                    {
                        SetAuthenticatedRequestHeaders(clientReq, app);
                    }
                    else
                    {
                        SetRequestHeadersCommon(clientReq);
                    }

                    resp = null;
                    var reqStartTime = DateTime.Now;
                    try
                    {
                        resp = await httpClient.SendAsync(clientReq)
                            .ConfigureAwait(true);
                    }
                    catch (TaskCanceledException ex)
                    {
                        var reqDuration = DateTime.Now.Subtract(reqStartTime);
                        System.Diagnostics.Debugger.Break();

                        if (reqDuration >= httpClient.Timeout)
                        {
                            var exMsg = $"The {clientReq.Method} request to the following URL timed out after {httpClient.Timeout.TotalSeconds} seconds.\n{clientReq.RequestUri}";
                            throw new TimeoutException(exMsg, ex);
                        }
                        throw;
                    }
                }
                finally
                {
                    httpClient?.Dispose();
                }
            }

            if (resp?.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var respBody = await resp.Content.ReadAsStringAsync()
                    .ConfigureAwait(true);
                throw NucleusOneHttpException.FromJsonSafe(resp.StatusCode, respBody);
            }

            return resp;
        }

        /// <summary>
        /// Execute an HTTP GET request, returning the response body.
        /// </summary>
        /// <inheritdoc cref="ExecuteStandardHttpRequest" select="param" />
        public static async Task<string> ExecuteGetRequestWithTextResponse(
            string apiRelativeUrlPath,
            Dictionary<string, dynamic> queryParams = null,
            string body = null,
            bool authenticated = true,
            NucleusOneApp app = null
        )
        {
            var clientResponse = await ExecuteGetRequestInternal(
                    authenticated: authenticated,
                    app: app,
                    apiRelativeUrlPath: apiRelativeUrlPath,
                    queryParams: queryParams,
                    body: body
                )
                .ConfigureAwait(true);
            var respBody = await clientResponse.Content.ReadAsStringAsync()
                .ConfigureAwait(true);
            return respBody;
        }

        /// <summary>
        /// Execute an HTTP GET request, returning the response object.
        /// </summary>
        /// <inheritdoc cref="ExecuteStandardHttpRequest" select="param" />
        public static async Task<HttpResponseMessage> ExecuteGetRequest(
            string apiRelativeUrlPath,
            Dictionary<string, dynamic> queryParams = null,
            string body = null,
            bool authenticated = true,
            NucleusOneApp app = null
        )
        {
            return await ExecuteGetRequestInternal(
                    authenticated: authenticated,
                    app: app,
                    apiRelativeUrlPath: apiRelativeUrlPath,
                    queryParams: queryParams,
                    body: body
                )
                .ConfigureAwait(true);
        }

        /// <summary>
        /// Contains the core logic for executing an HTTP GET request, returning the response object.
        /// </summary>
        /// <inheritdoc cref="ExecuteStandardHttpRequest" select="param" />
        private static async Task<HttpResponseMessage> ExecuteGetRequestInternal(
            string apiRelativeUrlPath,
            Dictionary<string, dynamic> queryParams = null,
            string body = null,
            bool authenticated = true,
            NucleusOneApp app = null
        )
        {
            return await ExecuteStandardHttpRequest(
                authenticated: authenticated,
                app: app,
                apiRelativeUrlPath: apiRelativeUrlPath,
                queryParams: queryParams,
                body: body,
                method: HttpMethod.Get
            )
                .ConfigureAwait(true); ;
        }

        /// <summary>
        /// Execute an HTTP POST request, returning the response body.
        /// </summary>
        /// <inheritdoc cref="ExecuteStandardHttpRequest" select="param" />
        public static async Task<string> ExecutePostRequestWithTextResponse(
            string apiRelativeUrlPath,
            Dictionary<string, dynamic> queryParams = null,
            string body = null,
            bool authenticated = true,
            NucleusOneApp app = null
        )
        {
            var clientResponse = await ExecuteStandardHttpRequest(apiRelativeUrlPath, HttpMethod.Post, queryParams, body, authenticated, app)
                .ConfigureAwait(true);
            return await clientResponse.Content.ReadAsStringAsync()
                .ConfigureAwait(true);
        }

        /// <summary>
        /// Execute an HTTP POST request.
        /// </summary>
        /// <param name="apiRelativeUrlPath">The relative Nucleus One API path to use when call the API.</param>
        /// <param name="queryParams">Query string parameters.</param>
        /// <param name="body">The request body.</param>
        /// <param name="authenticated">Indicates whether the request requires authentication.</param>
        /// <param name="app">The application to use when connecting to Nucleus One.</param>
        /// <returns>An instance of <see cref="HttpResponseMessage"/>.</returns>
        public static async Task<HttpResponseMessage> ExecutePostRequest(
            string apiRelativeUrlPath,
            Dictionary<string, dynamic> queryParams = null,
            string body = null,
            bool authenticated = true,
            NucleusOneApp app = null
        )
        {
            return await ExecuteStandardHttpRequest(apiRelativeUrlPath, HttpMethod.Post, queryParams, body, authenticated, app)
                .ConfigureAwait(true);
        }

        /// <summary>
        /// Execute an HTTP DELETE request.
        /// </summary>
        /// <inheritdoc cref="ExecutePostRequest" />
        public static async Task<HttpResponseMessage> ExecuteDeleteRequest(
            string apiRelativeUrlPath,
            Dictionary<string, dynamic> queryParams = null,
            string body = null,
            bool authenticated = true,
            NucleusOneApp app = null
        )
        {
            return await ExecuteStandardHttpRequest(apiRelativeUrlPath, HttpMethod.Delete, queryParams, body, authenticated, app)
                .ConfigureAwait(true);
        }

        /// <summary>
        /// Execute an HTTP PUT request, returning the response body.
        /// </summary>
        /// <inheritdoc cref="ExecuteStandardHttpRequest" select="param" />
        public static async Task<string> ExecutePutRequestWithTextResponse(
            string apiRelativeUrlPath,
            Dictionary<string, dynamic> queryParams = null,
            string body = null,
            bool authenticated = true,
            NucleusOneApp app = null
        )
        {
            var clientResponse = await ExecuteStandardHttpRequest(apiRelativeUrlPath, HttpMethod.Put, queryParams, body, authenticated, app);
            return await clientResponse.Content.ReadAsStringAsync()
                .ConfigureAwait(true);
        }

        /// <summary>
        /// Execute an HTTP PUT request, returning the response body.
        /// </summary>
        /// <inheritdoc cref="ExecuteStandardHttpRequest" select="param" />
        public static async Task<HttpResponseMessage> ExecutePutRequest(
            string apiRelativeUrlPath,
            Dictionary<string, dynamic> queryParams = null,
            string body = null,
            bool authenticated = true,
            NucleusOneApp app = null
        )
        {
            return await ExecuteStandardHttpRequest(apiRelativeUrlPath, HttpMethod.Put, queryParams, body, authenticated, app)
                .ConfigureAwait(true);
        }
    }

    /// <summary>
    /// Contains relative URL paths for calling the Nucleus One API.
    /// </summary>
    internal abstract class ApiPaths
    {
        // /organizations/<organizationId>/members?homePath=%2Forganizations%2FrCUlmAf0TYoVeZyFG3Cp%2Flink%2Fhome

        //public const string documentSignatureSessionsSigningRecipientsFieldsFormat =
        //    "/documentSignatureSessions/<documentSignatureSessionId>/signingRecipients/<documentSignatureSessionRecipientId>/fields";
        //public const string formTemplatesPublicFormat = "/formTemplatesPublic/<formTemplateId>";
        //public const string formTemplatesPublicFieldsFormat = "/formTemplatesPublic/<formTemplateId>/fields";
        //public const string formTemplatesPublicFieldListItemsFormat =
        //    "/formTemplatesPublic/<formTemplateId>/fields/<formTemplateFieldId>/listItems";
        //public const string formTemplatesPublicSubmissions = "/formTemplatesPublic/<formTemplateId>/submissions";
        //public const string logs = "/logs";
        public const string OrganizationMembers = "/organizations/<organizationId>/members";
        //public const string organizationMembershipPackages = "/organizationMembershipPackages";
        //public const string organizationMembershipPackagesFormat =
        //    "/organizationMembershipPackages/<organizationId>";
        public const string Organizations = "/organizations";
        public const string OrganizationSearchResults = "/organizations/<organizationId>/searchResults";
        public const string OrganizationsOrganizationFormat = "/organizations/<organizationId>";
        //public const string organizationsOrganizationDocumentSubscriptionsFormat =
        //    "/organizations/<organizationId>/documentSubscriptions";
        //public const string organizationsPermissionsFormat = "/organizations/<organizationId>/permissions";
        //public const string organizationsProjectsApprovalActionsApproveFormat =
        //    "/organizations/<organizationId>/projects/<projectId>/approvalActions/approve";
        //public const string organizationsProjectsApprovalActionsDeclineFormat =
        //    "/organizations/<organizationId>/projects/<projectId>/approvalActions/decline";
        //public const string organizationsProjectsApprovalActionsDenyFormat =
        //    "/organizations/<organizationId>/projects/<projectId>/approvalActions/deny";
        //public const string organizationsProjectsApprovalsFormat =
        //    "/organizations/<organizationId>/projects/<projectId>/approvals";
        //public const string organizationsProjectsCountsDocumentsFormat =
        //    "/organizations/<organizationId>/projects/<projectId>/counts/documents";
        //public const string organizationsProjectsCountsPagesFormat =
        //    "/organizations/<organizationId>/projects/<projectId>/counts/pages";
        //public const string organizationsProjectsCountsRecycleBinDocumentsFormat =
        //    "/organizations/<organizationId>/projects/<projectId>/counts/recycleBinDocuments";
        //public const string organizationsProjectsDocumentActionsRestoreFromRecycleBinFormat =
        //    "/organizations/<organizationId>/projects/<projectId>/documentActions/restoreFromRecycleBin";
        //public const string organizationsProjectsDocumentActionsSendToRecycleBinFormat =
        //    "/organizations/<organizationId>/projects/<projectId>/documentActions/sendToRecycleBin";
        //public const string organizationsProjectsDocumentContentPackagesFormat =
        //    "/organizations/<organizationId>/projects/<projectId>/documentContentPackages/<documentId>";
        public const string OrganizationsProjectsDocumentFoldersFormat =
            "/organizations/<organizationId>/projects/<projectId>/documentFolders";
        //public const string organizationsProjectsDocumentFoldersDocumentFolderFormat =
        //    "/organizations/<organizationId>/projects/<projectId>/documentFolders/<documentFolderId>";
        //public const string organizationsProjectsDocumentPackagesFormat =
        //    "/organizations/<organizationId>/projects/<projectId>/documentPackages/<documentId>";
        //public const string organizationsProjectsDocumentSubscriptionsFormat =
        //    "/organizations/<organizationId>/projects/<projectId>/documentSubscriptions/<documentId>";
        public const string OrganizationsProjectsDocumentUploadsFormat =
            "/organizations/<organizationId>/projects/<projectId>/documentUploads";
        //public const string organizationsProjectsDocumentsFormat =
        //    "/organizations/<organizationId>/projects/<projectId>/documents";
        //public const string organizationsProjectsDocumentsDocumentFormat =
        //    "/organizations/<organizationId>/projects/<projectId>/documents/<documentId>";
        //public const string organizationsProjectsDocumentsDocumentCommentsFormat =
        //    "/organizations/<organizationId>/projects/<projectId>/documents/<documentId>/comments";
        //public const string organizationProjectsDocumentsDocumentEventsFormat =
        //    "/organizations/<organizationId>/projects/<projectId>/documents/<documentId>/events";
        //public const string organizationsProjectsDocumentsDocumentSignatureFormsFormat =
        //    "/organizations/<organizationId>/projects/<projectId>/documents/<documentId>/signatureForms";
        //public const string organizationsProjectsDocumentsSignatureFormsDocumentSignatureFormFormat =
        //    "/organizations/<organizationId>/projects/<projectId>/documents/<documentId>/signatureForms/<documentSignatureFormId>";
        //public const string organizationsProjectsDocumentsSignatureFormsDocumentSignatureFormFieldsFormat =
        //    "/organizations/<organizationId>/projects/<projectId>/documents/<documentId>/signatureForms/<documentSignatureFormId>/fields";
        //public const string organizationsProjectsDocumentsSignatureFormsDocumentSignatureFormFieldsdocumentSignatureFormFieldFormat =
        //    "/organizations/<organizationId>/projects/<projectId>/documents/<documentId>/signatureForms/<documentSignatureFormId>/fields/<documentSignatureFormFieldId>";
        //public const string organizationsProjectsDocumentsSignatureSessionPackagesFormat =
        //    "/organizations/<organizationId>/projects/<projectId>/documents/<documentId>/signatureSessionPackages";
        //public const string organizationsProjectsDocumentsThumbnailsFormat =
        //    "/organizations/<organizationId>/projects/<projectId>/documents/<documentId>/thumbnails";
        //public const string organizationsProjectsDocumentsRecentDocumentSignatureFormsFormat =
        //    "/organizations/<organizationId>/projects/<projectId>/recentDocumentSignatureForms";
        public const string OrganizationsProjectsFieldsFormat =
            "/organizations/<organizationId>/projects/<projectId>/fields";
        public const string OrganizationsProjectsFieldsFieldFormat =
            "/organizations/<organizationId>/projects/<projectId>/fields/<fieldId>";
        public const string OrganizationsProjectsFieldsFieldListItemsFormat =
            "/organizations/<organizationId>/projects/<projectId>/fields/<fieldId>/listItems";
        public const string OrganizationsProjectsFormat = "/organizations/<organizationId>/projects";
        public const string OrganizationsProjectsProjectFormat =
            "/organizations/<organizationId>/projects/<projectId>";
        //public const string organizationsProjectsFormTemplatesFormat =
        //    "/organizations/<organizationId>/projects/<projectId>/formTemplates";
        public const string OrganizationsProjectsMembersFormat =
            "/organizations/<organizationId>/projects/<projectId>/members";
        //public const string organizationsProjectsSignatureFormTemplatesFormat =
        //    "/organizations/<organizationId>/projects/<projectId>/signatureFormTemplates";
        //public const string organizationsProjectsSignatureFormTemplatesSignatureFormTemplateFormat =
        //    "/organizations/<organizationId>/projects/<projectId>/signatureFormTemplates/<signatureFormTemplateId>";
        //public const string organizationsProjectsSignatureFormTemplatesSignatureFormTemplateFieldsFormat =
        //    "/organizations/<organizationId>/projects/<projectId>/signatureFormTemplates/<signatureFormTemplateId>/fields";
        //public const string organizationsProjectsTasksFormat =
        //    "/organizations/<organizationId>/projects/<projectId>/tasks";
        //public const string organizationsProjectsTasksTaskFormat =
        //    "/organizations/<organizationId>/projects/<projectId>/tasks/<taskId>";
        //public const string organizationsProjectsTasksTaskCommentsFormat =
        //    "/organizations/<organizationId>/projects/<projectId>/tasks/<taskId>/comments";
        //public const string organizationsSubscriptionsFormat = "/organizations/<organizationId>/subscriptions";
        //public const string organizationsSubscriptionsInvoicesFormat =
        //    "/organizations/<organizationId>/subscriptions/invoices";
        //public const string organizationsSubscriptionsPlansFormat =
        //    "/organizations/<organizationId>/subscriptions/plans";
        //public const string supportErrorEvents = "/support/errorEvents";
        //public const string supportOrganizations = "/support/organizations";
        //public const string supportUsers = "/support/users";
        //public const string supportAdmin = "/supportAdmin";
        //public const string userAddressBookItems = "/user/addressBookItems";
        //public const string userEmailAddresses = "/user/emailAddresses";
        //public const string userEmailAddressesEmailChangeCodeFormat = "/user/emailAddresses/<emailChangeCode>";
        //public const string userEmailAddressVerifications = "/user/emailAddressVerifications";
        //public const string userEmailLoginOTPSend = "/user/emailLoginOTPSend";
        //public const string userEmailLoginVerify = "/user/emailLoginVerify";
        //public const string userLogin = "/user/login";
        //public const string userLogout = "/user/logout";
        //public const string userOrganizations = "/user/organizations";
        //public const string userOrganizationsProjectsFormat = "/user/organizations/<organizationId>/projects";
        //public const string userPreferences = "/user/preferences";
        //public const string userPreferenceFormat = "/user/preferences/<singleUserPreferenceId>";
        //public const string userProfile = "/user/profile";
        //public const string userSmsNumbers = "/user/smsNumbers";
        //public const string userSmsNumbersSmsChangeCodeFormat = "/user/smsNumbers/<smsChangeCode>";
    }

    /// <summary>
    /// Provides support for adding common query string parameters.  Values are only included if they
    /// are not null.
    /// </summary>
    internal sealed class StandardQueryParams
    {
        private readonly Dictionary<string, dynamic> _map = new Dictionary<string, dynamic>();

        /// <summary>
        /// Builds a map containing query string parameters.
        /// </summary>
        /// <param name="callbacks">A list of standard query parameters to include in the map.</param>
        public static Dictionary<string, dynamic> Get(
            Action<StandardQueryParams>[] callbacks = null
        )
        {
            var sqp = new StandardQueryParams();
            if (callbacks != null)
            {
                foreach (var cb in callbacks)
                {
                    cb(sqp);
                }
            }
            return sqp._map;
        }

        /// <summary>
        /// Contains core logic for setting a map parameter's value.
        /// </summary>
        /// <param name="name">The parameter"s name.</param>
        /// <param name="value">The parameter"s value.</param>
        private void SetMapParamInternal<T>(string name, T value)
        {
            if (value != null)
            {
                _map[name] = value;
            }
        }

        /// <summary>
        /// If not null, includes the "sortDescending" parameter in the map.
        /// </summary>
        /// <param name="sortDescending">Whether to sort in descending order.</param>
        public void SortDescending(bool? sortDescending) => SetMapParamInternal("sortDescending", sortDescending);

        /// <summary>
        /// If not null, includes the "sortType" parameter in the map.
        /// </summary>
        /// <param name="sortType">The type of sorting to apply.</param>
        public void SortType(string sortType) => SetMapParamInternal("sortType", sortType);

        /// <summary>
        /// If not null, includes the "offset" parameter in the map.
        /// </summary>
        /// <param name="offset">The offset at which to start the returned results.  Used for paging results.</param>
        public void Offset(int? offset) => SetMapParamInternal("offset", offset);

        /// <summary>
        /// If not null, includes the "sortDescending" parameter in the map.
        /// </summary>
        /// <param name="cursor">The ID of the cursor, from a previous query.  Used for paging results.</param>
        public void Cursor(string cursor) => SetMapParamInternal("cursor", cursor);
    }

    /// <summary>
    /// The exception thrown when an HTTP-related error occurs while calling the Nucleus One API.
    /// </summary>
    [Serializable]
#pragma warning disable CA1032 // Implement standard exception constructors
    public class NucleusOneHttpException : Exception
#pragma warning restore CA1032 // Implement standard exception constructors
    {
        #region Fields

        private readonly System.Net.HttpStatusCode _status;

        #endregion

        private NucleusOneHttpException() { }

        /// <summary>
        /// Creates an instance of the <see cref="NucleusOneHttpException"/> class.
        /// </summary>
        /// <param name="status">The HTTP status code.</param>
        /// <param name="message">The error message.</param>
        public NucleusOneHttpException(System.Net.HttpStatusCode status, string message) : base(message)
        {
            _status = status;
        }

        /// <summary>
        /// Creates an instance of the <see cref="NucleusOneHttpException"/> class.
        /// </summary>
        /// <param name="status">The HTTP status code.</param>
        /// <param name="message">The error message.</param>
        /// <param name="inner">
        /// The exception that is the cause of the current exception, or a null reference
        /// (Nothing in Visual Basic) if no inner exception is specified.
        /// </param>
        public NucleusOneHttpException(System.Net.HttpStatusCode status, string message, Exception inner) : base(message, inner)
        {
            _status = status;
        }

        protected NucleusOneHttpException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public static NucleusOneHttpException FromJsonSafe(System.Net.HttpStatusCode status, string json)
        {
            string message;
#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                message = (string)((JObject)Common.Util.JsonDeserializeObject(json))["message"];
            }
            catch
            {
                // The above logic assumes a standard error response from the API, if that wasn't received,
                // then just set the "json" as the message
                message = json;
            }
#pragma warning restore CA1031 // Do not catch general exception types
            return new NucleusOneHttpException(status: status, message: message);
        }

        #region Properties

        /// <summary>
        /// The HTTP status code.
        /// </summary>
        public System.Net.HttpStatusCode Status { get => _status; }

        #endregion
    }
}