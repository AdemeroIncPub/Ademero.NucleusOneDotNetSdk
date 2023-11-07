using Ademero.NucleusOneDotNetSdk.Common.Strings;
using Ademero.NucleusOneDotNetSdk.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

[assembly: CLSCompliant(true)]

namespace Ademero.NucleusOneDotNetSdk
{
    public static class NucleusOne
    {
        static bool _sdkInitialized;

        /// <summary>
        /// Initializes the SDK.  This must be called prior to calling any other SDK methods.
        /// See also: <see cref="ResetSdk"/>.
        /// </summary>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public static async Task InitializeSdk()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            // This method is intentionally async, even though we don"t currently make use of it.  This is
            // because of the high likelihood that it will be needed in the near future.  By making it async
            // now, it won"t be a breaking change when we do introduce the need to await within this method.

            if (_sdkInitialized)
            {
                throw new InvalidOperationException("The SDK is already initialized.");
            }

            //getIt.registerSingleton<file.FileSystem>(const file.LocalFileSystem());
            _sdkInitialized = true;
        }

        /// <summary>
        /// Resets the SDK to its initial state.
        /// See also: <see cref="InitializeSdk"/>.
        /// </summary>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public static async Task ResetSdk()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            // This method is intentionally async, even though we don"t currently make use of it.  This is
            // because of the high likelihood that it will be needed in the near future.  By making it async
            // now, it won"t be a breaking change when we do introduce the need to await within this method.

            if (!_sdkInitialized)
            {
                return;
            }

