using System;

namespace Ademero.NucleusOneDotNetSdk.Model
{
    [Serializable]
    public class QueryResult<TModelCollection, TModel, TApiModelCollection, TApiModel> : NucleusOneAppDependent
        where TApiModelCollection : class, new()
        where TApiModel : class, new()
        where TModelCollection : Common.Model.EntityCollection<TModel, TApiModelCollection>
        where TModel : Common.Model.Entity<TApiModel>
    {
        private static FromApiModelDelegate _fromApiModelFactory;

        private delegate TModelCollection FromApiModelDelegate(TApiModelCollection apiModel, NucleusOneApp app);

        // This is invoked once for each derived, concrete class
        static QueryResult()
        {
            var fromApiModelMethod = typeof(TModelCollection).GetMethod("FromApiModel",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy);

            if (fromApiModelMethod == null)
                throw new NotImplementedException($"The {typeof(TModelCollection)}.FromApiModel factory constructor was not found.");

            _fromApiModelFactory = (FromApiModelDelegate)
                Delegate.CreateDelegate(typeof(FromApiModelDelegate), null, fromApiModelMethod);
        }

        private static TQueryResult ResultsFromApiModelInternal<TQueryResult>(
            bool isQueryResult2,
            Func<dynamic, FromApiModelDelegate, TQueryResult> qrFromApiModelHandler,
            dynamic apiModel
        )
        {
            return qrFromApiModelHandler(apiModel, _fromApiModelFactory);
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
            var fromApiModelLocal = new Func<dynamic, FromApiModelDelegate, QueryResult<TModelCollection, TModel, TApiModelCollection, TApiModel>>(
                (apiModelLocal, fromApiModelHandler) =>
                    FromApiModelInternal<QueryResult<TModelCollection, TModel, TApiModelCollection, TApiModel>>(apiModelLocal, fromApiModelHandler)
            );

            return ResultsFromApiModelInternal<QueryResult<TModelCollection, TModel, TApiModelCollection, TApiModel>>(
                false, fromApiModelLocal, apiModel);
        }

        private static TQueryResult FromApiModelInternal<TQueryResult>(
            dynamic apiModel,
            FromApiModelDelegate fromApiModel
        )
            where TQueryResult : QueryResult<TModelCollection, TModel, TApiModelCollection, TApiModel>
        {
            NucleusOneApp n1App = null;
            return (TQueryResult)
                new QueryResult<TModelCollection, TModel, TApiModelCollection, TApiModel>(
                    results: fromApiModel(apiModel.Results, n1App) as TModelCollection,
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