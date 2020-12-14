using System;
using System.Collections.Generic;
using System.Text;

namespace DependencyInjectionContainer
{
    class ImplConfig
    {
        public DependenciesConfiguration.Lifetime Lifetime { get; }

        public Type implType { get; } 

        public ImplConfig(Type type, DependenciesConfiguration.Lifetime lifetime)
        {
            Lifetime = lifetime;
            implType = type;
        }
    }
}
