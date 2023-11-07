using Ademero.NucleusOneDotNetSdk.Model;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ademero.NucleusOneDotNetSdk.Common.Model
{
    public interface IToApiModel<T>
    {
        T ToApiModel();
    }

    /// <summary>
    /// The base class for all entities.
    /// </summary>
    [Serializable]
    public abstract class Entity<TApiModel> : NucleusOneAppDependent, IToApiModel<TApiModel>
    {
        public Entity(NucleusOneApp app) : base(app) { }

        public abstract TApiModel ToApiModel();
    }

    /// <summary>
    /// Base class for Model classes which support paging.
    /// </summary>
    [Serializable]
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
    [Serializable]
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
    [Serializable]
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
        /// Gets all items in this collection.
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

    /// <summary>
    /// Contains logic for operating on list items used with the Nucleus One API.
    /// </summary>
    public abstract class ListItems
    {
        /// <summary>
        /// Builds query string parameters relevant to list items.
        /// </summary>
        /// <param name="cursor">The ID of the cursor, from a previous query.  Used for paging results.</param>
        /// <param name="parentValue">The parent list item value.</param>
        /// <param name="valueFilter">The list item value to filter on.</param>
        public static Dictionary<string, object> GetListItemsQueryParams(string cursor = null, string parentValue = null, string valueFilter = null)
        {
            var qp = StandardQueryParams.Get(callbacks: new Action<StandardQueryParams>[] { sqp => sqp.Cursor(cursor) });
            if (parentValue != null)
            {
                qp["parentValue"] = parentValue;
            }
            if (valueFilter != null)
            {
                qp["valueFilter"] = valueFilter;
            }
            return qp;
        }

        /// <inheritdoc cref="GetListItemsInternal" />
        public static async Task<FieldListItemCollection> GetListItems(
            string apiRelativeUrlPath, Dictionary<string, object> additionalQueryParams = null, NucleusOneApp app = null,
            string cursor = null, string valueFilter = null, string parentValue = null)
        {
            string responseBody = await GetListItemsInternal(apiRelativeUrlPath, additionalQueryParams, app, cursor, valueFilter, parentValue, false);
            var apiModel = NucleusOneDotNetSdk.ApiModel.FieldListItemCollection.FromJsonArray(
                arrayItemsJson: responseBody,
                instance: new NucleusOneDotNetSdk.ApiModel.FieldListItemCollection(),
                entityFromJsonCallback: (x) => NucleusOneDotNetSdk.ApiModel.FieldListItem.FromJson(x)
            );

            return FieldListItemCollection.FromApiModel(apiModel, app);
        }

        /// <summary>
        /// Gets list item values of a collection on the Nucleus One server, without the Id property populated.
        /// </summary>
        /// <inheritdoc cref="GetListItemsInternal" />
        public static async Task<FieldListItemCollection> GetAllListItemsNoIds(
            string apiRelativeUrlPath, Dictionary<string, object> additionalQueryParams = null, NucleusOneApp app = null,
            string valueFilter = null, string parentValue = null)
        {
            string listItemsAsCsvFlatFile = await GetListItemsInternal(apiRelativeUrlPath, additionalQueryParams, app, null, valueFilter, parentValue, true);

            var csvConfig = new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false
            };
            int columnCount;
            bool containsParentValue;

            // Get the number of columns
            using (var stringReader = new System.IO.StringReader(listItemsAsCsvFlatFile))
            using (var csv = new CsvHelper.CsvReader(stringReader, csvConfig))
            {
                csv.Read();

                var firstRecord = csv.GetRecord<dynamic>();
                columnCount = ((IDictionary<string, object>)firstRecord).Count;
                containsParentValue = (columnCount == 2);
            }

            if (columnCount == 0)
                return new FieldListItemCollection(Array.Empty<FieldListItem>(), app);

            using (var stringReader = new System.IO.StringReader(listItemsAsCsvFlatFile))
            using (var csv = new CsvHelper.CsvReader(stringReader, csvConfig))
            {
                var csvRecordType = containsParentValue
                    ? typeof(CsvRecordWithParentValue)
                    : typeof(CsvRecordWithoutParentValue);

                var records = csv.GetRecords(csvRecordType).ToArray();
                var results = new List<FieldListItem>(records.Length);

                foreach (CsvRecordWithoutParentValue record in records)
                {
                    var apiModel = new NucleusOneDotNetSdk.ApiModel.FieldListItem()
                    {
                        Value = record.Value,
                        ParentValue = containsParentValue ? ((CsvRecordWithParentValue)record).ParentValue : null
                    };
                    results.Add(FieldListItem.FromApiModel(apiModel));
                }

                return new FieldListItemCollection(results.ToArray(), app);
            }
        }

        /// <summary>
        /// Gets list item values of a collection on the Nucleus One server.
        /// </summary>
        /// <param name="apiRelativeUrlPath">The relative Nucleus One API path to use when call the API.</param>
        /// <param name="additionalQueryParams">Additional query string parameters.</param>
        /// <param name="app">The application to use when connecting to Nucleus One.</param>
        /// <param name="cursor">The ID of the cursor, from a previous query.  Used for paging results.</param>
        /// <param name="valueFilter">The value on which to filter.</param>
        /// <param name="parentValue">The parent field value.  Only valid if this field contains dependent field values.</param>
        /// <param name="getAllAsFlatFile">Whether to get all items in a single call as a flat file (e.g. a CSV file).</param>
        private static async Task<string> GetListItemsInternal(
            string apiRelativeUrlPath, Dictionary<string, object> additionalQueryParams = null, NucleusOneApp app = null,
            string cursor = null, string valueFilter = null, string parentValue = null, bool getAllAsFlatFile = false)
        {
            var qp = StandardQueryParams.Get(
                callbacks: new Action<StandardQueryParams>[] { sqp => sqp.Cursor(cursor) }
            );
            if (valueFilter != null)
                qp["valueFilter"] = valueFilter;
            if (parentValue != null)
                qp["parentValue"] = parentValue;
            if (getAllAsFlatFile)
                qp["getAllAsFlatFile"] = true;

            if (additionalQueryParams != null)
            {
                foreach (var param in additionalQueryParams)
                {
                    qp.Add(param.Key, param.Value);
                }
            }

            var responseBody = await Http.ExecuteGetRequestWithTextResponse(
                apiRelativeUrlPath: apiRelativeUrlPath,
                app: app,
                queryParams: qp
            );

            if (!getAllAsFlatFile)
                return responseBody;

            return responseBody;
        }

        /*
        public static async Task DownloadListItems(NucleusOneApp app, string apiRelativeUrlPath, string parentValue = null, string valueFilter = null, string destinationFilePath, string cursor = null)
        {
            var qp = GetListItemsQueryParams(cursor, parentValue, valueFilter);
            qp["getAllAsFlatFile"] = true;
            await Http.DownloadAuthenticated(apiRelativeUrlPath, destinationFilePath, app, queryParams: qp);
        }
        */

        /// <summary>
        /// Adds list items to a collection on the Nucleus One server.
        /// </summary>
        /// <param name="apiRelativeUrlPath">The relative Nucleus One API path to use when call the API.</param>
        /// <param name="items">The items to add.</param>
        /// <param name="additionalQueryParams">Additional query string parameters.</param>
        /// <param name="app">The application to use when connecting to Nucleus One.</param>
        public static async Task AddListItems(string apiRelativeUrlPath, FieldListItemCollection items, Dictionary<string, object> additionalQueryParams = null, NucleusOneApp app = null)
        {
            var qp = StandardQueryParams.Get();
            if (additionalQueryParams != null)
            {
                foreach (var param in additionalQueryParams)
                {
                    qp.Add(param.Key, param.Value);
                }
            }

            await Http.ExecutePostRequest(
                apiRelativeUrlPath: apiRelativeUrlPath,
                app: app,
                queryParams: qp,
                body: items.ToApiModel().ToJson()
            );
        }

        /// <summary>
        /// Sets or replaces list item values of a collection on the Nucleus One server.
        /// </summary>
        /// <param name="apiRelativeUrlPath">The relative Nucleus One API path to use when call the API.</param>
        /// <param name="values">The values to set.</param>
        /// <param name="additionalQueryParams">Additional query string parameters.</param>
        /// <param name="app">The application to use when connecting to Nucleus One.</param>
        public static async Task SetListItems(string apiRelativeUrlPath, List<string> values, Dictionary<string, object> additionalQueryParams = null, NucleusOneApp app = null)
        {
            var qp = StandardQueryParams.Get();
            qp["type"] = "file";
            if (additionalQueryParams != null)
            {
                foreach (var param in additionalQueryParams)
                {
                    qp.Add(param.Key, param.Value);
                }
            }

            await Http.ExecutePostRequest(
                apiRelativeUrlPath: apiRelativeUrlPath,
                app: app,
                queryParams: qp,
                body: string.Join("\n", values) + "\n");
        }

        private class CsvRecordWithoutParentValue
        {
            [CsvHelper.Configuration.Attributes.Index(0)]
            public virtual string Value { get; set; }
        }

        private class CsvRecordWithParentValue: CsvRecordWithoutParentValue
        {
            [CsvHelper.Configuration.Attributes.Index(0)]
            public string ParentValue { get; set; }
            [CsvHelper.Configuration.Attributes.Index(1)]
            public override string Value
            {
                get => base.Value;
                set => base.Value = value;
            }
        }
    }
}
