using System;

namespace Ademero.NucleusOneDotNetSdk.ApiModel
{
    [Serializable]
    public class QueryResult<T>
        where T : class
    {
        public static readonly FromJsonDelegate _fromJsonFactory;

        public delegate T FromJsonDelegate(string json);

        // This is invoked once for each derived, concrete class
        static QueryResult()
        {
            var fromApiModelMethod = typeof(T).GetMethod("FromJson",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy);

            if (fromApiModelMethod == null)
                throw new NotImplementedException($"The {nameof(T)}.FromJson factory constructor was not found.");

            _fromJsonFactory = (FromJsonDelegate)
                Delegate.CreateDelegate(typeof(FromJsonDelegate), null, fromApiModelMethod);
        }

        public QueryResult() { }

#pragma warning disable CA1000  // Do not declare static members on generic types
        public static QueryResult<T> FromJson(
#pragma warning restore CA1000  // Do not declare static members on generic types
            string json,
            FromJsonDelegate fromJsonFactoryOverride = null
        )
        {
            return ResultsFromJson<QueryResult<T>>(
                false/*, _$QueryResultFromJson*/, json, fromJsonFactoryOverride);
        }

        public string Cursor { get; set; }
        public int PageSize { get; set; }
        public T Results { get; set; }

        protected static TQueryResult ResultsFromJson<TQueryResult>(
            bool isQueryResult2,
            string json,
            FromJsonDelegate fromJsonFactoryOverride = null)
            where TQueryResult : QueryResult<T>
        {
            var r = Common.Util.DeserializeObject<TQueryResult>(json);
            var fromJsonFactory = fromJsonFactoryOverride ?? _fromJsonFactory;
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