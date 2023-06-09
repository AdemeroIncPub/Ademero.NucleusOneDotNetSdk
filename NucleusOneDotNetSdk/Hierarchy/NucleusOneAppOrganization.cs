using NucleusOneDotNetSdk.Common.Strings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NucleusOneDotNetSdk.Hierarchy
{
    /// <summary>
    /// Performs organization operations.
    /// </summary>
    public class NucleusOneAppOrganization : NucleusOneAppDependent
    {
        /// <summary>
        /// The organization ID.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Creates an instance of the NucleusOneAppOrganization class.
        /// </summary>
        /// <param name="id">The organization's ID.</param>
        /// <param name="app">The application to use when connecting to Nucleus One.</param>
        /// <exception cref="ArgumentException"></exception>
        public NucleusOneAppOrganization(string id, NucleusOneApp app = null) :base(app)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Value cannot be blank.", nameof(id));

            Id = id;
        }

        /*
        /// <summary>
        /// Gets a NucleusOneAppProject instance, which can be used to perform project operations for this organization.
        /// </summary>
        public NucleusOneAppProject Project(string projectId)
        {
            return new NucleusOneAppProject(projectId, this);
        }

        /// <summary>
        /// Gets a NucleusOneAppSubscriptions instance, which can be used to perform subscription operations for this organization.
        /// </summary>
        public NucleusOneAppSubscriptions Subscriptions()
        {
            return new NucleusOneAppSubscriptions(this);
        }

        /// <summary>
        /// Gets the current user's permissions within this organization.
        /// </summary>
        public async Task<OrganizationPermissions> GetPermissions()
        {
            var responseBody = await Http.ExecuteGetRequestWithTextResponse(
                ApiPaths.OrganizationsPermissionsFormat.ReplaceOrgIdPlaceholder(Id), App);

            var apiModel = JsonConvert.DeserializeObject<ApiMod.OrganizationPermissions>(responseBody);
            return await Util.DefineN1AppInScope(App, () => OrganizationPermissions.FromApiModel(apiModel));
        }

        /// <summary>
        /// Gets the current user's document subscriptions within this organization.
        /// </summary>
        public async Task<QueryResult<DocumentSubscriptionForClientCollection, ApiMod.DocumentSubscriptionForClientCollection>>
            GetDocumentSubscriptions()
        {
            var responseBody = await Http.ExecuteGetRequestWithTextResponse(
                ApiPaths.OrganizationsOrganizationDocumentSubscriptionsFormat.ReplaceOrgIdPlaceholder(Id), App);

            var apiModel = JsonConvert.DeserializeObject<ApiMod.QueryResult<ApiMod.DocumentSubscriptionForClientCollection>>(responseBody);

            return await Util.DefineN1AppInScope(App, () =>
            {
                return new QueryResult<DocumentSubscriptionForClientCollection, ApiMod.DocumentSubscriptionForClientCollection>(
                    new DocumentSubscriptionForClientCollection(
                        apiModel.Results.DocumentSubscriptions.Select(x => DocumentSubscriptionForClient.FromApiModel(x)).ToList()),
                    apiModel.Cursor,
                    apiModel.PageSize);
            });
        }

        /// <summary>
        /// Gets an organization's subscription.
        /// organizationId: The organization's ID.
        /// </summary>
        public async Task<SubscriptionDetails> GetOrganizationSubscription(string organizationId)
        {
            var responseBody = await Http.ExecuteGetRequestWithTextResponse(
                ApiPaths.OrganizationsSubscriptionsFormat.ReplaceOrgIdPlaceholder(Id), App);

            var apiModel = JsonConvert.DeserializeObject<ApiMod.SubscriptionDetails>(responseBody);
            return await Util.DefineN1AppInScope(App, () => SubscriptionDetails.FromApiModel(apiModel));
        }

        /// <summary>
        /// Gets a project within this organization, by ID.
        /// projectId: The organization's project ID.
        /// </summary>
        public async Task<OrganizationProject> GetProject(string projectId)
        {
            var qp = StandardQueryParams.Get();

            var responseBody = await Http.ExecuteGetRequestWithTextResponse(
                ApiPaths.OrganizationsProjectsProjectFormat.ReplaceOrgIdAndProjectIdPlaceholders(Id, projectId),
                App,
                qp);

            var apiModel = JsonConvert.DeserializeObject<ApiMod.OrganizationProject>(responseBody);
            return await Util.DefineN1AppInScope(App, () => OrganizationProject.FromApiModel(apiModel));
        }

        /// <summary>
        /// Gets projects that the current user is a member of, by page.
        /// cursor: The ID of the cursor, from a previous query. Used for paging results.
        /// projectAccessType: Can be any of the following values:
        /// - GlobalAssignments_MemberContentByDefault
        /// - MembersOnlyAssignments_MemberContentByAssignment
        /// nameFilter: Filters results to only those projects starting with this value.
        /// getAll: Returns all projects in a single results, without using paging.
        /// adminOnly: If true, only projects that the current user is an administrator of will be returned.
        /// </summary>
        public async Task<QueryResult<OrganizationProjectCollection, ApiMod.OrganizationProjectCollection>>
            GetProjects(string? cursor = null, string? projectAccessType = null, string? nameFilter = null, bool getAll = false, bool adminOnly = false)
        {
            var qp = new Dictionary<string, string>
            {
                {"cursor", cursor},
                {"projectAccessType", projectAccessType},
                {"nameFilter", nameFilter},
                {"getAll", getAll.ToString()},
                {"adminOnly", adminOnly.ToString()}
            };

            var responseBody = await Http.ExecuteGetRequestWithTextResponse(
                ApiPaths.OrganizationsProjectsFormat.ReplaceOrgIdPlaceholder(Id),
                App,
                qp);

            var apiModel = JsonConvert.DeserializeObject<ApiMod.QueryResult<ApiMod.OrganizationProjectCollection>>(responseBody);

            return await Util.DefineN1AppInScope(App, () =>
            {
                return new QueryResult<OrganizationProjectCollection, ApiMod.OrganizationProjectCollection>(
                    new OrganizationProjectCollection(
                        apiModel.Results.Projects.Select(x => OrganizationProject.FromApiModel(x)).ToList()),
                    apiModel.Cursor,
                    apiModel.PageSize);
            });
        }

        /// <summary>
        /// Gets membership packages for this organization, which the current user has access to.
        /// </summary>
        public async Task<QueryResult<OrganizationMembershipPackageCollection, ApiMod.OrganizationMembershipPackageCollection>>
            GetMembershipPackages()
        {
            return await App.GetOrganizationMembershipPackages(Id);
        }
        */

        public async Task<Model.OrganizationMemberCollection> AddMembers(Model.OrganizationMemberCollection users)
        {
            string body = Common.Util.SerializeObject(users.Items);

            string responseBody = await Http.ExecutePostRequestWithTextResponse(
                    apiRelativeUrlPath: ApiPaths.organizationMembers.ReplaceOrgIdPlaceholder(Id),
                    queryParams: new Dictionary<string, dynamic>()
                    {
                        { "homePath", Common.PathHelper.GetOrganizationLink(Id, Common.PathHelper.GetHomePath()) }
                    },
                    app: App,
                    body: body
                )
                .ConfigureAwait(true);

            var apiModel = ApiModel.OrganizationMemberCollection.FromJsonArray(
                arrayItemsJson: responseBody,
                instance: new ApiModel.OrganizationMemberCollection(),
                entityFromJsonCallback: (x) => ApiModel.OrganizationMember.FromJson(x)
            );

            return Common.Util.DefineN1AppInScope(App, () =>
            {
                return Model.OrganizationMemberCollection.FromApiModel(apiModel);
            });
        }
    }
}