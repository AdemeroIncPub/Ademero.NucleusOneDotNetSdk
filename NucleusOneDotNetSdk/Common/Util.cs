using Newtonsoft.Json;
using System;

namespace NucleusOneDotNetSdk.Common
{
    internal static class Util
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
            return DefineObjectInScopeInternal(app, action);
        }

        /// <summary>Defines a <see cref="TSingleton"/> instance in a local scope, such that it may be retrieved using the
        /// following code.
        /// </summary>
        /// <code>
        /// var app = GetIt.Get&lt;TSingleton&gt;();
        /// </code>
        private static TRet DefineObjectInScopeInternal<TSingleton, TRet>(
          TSingleton value,
          Func<TRet> action
        ) where TSingleton : class
        {
            bool scopeCreated = false;
            try
            {
                GetIt.PushNewScope();
                scopeCreated = true;
                GetIt.RegisterSingleton(value);
                return action();
            }
            finally
            {
                if (scopeCreated)
                {
                    GetIt.PopScope();
                }
            }
        }

        public static string SerializeObject(object value)
        {
            var settings = new JsonSerializerSettings()
            {
                // This should also be set on each ApiModel class
                NullValueHandling = NullValueHandling.Ignore
            };
            return JsonConvert.SerializeObject(value, settings);
        }
    }
}