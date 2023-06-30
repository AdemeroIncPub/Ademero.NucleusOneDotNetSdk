using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ademero.NucleusOneDotNetSdk.Common.ApiModel
{
    /// <summary>
    /// Base class for API Model classes which support paging.
    /// </summary>
    [Serializable]
    public abstract class IApiModelPagingCursor
    {
        public abstract string Cursor { get; set; }
        public abstract int? PageSize { get; set; }
    }

    /// <summary>
    /// Base class for API Model classes which support paging and a reverse cursor.
    /// </summary>
    [Serializable]
    public abstract class IApiModelPagingCursor2 : IApiModelPagingCursor
    {
        public abstract string ReverseCursor { get; set; }
    }

    /// <summary>
    /// The base class for all entities.
    /// </summary>
    [Serializable]
    public abstract class Entity<T>
        where T : class, new()
    {
        /// <summary>
        /// Creates an instance of <see cref="Entity&lt;T&gt;"/> from a JSON string.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
#pragma warning disable CA1000  // Do not declare static members on generic types
        public static T FromJson(string json)
#pragma warning restore CA1000  // Do not declare static members on generic types
        {
            return Util.DeserializeObject<T>(json);
        }

        /// <summary>
        /// Get this object as JSON.
        /// </summary>
        public virtual string ToJson() => Util.SerializeObject(this);
    }

    /// <summary>
    /// Supports serialization of JSON structures that have an array at their root, which is not
    /// currently implicitly supported by json_serializable.
    /// https://github.com/google/json_serializable.dart/issues/648
    /// </summary>
    [Serializable]
    [JsonArray]
    public abstract class EntityCollection<TEntity>: IEnumerable, IEnumerable<TEntity>
        where TEntity : Entity<TEntity>, new()
    {
        /// <summary>
        /// Creates an instance of the <see cref="EntityCollection{TEntity}"/> class from JSON.
        /// </summary>
#pragma warning disable CA1000  // Do not declare static members on generic types
        public static TCollection FromJsonArray<TCollection>(
#pragma warning restore CA1000  // Do not declare static members on generic types
            string arrayItemsJson,
            TCollection instance,
            Func<string, TEntity> entityFromJsonCallback
        )
            where TCollection : EntityCollection<TEntity>
        {
            // Split the JSON array into a string array.  Each array element is the JSON for the individual JSON object.
            var arrayItemsJsons = Newtonsoft.Json.Linq.JArray.Parse(arrayItemsJson)
                .Select(x => x.ToString(Formatting.None))
                .ToArray();

            instance.Items = arrayItemsJsons
                .Select((m) => entityFromJsonCallback(m))
                .ToArray();
            return instance;
        }

        /// <summary>
        /// The items in the array.
        /// </summary>
        protected TEntity[] Items { get; set; } = Array.Empty<TEntity>();

        /// <summary>
        /// Get this object as JSON.
        /// </summary>
        public string ToJson() => Common.Util.SerializeObject(Items);

        public IEnumerator GetEnumerator() => Items?.GetEnumerator();

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator() => ((IEnumerable<TEntity>)Items).GetEnumerator();
    }
}