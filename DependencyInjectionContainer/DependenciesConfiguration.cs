using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DependencyInjectionContainer
{
    public class DependenciesConfiguration
    {
        public enum Lifetime { Instance, Singleton }

        internal Dictionary<Type, List<Type>> RegisterCofig = new Dictionary<Type, List<Type>>();

        private void Register(Type tDependency, Type tImplementation, Lifetime lifetime)
        {
            if (tImplementation.IsAbstract)
                return;
            if (tDependency.IsAssignableFrom(tImplementation))
                return;
            if (!tImplementation.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Any())
                return;
            if (!RegisterCofig.ContainsKey(tDependency))
            {
                RegisterCofig.Add(tDependency, new List<Type>());
            }

            if (RegisterCofig[tDependency].Contains(tImplementation))
                return;
            else
                RegisterCofig[tDependency].Add(tImplementation);
        }

        public void Register<TDependency, TImplementation>(Lifetime lifetime = Lifetime.Instance) where TDependency : class where TImplementation : TDependency
        {
            Register(typeof(TDependency), typeof(TImplementation), lifetime);
        }
    }
}
