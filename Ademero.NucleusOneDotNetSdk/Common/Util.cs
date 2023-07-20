using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ademero.NucleusOneDotNetSdk.Common
{
    public static class Util
    {
        /// <summary>
        /// Defines a <see cref="NucleusOneApp"/> instance in a local scope, such that it may be retrieved using the
        /// following code.
        /// </summary>
        /// <code>
        /// var app = GetIt.Get&lt;NucleusOneApp&gt;();
        /// </code>
        public static T DefineN1AppInScope<T>(NucleusOneApp app, Func<T> action)
        {
            // This will block until DefineObjectInScopeInternal completes
            return Task.Run(() => DefineObjectInScopeInternal(app, () => Task.FromResult(action())))
                .GetAwaiter()
                .GetResult();
        }

        /// <inheritdoc cref="DefineN1AppInScope{T}(NucleusOneApp, Func{T})" />
        public async static Task DefineN1AppInScopeAsync(NucleusOneApp app, Func<Task> action)
        {
            var actionWrapper = new Func<Task<object>>(
                async () =>
                {
                    await action();
                    return null;
                }
            );
            await DefineObjectInScopeInternal(app, actionWrapper);
        }

        /// <inheritdoc cref="DefineN1AppInScope{T}(NucleusOneApp, Func{T})" />
        public async static Task<T> DefineN1AppInScopeAsync<T>(NucleusOneApp app, Func<Task<T>> action)
        {
            return await DefineObjectInScopeInternal(app, action);
        }

        /// <summary>Defines a <see cref="TSingleton"/> instance in a local scope, such that it may be retrieved using the
        /// following code.
        /// </summary>
        /// <code>
        /// var app = GetIt.Get&lt;TSingleton&gt;();
        /// </code>
        private async static Task<TRet> DefineObjectInScopeInternal<TSingleton, TRet>(
          TSingleton value,
          Func<Task<TRet>> action
        ) where TSingleton : class
        {
            bool scopeCreated = false;
            try
            {
                GetIt.PushNewScope();
                scopeCreated = true;
                GetIt.RegisterSingleton(value);
                return await action();
            }
            finally
            {
                if (scopeCreated)
                {
                    GetIt.PopScope();
                }
            }
        }

        private static JsonSerializerSettings GetDefaultSettings()
        {
            var settings = new JsonSerializerSettings()
            {
                // This should also be set on each ApiModel class
                NullValueHandling = NullValueHandling.Ignore
            };
            return settings;
        }

        public static object JsonDeserializeObject(string value)
        {
            return JsonConvert.DeserializeObject(value, GetDefaultSettings());
        }

        public static T JsonDeserializeObject<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value, GetDefaultSettings());
        }

        public static string JsonSerializeObject(object value)
        {
            return JsonSerializeObject(value, GetDefaultSettings());
        }

        public static string JsonSerializeObject(object value, JsonSerializerSettings settings)
        {
            return JsonConvert.SerializeObject(value, settings);
        }

        internal static void SetDictionaryValuesIfNotNull<T1, T2>(Dictionary<T1, T2> dict, IEnumerable<KeyValuePair<T1, T2>> keysAndValues)
        {
            foreach (var keyAndValue in keysAndValues)
                SetDictionaryValueIfNotNull(dict, keyAndValue.Key, keyAndValue.Value);
        }

        internal static void SetDictionaryValueIfNotNull<T1, T2>(Dictionary<T1, T2> dict, T1 key, T2 value)
        {
            if (value != null)
                dict[key] = value;
        }
    }
}