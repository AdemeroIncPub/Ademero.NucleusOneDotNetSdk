using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Ademero.NucleusOneDotNetSdk.ApiModel
{
    [Serializable]
    public class QueryResult<T>
        where T : class
    {
        public static readonly Dictionary<Type, Func<string, object>> fromJsonFactories = new Dictionary<Type, Func<string, object>>
        {
            { typeof(ApiModel.DocumentFolderCollection), (x) => ApiModel.DocumentFolderCollection.FromJson(x) },
            { typeof(ApiModel.DocumentUploadCollection), (x) => ApiModel.DocumentUploadCollection.FromJson(x) },
            { typeof(ApiModel.FieldCollection), (x) => ApiModel.FieldCollection.FromJson(x) },
            { typeof(ApiModel.OrganizationForClientCollection), (x) => ApiModel.OrganizationForClientCollection.FromJson(x) },
            { typeof(ApiModel.OrganizationMemberCollection), (x) => ApiModel.OrganizationMemberCollection.FromJson(x) },
            { typeof(ApiModel.OrganizationProjectCollection), (x) => ApiModel.OrganizationProjectCollection.FromJson(x) }
        };

        public QueryResult() { }

#pragma warning disable CA1000  // Do not declare static members on generic types
        public static QueryResult<T> FromJson(
#pragma warning restore CA1000  // Do not declare static members on generic types
            string json,
            Func<string, object> fromJsonFactoryOverride = null
        )
        {
            return ResultsFromJson<QueryResult<T>>(
                false/*, _$QueryResultFromJson*/, fromJsonFactories, json, fromJsonFactoryOverride);
        }

        public string Cursor { get; set; }
        public int PageSize { get; set; }
        public T Results { get; set; }

        protected static TQueryResult ResultsFromJson<TQueryResult>(
            bool isQueryResult2,
            Dictionary<Type, Func<string, object>> fromJsonFactories,
            string json,
            Func<string, object> fromJsonFactoryOverride = null)
            where TQueryResult : QueryResult<T>
        {
            var r = Common.Util.DeserializeObject<TQueryResult>(json);
            var fromJsonFactory = fromJsonFactoryOverride ?? fromJsonFactories[typeof(T)];
            if (fromJsonFactory == null)
            {
                var className = isQueryResult2 ? "QueryResult2" : "QueryResult";
                throw new NotImplementedException(
                    $"The {nameof(T)}.fromJson factory constructor must be explicitly registered in the API model {className} class.");
            }
            r.Results = fromJsonFactory(json) as T;

            return r;
        }

    }

    /// <summary>
    /// </summary>
    [Serializable]
    public abstract class QueryResultEntityCollection<TCollection, TEntity> : Common.ApiModel.EntityCollection<TEntity>
        where TEntity : Common.ApiModel.Entity<TEntity>, new()
    {
#pragma warning disable CA1000  // Do not declare static members on generic types
        public static TCollection FromJson(string json) => Common.Util.DeserializeObject<TCollection>(json);
#pragma warning restore CA1000  // Do not declare static members on generic types
    }
}