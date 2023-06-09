using System.Collections;
using System.Collections.Generic;

namespace Ademero.NucleusOneDotNetSdk.Common.Model
{
    public interface IToApiModel<T>
    {
        T ToApiModel();
    }

    /// <summary>
    /// The base class for all entities.
    /// </summary>
    public abstract class Entity<TApiModel> : NucleusOneAppDependent, IToApiModel<TApiModel>
    {
        public Entity(NucleusOneApp app) : base(app) { }

        public abstract TApiModel ToApiModel();
    }

    /// <summary>
    /// Base class for Model classes which support paging.
    /// </summary>
    public abstract class IModelPagingCursor
    {
        /// <summary>
        /// The ID of the cursor, from a previous query.  Used for paging results.
        /// </summary>
        public abstract string Cursor { get; set; }

        /// <summary>
        /// The size of each page of results returned from a query.  Used for paging results.
        /// </summary>
        public abstract int PageSize { get; set; }
    }

    /// <summary>
    /// Base class for Model classes which support paging and a reverse cursor.
    /// </summary>
    public abstract class IModelPagingCursor2 : IModelPagingCursor
    {
        /// <summary>
        /// The ID of the cursor used for getting results in reverse order, from a previous query.  Used
        /// for paging results.
        /// </summary>
        public abstract string ReverseCursor { get; set; }
    }

    /// <summary>
    /// </summary>
    public abstract class EntityCollection<TResult, TApiModel>
        : NucleusOneAppDependent, IEnumerable, IEnumerable<TResult>
        where TResult : NucleusOneAppDependent
    {
        /// <summary>
        /// Creates an EntityCollection instance.
        /// </summary>
        /// <param name="app">The application to use when connecting to Nucleus One.</param>
        /// <param name="items">The items to insert into this new collection.</param>
        public EntityCollection(
            NucleusOneApp app,
            TResult[] items
        ) : base(app)
        {
            this._items = items ?? System.Array.Empty<TResult>();
            this.App = app ?? GetIt.Get<NucleusOneApp>();
        }

        private readonly TResult[] _items;

        /// <summary>
        /// Gets all items in this collections.
        /// </summary>
        public TResult[] Items { get => _items; }

        // @override
        // Iterator<T> get iterator => EntityCollectionIterator(_items);

        // T operator [](int i) => _items[i];

        /// <summary>
        /// Gets the API Model representation of this object.
        /// </summary>
        /// <returns></returns>
        public abstract TApiModel ToApiModel();

        public IEnumerator GetEnumerator() => _items?.GetEnumerator();
        
        IEnumerator<TResult> IEnumerable<TResult>.GetEnumerator() => ((IEnumerable<TResult>)_items).GetEnumerator();
    }
}
