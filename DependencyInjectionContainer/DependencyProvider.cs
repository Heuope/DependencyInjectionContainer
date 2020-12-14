using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Concurrent;
using System.Collections;

namespace DependencyInjectionContainer
{
    class DependencyProvider
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
                int implCount = _config[tDependency].Count;

                var container = Array.CreateInstance(actual, implCount);

                for (int i = 0; i < implCount; i++)
                {
                    container.SetValue(Resolve(actual, i), i);
                }
                return container;
            }

            var constructor = tDependency.GetConstructors(BindingFlags.Public | BindingFlags.Instance).First();
            var parametrs = constructor.GetParameters();

            var constrParams = new List<object>();

            var impConfig = _config[tDependency];

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

                return result;
            }
            catch (Exception)
            {
                throw new ArgumentException("");
            }
        }

        public TDependency Resolve<TDependency>() 
        {
            return (TDependency)Resolve(typeof(TDependency));
        }
    }
}
