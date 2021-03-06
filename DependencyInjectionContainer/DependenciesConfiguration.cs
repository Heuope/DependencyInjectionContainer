﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DependencyInjectionContainer
{
    public class DependenciesConfiguration
    {
        public enum Lifetime { Instance, Singleton }

        internal Dictionary<Type, List<ImplConfig>> Config = new Dictionary<Type, List<ImplConfig>>();

        public void Register(Type tDependency, Type tImplementation, Lifetime lifetime = Lifetime.Instance)
        {
            if (tImplementation.IsAbstract)
                throw new ArgumentException("TImplementation cannot be abstract");
            if (!tDependency.IsAssignableFrom(tImplementation) && !tDependency.IsGenericTypeDefinition)
                throw new ArgumentException("TImplementation doesn't implemented TDependency interface");
            if (!tImplementation.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Any())
                throw new ArgumentException("TImplementation doesn't have any public constructors"); ;
            if (!Config.ContainsKey(tDependency))
            {
                Config.Add(tDependency, new List<ImplConfig>());
            }

            if (Config[tDependency].Contains(new ImplConfig(tImplementation, lifetime)))
                throw new ArgumentException("Such dependency is already registred");
            else
                Config[tDependency].Add(new ImplConfig(tImplementation, lifetime));
        }

        public void Register<TDependency, TImplementation>(Lifetime lifetime = Lifetime.Instance) where TDependency : class where TImplementation : TDependency
        {
            Register(typeof(TDependency), typeof(TImplementation), lifetime);
        }
    }
}
