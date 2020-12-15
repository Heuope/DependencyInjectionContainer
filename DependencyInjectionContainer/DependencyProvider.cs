using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Concurrent;
using System.Collections;

namespace DependencyInjectionContainer
{
    public class DependencyProvider
    {
        private Dictionary<Type, List<ImplConfig>> _config { get; }

        private ConcurrentDictionary<Type, object> _instances { get; } = new ConcurrentDictionary<Type, object>();

        public DependencyProvider(DependenciesConfiguration depConfig)
        {
            _config = depConfig.Config;
        }

        private object Resolve(Type tDependency, int implNumber = 0)
        {
            if (typeof(IEnumerable).IsAssignableFrom(tDependency))
            {
                var actual = tDependency.GetGenericArguments().First();
                int implCount = _config[actual].Count;

                var container = Array.CreateInstance(actual, implCount);

                for (int i = 0; i < implCount; i++)
                {
                    container.SetValue(Resolve(actual, i), i);
                }
                return container;
            }

            var isGenericDependency = tDependency.GetGenericArguments().Length == 0 ? false : true;
            var isOpenGenericDependency = false;

            ImplConfig implConfig;

            if (isGenericDependency)
            {
                var t = tDependency.GetGenericTypeDefinition();
                if (_config.ContainsKey(t))
                {
                    implConfig = _config[t].First();
                    isOpenGenericDependency = true;
                }
                else
                {
                    implConfig = _config[tDependency].First();
                }
            }
            else
            {
                implConfig = _config[tDependency][implNumber];
            }

            var targetType = implConfig.implType;
            if (isOpenGenericDependency)
            {
                targetType = targetType.MakeGenericType(tDependency.GetGenericArguments().First());
            }

            var constructor = targetType.GetConstructors(BindingFlags.Public | BindingFlags.Instance).First();
            var parametrs = constructor.GetParameters();

            var constrParams = new List<object>();

            foreach (var parameter in parametrs)
            {
                if (parameter.ParameterType.IsValueType)
                {
                    constrParams.Add(Activator.CreateInstance(parameter.ParameterType));
                }
                else
                {
                    constrParams.Add(Resolve(parameter.ParameterType));
                }
            }

            try
            {
                object result = constructor.Invoke(constrParams.ToArray());

                if (implConfig.Lifetime == DependenciesConfiguration.Lifetime.Singleton)
                {
                    return _instances.TryAdd(targetType, result) ? result : _instances[targetType];
                }
                return result;
            }
            catch (Exception)
            {
                throw new ArgumentException($"{targetType.Name} constructor threw an exception");
            }
        }

        public TDependency Resolve<TDependency>(int implNumber = 0) 
        {
            return (TDependency)Resolve(typeof(TDependency), implNumber);
        }
    }
}
