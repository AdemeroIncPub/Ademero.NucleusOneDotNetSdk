using System;
using System.Collections.Generic;

namespace Ademero.NucleusOneDotNetSdk.Model
{
    [Serializable]
    public class QueryResult<TModelCollection, TModel, TApiModelCollection, TApiModel> : NucleusOneAppDependent
        where TApiModelCollection : class, new()
        where TApiModel : class, new()
        where TModelCollection : Common.Model.EntityCollection<TModel, TApiModelCollection>
        where TModel : Common.Model.Entity<TApiModel>
    {
        private static readonly Dictionary<Type, Func<dynamic, object>> _fromApiModelFactories =
            new Dictionary<Type, Func<dynamic, object>>()
            {
                /*
                ...QueryResult2._fromApiModelFactories, // Include factories from the QueryResult2 class
                mod.DocumentCollection: (x) => mod.DocumentCollection.fromApiModel(x),
                */
                { typeof(Model.DocumentFolderCollection), (x) => Model.DocumentFolderCollection.FromApiModel(x) },
                /*
                mod.DocumentSubscriptionForClientCollection: (x) =>
                    mod.DocumentSubscriptionForClientCollection.fromApiModel(x),
                */
                { typeof(Model.FieldCollection), (x) => Model.FieldCollection.FromApiModel(x) },
                /*
                mod.ApprovalCollection: (x) => mod.ApprovalCollection.fromApiModel(x),
                mod.FolderHierarchyCollection: (x) => mod.FolderHierarchyCollection.fromApiModel(x),
                mod.FormTemplateCollection: (x) => mod.FormTemplateCollection.fromApiModel(x),
                mod.TaskCollection: (x) => mod.TaskCollection.fromApiModel(x),
                */
                { typeof(Model.OrganizationForClientCollection), (x) => Model.OrganizationForClientCollection.FromApiModel(x) },
                { typeof(Model.OrganizationMemberCollection), (x) => Model.OrganizationMemberCollection.FromApiModel(x) },
                { typeof(Model.OrganizationProjectCollection), (x) => Model.OrganizationProjectCollection.FromApiModel(x) },
                /*
                mod.OrganizationMembershipPackageCollection: (x) =>
                    mod.OrganizationMembershipPackageCollection.fromApiModel(x),
                mod.OrganizationPackageCollection: (x) => mod.OrganizationPackageCollection.fromApiModel(x),
                mod.OrganizationProjectCollection: (x) => mod.OrganizationProjectCollection.fromApiModel(x),
                mod.SupportUserCollection: (x) => mod.SupportUserCollection.fromApiModel(x),
                mod.SupportOrganizationCollection: (x) => mod.SupportOrganizationCollection.fromApiModel(x),
                mod.SupportErrorEventCollection: (x) => mod.SupportErrorEventCollection.fromApiModel(x),
                mod.UserOrganizationProjectCollection: (x) =>
                    mod.UserOrganizationProjectCollection.fromApiModel(x),
                */
            };

        private static TQueryResult ResultsFromApiModelInternal<TQueryResult>(
            bool isQueryResult2,
            Func<dynamic, Func<dynamic, object>, TQueryResult> qrFromApiModelHandler,
            Dictionary<Type, Func<dynamic, object>> fromApiModelFactories,
            dynamic apiModel
        )
        {
            Func<dynamic, object> fromApiModel = null;

            if (fromApiModelFactories.ContainsKey(typeof(TModelCollection)))
            {
                fromApiModel = fromApiModelFactories[typeof(TModelCollection)];
            }

            if (fromApiModel == null)
            {
                string className = isQueryResult2 ? "QueryResult2" : "QueryResult";
                throw new NotImplementedException(
                    $"The {typeof(TModelCollection)}.FromApiModel factory constructor must be explicitly registered in the model {className} class.");
            }

            TQueryResult result = qrFromApiModelHandler(apiModel, fromApiModel);
            return result;
        }

        protected QueryResult(
            TModelCollection results,
            string cursor,
            int pageSize,
            NucleusOneApp app = null
        ) : base(app)
        {
            this.Results = results;
            this.Cursor = cursor;
            this.PageSize = pageSize;
        }

#pragma warning disable CA1000 // Do not declare static members on generic types
        public static QueryResult<TModelCollection, TModel, TApiModelCollection, TApiModel> FromApiModel(
            dynamic apiModel)
#pragma warning restore CA1000 // Do not declare static members on generic types
        {
            //return _resultsFromApiModel<QueryResult<TModelCollection, TModel, TApiModelCollection, TApiModel>>(
            //    false, _fromApiModel, _fromApiModelFactories, apiModel);
            var fromApiModelLocal = new Func<dynamic, Func<dynamic, object>, QueryResult<TModelCollection, TModel, TApiModelCollection, TApiModel>>(
                (apiModelLocal, fromApiModelHandler) =>
                    FromApiModelInternal<QueryResult<TModelCollection, TModel, TApiModelCollection, TApiModel>>(apiModelLocal, fromApiModelHandler)
            );

            return ResultsFromApiModelInternal<QueryResult<TModelCollection, TModel, TApiModelCollection, TApiModel>>(
                false, fromApiModelLocal, _fromApiModelFactories, apiModel);
        }

        private static TQueryResult FromApiModelInternal<TQueryResult>(
            dynamic apiModel,
            Func<dynamic, object> fromApiModel
        )
            where TQueryResult : QueryResult<TModelCollection, TModel, TApiModelCollection, TApiModel>
        {
            return (TQueryResult)
                new QueryResult<TModelCollection, TModel, TApiModelCollection, TApiModel>(
                    results: fromApiModel(apiModel.Results) as TModelCollection,
                    cursor: apiModel.Cursor,
                    pageSize: apiModel.PageSize
                );
        }

        public TModelCollection Results { get; private set; }
        public string Cursor { get; private set; }
        public int PageSize { get; private set; }

        //@override
        //@visibleForTesting
        //  @protected
        //  api_mod.QueryResult<TApiModel> toApiModel() {
        //    return api_mod.QueryResult<TApiModel>()..results = results.toApiModel()..cursor = cursor..pageSize = pageSize;
        //}

        protected ApiModel.QueryResult<TApiModelCollection> ToApiModel()
        {
            return null;
            //return new ApiModel.QueryResult<TApiModelCollection>()
            //{
            //    Results = Results.ToApiModel(),
            //    Cursor = Cursor,
            //    PageSize = PageSize
            //};
        }
    }
}