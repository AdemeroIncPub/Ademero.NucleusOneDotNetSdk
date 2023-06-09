using System;
using System.Collections.Concurrent;
using System.Threading;

namespace NucleusOneDotNetSdk
{
    /// <summary>
    /// Provides the ability to provide variables in an execution scope.  Imitates the functionality
    /// provided by the <see href="https://pub.dev/packages/get_it">get_it</see> package in Dart.
    /// </summary>
    internal static class GetIt
    {
        private static AsyncLocal<ConcurrentStack<ConcurrentDictionary<Type, object>>> _scopes;

        private static AsyncLocal<ConcurrentStack<ConcurrentDictionary<Type, object>>> Scopes
        {
            get
            {
                if (_scopes == null)
                    _scopes = new AsyncLocal<ConcurrentStack<ConcurrentDictionary<Type, object>>>();
                
                if (_scopes.Value == null)
                {
                    var scopesValue = _scopes.Value = new ConcurrentStack<ConcurrentDictionary<Type, object>>();
                    scopesValue.Push(new ConcurrentDictionary<Type, object>());
                }

                return _scopes;
            }
        }

        public static void PushNewScope()
        {
            Scopes.Value.Push(new ConcurrentDictionary<Type, object>());
        }

        public static T Get<T>()
        {
            if (Scopes.Value.TryPeek(out var currentScope))
            {
                if (currentScope.TryGetValue(typeof(T), out var value))
                {
                    return value is T t ? t : default;
                }
            }
            return default;
        }

        public static void RegisterSingleton<TSingleton>(TSingleton value) where TSingleton : class
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (Scopes.Value.TryPeek(out var currentScope))
            {
                currentScope[typeof(TSingleton)] = value;
            }
        }

        public static void PopScope()
        {
            Scopes.Value.TryPop(out _);
        }
    }
}
