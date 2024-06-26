﻿using Ademero.NucleusOneDotNetSdk.Common;
using Ademero.NucleusOneDotNetSdk.Common.Strings;
using Ademero.NucleusOneDotNetSdk.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Ademero.NucleusOneDotNetSdk.Hierarchy
{
    /// <summary>
    /// Performs organization operations.
    /// </summary>
    public class NucleusOneAppOrganization : NucleusOneAppDependent
    {
        /// <summary>
        /// The organization ID.
        /// </summary>
        public string Id
        {
            [DebuggerStepThrough]
            get;
        }

        /// <summary>
        /// Creates an instance of the NucleusOneAppOrganization class.
        /// </summary>
        /// <param name="id">The organization's ID.</param>
        /// <param name="app">The application to use when connecting to Nucleus One.</param>
        /// <exception cref="ArgumentException"></exception>
        public NucleusOneAppOrganization(string id, NucleusOneApp app = null) : base(app)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Value cannot be blank.", nameof(id));

            Id = id;
        }

        /// <summary>
        /// Gets a NucleusOneAppProject instance, which can be used to perform project operations for this organization.
        /// </summary>
        public NucleusOneAppProject Project(string projectId)
        {
            return new NucleusOneAppProject(this, projectId);
        }

        /*

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

            var apiModel = Common.Util.DeserializeObject<ApiMod.OrganizationPermissions>(responseBody);
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

            var apiModel = Common.Util.DeserializeObject<ApiMod.QueryResult<ApiMod.DocumentSubscriptionForClientCollection>>(responseBody);

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

            var apiModel = Common.Util.DeserializeObject<ApiMod.SubscriptionDetails>(responseBody);
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

            var apiModel = Common.Util.DeserializeObject<ApiMod.OrganizationProject>(responseBody);
            return await Util.DefineN1AppInScope(App, () => OrganizationProject.FromApiModel(apiModel));
        }
        */

        /// <summary>
        /// Gets all projects that the current user is a member of.
        /// </summary>
        /// <param name="cursor">The ID of the cursor, from a previous query. Used for paging results.</param>
        /// <param name="projectAccessType">
        /// Can be any of the following values:
        /// - GlobalAssignments_MemberContentByDefault
        /// - MembersOnlyAssignments_MemberContentByAssignment
        /// </param>
        /// <param name="nameFilter">Filters results to only those projects starting with this value.</param>
        /// <param name="getAll">Returns all projects in a single results, without using paging.</param>
        /// <param name="adminOnly">If true, only projects that the current user is an administrator of will be returned.</param>
        public async Task<OrganizationProjectCollection> GetAllProjects(
            string projectAccessType = null, string nameFilter = null, bool? getAll = null, bool? adminOnly = null
        )
        {
            Func<string, Task<dynamic>> getNextPageHandler = async (string cursor) =>
                await GetProjectsPaged(cursor, projectAccessType, nameFilter, getAll, adminOnly);
            OrganizationProject[] allResults = await GetAllEntitiesByPages<OrganizationProject, OrganizationProjectCollection>(
                getNextPageHandler
            );
            return new OrganizationProjectCollection(allResults, App);
        }

        /// <summary>
        /// Gets projects that the current user is a member of, by page.
        /// </summary>
        /// <inheritdoc cref="GetAllProjects" />
        public async Task<QueryResult<OrganizationProjectCollection, OrganizationProject, ApiModel.OrganizationProjectCollection, ApiModel.OrganizationProject>> GetProjectsPaged(
            string cursor = null, string projectAccessType = null, string nameFilter = null, bool? getAll = null, bool? adminOnly = null)
        {
            var apiRelativeUrlPath = ApiPaths.OrganizationsProjectsFormat.ReplaceOrgIdPlaceholder(Id);
            Action<Dictionary<string, dynamic>> qpCallback = (qp) =>
            {
                Util.SetDictionaryValueIfNotNull(qp, "projectAccessType", projectAccessType);
                Util.SetDictionaryValueIfNotNull(qp, "nameFilter", nameFilter);
                Util.SetDictionaryValueIfNotNull(qp, "getAll", getAll);
                Util.SetDictionaryValueIfNotNull(qp, "adminOnly", adminOnly);
            };
            return await GetItemsPaged<OrganizationProjectCollection, OrganizationProject, ApiModel.OrganizationProjectCollection, ApiModel.OrganizationProject>(
                apiRelativeUrlPath, qpCallback, cursor
            );
        }

        /*
        /// <summary>
        /// Gets membership packages for this organization, which the current user has access to.
        /// </summary>
        public async Task<QueryResult<OrganizationMembershipPackageCollection, ApiMod.OrganizationMembershipPackageCollection>>
            GetMembershipPackages()
        {
            return await App.GetOrganizationMembershipPackages(Id);
        }
        */

        public async Task<Model.OrganizationProject> CreateProject(string projectName,
            string templateId = null, bool sourceContentCopy = false)
        {
            var qp = StandardQueryParams.Get();
            qp["homePath"] = Common.PathHelper.GetOrganizationLink(Id, Common.PathHelper.GetHomePath());
            qp["projectPathPrefix"] = Common.PathHelper.GetOrganizationLink(Id, Common.PathHelper.GetProjectsPath());

            string body = Common.Util.JsonSerializeObject(
                new[] {
                    new {
                        Name = projectName,
                        SourceID = templateId,
                        SourceContentCopy = sourceContentCopy
                    }
                });

            string responseBody = await Http.ExecutePostRequestWithTextResponse(
                    apiRelativeUrlPath: ApiPaths.OrganizationsProjectsFormat.ReplaceOrgIdPlaceholder(Id),
                    queryParams: qp,
                    app: App,
                    body: body
                )
                .ConfigureAwait(true);

            var apiModel = ApiModel.OrganizationProjectCollection.FromJsonArray(
                arrayItemsJson: responseBody,
                instance: new ApiModel.OrganizationProjectCollection(),
                entityFromJsonCallback: (x) => ApiModel.OrganizationProject.FromJson(x)
            );

            return Common.Util.DefineN1AppInScope(App, () =>
            {
                var createdProjects = Model.OrganizationProjectCollection.FromApiModel(apiModel);
                if ((createdProjects == null) || (createdProjects.Items.Length == 0))
                    return null;
                return createdProjects.Items[0];
            });
        }

        public async Task DeleteProject(string projectId)
        {
            string body = Common.Util.JsonSerializeObject(
                new
                {
                    IDs = new string[] { projectId }
                });

            await Http.ExecuteDeleteRequest(
                    apiRelativeUrlPath: ApiPaths.OrganizationsProjectsFormat.ReplaceOrgIdPlaceholder(Id),
                    app: App,
                    body: body
                )
                .ConfigureAwait(true);
        }

        public async Task<Model.OrganizationMemberCollection> GetMembers()
        {
            var qp = StandardQueryParams.Get();
            qp["homePath"] = Common.PathHelper.GetOrganizationLink(Id, Common.PathHelper.GetHomePath());

            string responseBody = await Http.ExecuteGetRequestWithTextResponse(
                    apiRelativeUrlPath: ApiPaths.OrganizationMembers.ReplaceOrgIdPlaceholder(Id),
                    queryParams: qp,
                    app: App
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

        public async Task<Model.OrganizationMemberCollection> AddMembers(Model.OrganizationMemberCollection users)
        {
            var qp = StandardQueryParams.Get();
            qp["homePath"] = Common.PathHelper.GetOrganizationLink(Id, Common.PathHelper.GetHomePath());

            string body = Common.Util.JsonSerializeObject(users.Items);

            string responseBody = await Http.ExecutePostRequestWithTextResponse(
                    apiRelativeUrlPath: ApiPaths.OrganizationMembers.ReplaceOrgIdPlaceholder(Id),
                    queryParams: qp,
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