            //getIt.unregister<file.FileSystem>();
            _sdkInitialized = false;
        }
    }

    /// <summary>
    /// Options for configuring a connection to Nucleus One (e.g. base URL, API key).
    /// </summary>
    public class NucleusOneOptions
    {
        public string ApiBaseUrl { get; set; }
        public string ApiKey { get; set; }

        private const string DefaultApiBaseUrl = "https://client-api.nucleus.one";
        public const int UploadChunkSize = 1024 * 1024;

        /// <summary>
        /// Creates an instance of the <see cref="NucleusOneOptions"/>class.
        /// </summary>
        /// <param name="apiBaseUrl">
        /// The base URL for the Nucleus One API.  Note that this is different from the Nucleus One app URL.
        /// </param>
        /// <param name="apiKey">
        /// The API key to use when connecting.  This can be created by visiting your User Profile in Nucleus One then clicking "API Keys".
        /// </param>
        public NucleusOneOptions(
            string apiBaseUrl,
            string apiKey
        )
        {
            this.ApiBaseUrl = string.IsNullOrEmpty(apiBaseUrl) ? DefaultApiBaseUrl : apiBaseUrl;
            this.ApiKey = apiKey;
        }
    }

    /// Classes deriving from this class require an instance of the <see cref="NucleusOneApp"/>
    /// class in order to function.
    [Serializable]
    public abstract class NucleusOneAppDependent
    {
        [NonSerialized]
        NucleusOneApp _app;

        protected NucleusOneAppDependent(NucleusOneApp app)
        {
            InitializeApp(app);
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            InitializeApp();
        }

        private void InitializeApp(NucleusOneApp app = null)
        {
            _app = app ?? GetIt.Get<NucleusOneApp>();
        }

        /// The Nucleus One application object.  This controls API configuration.  See the
        /// <see cref="NucleusOneApp"/> class for details.
        public NucleusOneApp App
        {
            [DebuggerStepThrough]
            get => _app;
            [DebuggerStepThrough]
            protected set => _app = value;
        }

        // TODO: This would be better-handled by explicitly specifying generic type arguments
        protected internal static async Task<TModel[]> GetAllEntitiesByPages<TModel, TModelCollection>(
            Func<string, Task<dynamic>> getNextPageOfQueryResultHandler
        )
            where TModelCollection : IEnumerable<TModel>
        {
            var allEntityCollectionResults = new List<TModelCollection>();
            string cursor = null;

            do
            {
                var resultsPaged = await getNextPageOfQueryResultHandler(cursor);
                var results = resultsPaged.Results;

                allEntityCollectionResults.Add(results);

                int itemCount = ((Array)results.Items).Length;
                int pageSize = resultsPaged.PageSize;

                // If the result count is less than the cursor's page size then this is the end of the results
                if ((pageSize == 0) || (itemCount < pageSize))
                    break;

                cursor = resultsPaged.Cursor;
            } while (true);

            return allEntityCollectionResults
                .SelectMany(x => (IEnumerable<TModel>)((dynamic)x).Items)
                .ToArray();
        }

        protected async Task<QueryResult<TModelCollection, TModel, TApiModelCollection, TApiModel>>
            GetItemsPaged<TModelCollection, TModel, TApiModelCollection, TApiModel>(
            string apiRelativeUrlPath,
            Action<Dictionary<string, dynamic>> qpCallback,
            string cursor = null
        )
            where TApiModelCollection : class, new()
            where TApiModel : class, new()
            where TModelCollection : Common.Model.EntityCollection<TModel, TApiModelCollection>
            where TModel : Common.Model.Entity<TApiModel>
        {
            var qp = StandardQueryParams.Get(
                callbacks: new Action<StandardQueryParams>[] {
                    (sqp) => sqp.Cursor(cursor)
                }
            );

            if (qpCallback != null)
                qpCallback(qp);

            var responseBody = await Http.ExecuteGetRequestWithTextResponse(
                apiRelativeUrlPath: apiRelativeUrlPath,
                app: App,
                queryParams: qp
            );

            var apiModel = ApiModel.QueryResult<TApiModelCollection>.FromJson(responseBody);

            return Common.Util.DefineN1AppInScope(App, () =>
            {
                return QueryResult<TModelCollection, TModel, TApiModelCollection, TApiModel>
                    .FromApiModel(apiModel);
            });
        }
    }

    /// <summary>
    /// The Nucleus One application.  This is a core class needed for most operations.
    /// </summary>
    public class NucleusOneApp
    {
        public const string ApiBaseUrlPath = "/api/v1";
        public NucleusOneOptions Options
        {
            [DebuggerStepThrough]
            get;
            [DebuggerStepThrough]
            private set;
        }
        private readonly string _baseUrlWithApi;

        /// <summary>
        /// Creates an instance of the <see cref="NucleusOneApp"/> class.
        /// </summary>
        /// <param name="options">The options to use when connecting to Nucleus One.</param>
        public NucleusOneApp(
            NucleusOneOptions options
        )
        {
            this.Options = options;
            _baseUrlWithApi = options.ApiBaseUrl + ApiBaseUrlPath;
        }

        /// <summary>Internal use only.</summary>
        public string GetFullUrl(string apiRelativeUrlPath)
        {
            return _baseUrlWithApi + apiRelativeUrlPath;
        }

        /// <summary>
        /// Creates a <see cref="NucleusOneAppOrganization"/> object, which can be used to perform
        /// operations specific to this organization.
        /// </summary>
        public Hierarchy.NucleusOneAppOrganization Organization(string organizationId)
        {
            return new Hierarchy.NucleusOneAppOrganization(
                app: this,
                id: organizationId
            );
        }

        /// <summary>
        /// Gets an organization by ID.
        /// </summary>
        public async Task<OrganizationForClient> GetOrganization(string id)
        {
            var responseBody = await Http.ExecuteGetRequestWithTextResponse(
                    apiRelativeUrlPath: ApiPaths.OrganizationsOrganizationFormat
                        .ReplaceOrgIdPlaceholder(id),
                    app: this
                )
                .ConfigureAwait(true);

            var apiModel = ApiModel.OrganizationForClient.FromJson(responseBody);

            // As of writing this, there is a bug with the N1 API where, if the ID of a non-existent
            // organization is provided, it will return a random organization.
            if ((apiModel == null) || (apiModel.Id != id))
                return null;

            return Common.Util.DefineN1AppInScope(this, () => {
                return OrganizationForClient.FromApiModel(apiModel);
            });
        }

        /// <summary>
        /// Gets organizations that the current user is a member of, by page.
        /// </summary>
        public async Task<QueryResult<OrganizationForClientCollection, OrganizationForClient, ApiModel.OrganizationForClientCollection, ApiModel.OrganizationForClient>>
            GetOrganizations(string cursor = null)
        {
            var qp = StandardQueryParams.Get(
                callbacks: new Action<StandardQueryParams>[]{
                    (sqp) => sqp.Cursor(cursor)
                }
            );
            var responseBody = await Http.ExecuteGetRequestWithTextResponse(
                    apiRelativeUrlPath: ApiPaths.Organizations,
                    app: this,
                    queryParams: qp
                )
                .ConfigureAwait(true);

            var apiModel = ApiModel.QueryResult<ApiModel.OrganizationForClientCollection>.FromJson(responseBody);

            return Common.Util.DefineN1AppInScope(this, () => {
                return QueryResult<OrganizationForClientCollection, OrganizationForClient, ApiModel.OrganizationForClientCollection, ApiModel.OrganizationForClient>
                    .FromApiModel(apiModel);
            });
        }
    }
}