using Microsoft.VisualStudio.TestTools.UnitTesting;
using DependencyInjectionContainer;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace DependencyInjectionContainer.Test
{
    public interface ISimpleDep { }

    public class Simple : ISimpleDep
    {
        int a;

        public Simple()
        {
            a = 2;
        }

        public override bool Equals(object obj)
        {
            return obj is Simple simple &&
                   a == simple.a;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(a);
        }
    }

    public class OtherSimple : ISimpleDep
    {
        char c;

        public OtherSimple()
        {
            c = 'a';
        }

        public override bool Equals(object obj)
        {
            return obj is OtherSimple simple &&
                   c == simple.c;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(c);
        }
    }

    public class SimpleInstance : ISimpleDep
    {
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }

    public class ClassWithInnerDep : ISimpleDep
    {
        IInner _inner;

        public ClassWithInnerDep(IInner inner)
        {
            _inner = inner;
        }

        public override bool Equals(object obj)
        {
            return obj is ClassWithInnerDep dep &&
                   EqualityComparer<IInner>.Default.Equals(_inner, dep._inner);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_inner);
        }
    }

    public interface IInner { }

    public class InnerClass : IInner
    {
        public long c = 2;

        public override bool Equals(object obj)
        {
            return obj is InnerClass @class &&
                   c == @class.c;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(c);
        }
    }

    public interface ISingleton 
    {
        public void ChangeNum();
    }

    public class Singleton : ISingleton
    {
        public int e { get; private set; } = 1;

        public void ChangeNum()
        {
            e = 4;
        }

        public override bool Equals(object obj)
        {
            return obj is Singleton singleton &&
                   e == singleton.e;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(e);
        }
    }

    public interface ICollectionClass { }

    public class CollectionClass1 : ICollectionClass
    {
        public int a = 2;

        public override bool Equals(object obj)
        {
            return obj is CollectionClass1 @class &&
                   a == @class.a;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(a);
        }
    }

    public class CollectionClass2 : ICollectionClass
    {
        public char e = '2';

        public override bool Equals(object obj)
        {
            return obj is CollectionClass2 @class &&
                   e == @class.e;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(e);
        }
    }

    public class CollectionClass3 : ICollectionClass
    {
        public double d = 2;

        public override bool Equals(object obj)
        {
            return obj is CollectionClass3 @class &&
                   d == @class.d;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(d);
        }
    }

    public class CollectionClass4 : ICollectionClass
    {
        public long a { get; } = 4;

        public override bool Equals(object obj)
        {
            return obj is CollectionClass4 @class &&
                   a == @class.a;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(a);
        }
    }

    public interface IOpen { }

    public class OpenConstrainedClass : IOpen
    {
        int a = 5;

        public override bool Equals(object obj)
        {
            return obj is OpenConstrainedClass @class &&
                   a == @class.a;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(a);
        }
    }

    public interface IOpenGeneric<TOpenClass> where TOpenClass : IOpen { }

    class OpenGenericConstrainedClass<TOpenClass> : IOpenGeneric<TOpenClass> where TOpenClass : IOpen
    {
        TOpenClass cls;
        public OpenGenericConstrainedClass(TOpenClass @class)
        {
            cls = @class;
        }

        public override bool Equals(object obj)
        {
            return obj is OpenGenericConstrainedClass<TOpenClass> @class &&
                   EqualityComparer<TOpenClass>.Default.Equals(cls, @class.cls);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(cls);
        }
    }

    [TestClass]
    public class UnitTest1
    {
        DependenciesConfiguration configuration;

        DependencyProvider provider;

        [TestInitialize]
        public void Setup()
        {
            configuration = new DependenciesConfiguration();

            configuration.Register<ISimpleDep, Simple>();
            configuration.Register<ISimpleDep, OtherSimple>();
            configuration.Register<ISimpleDep, ClassWithInnerDep>();
            configuration.Register<ISimpleDep, SimpleInstance>();
            configuration.Register<ISingleton, Singleton>();
            configuration.Register<IInner, InnerClass>();
            configuration.Register<ICollectionClass, CollectionClass1>();
            configuration.Register<ICollectionClass, CollectionClass2>();
            configuration.Register<ICollectionClass, CollectionClass3>();
            configuration.Register<ICollectionClass, CollectionClass4>();
            configuration.Register<IOpen, OpenConstrainedClass>();
            configuration.Register(typeof(IOpenGeneric<>), typeof(OpenGenericConstrainedClass<>));

            provider = new DependencyProvider(configuration);
        }

        [TestMethod]
        public void TestSimpleDependency()
        {
            var actual = provider.Resolve<ISimpleDep>();

            var expected = new Simple();

            Assert.AreEqual(actual, expected);
        }

        [TestMethod]
        public void TestInnerDependency()
        {
            var actual = provider.Resolve<ISimpleDep>(2);

            var expected = new ClassWithInnerDep(new InnerClass());

            Assert.AreEqual(actual, expected);
        }

        [TestMethod]
        public void TestSelectImplementation()
        {
            var actual = provider.Resolve<ISimpleDep>(1);

            var expected = new OtherSimple();

            Assert.AreEqual(actual, expected);
        }

        [TestMethod]
        public void TestInstance()
        {
            var actual = provider.Resolve<ISimpleDep>(3);

            var expected = provider.Resolve<ISimpleDep>(3);

            Assert.AreNotEqual(actual, expected);
        }

        [TestMethod]
        public void TestSingleton()
        {
            var actual = Task.Run(() => provider.Resolve<ISingleton>());

            var expected1 = Task.Run(() => provider.Resolve<ISingleton>());
            var expected2 = Task.Run(() => provider.Resolve<ISingleton>());
            var expected3 = Task.Run(() => provider.Resolve<ISingleton>());

            Assert.AreEqual(actual.Result, expected1.Result);
            Assert.AreEqual(actual.Result, expected2.Result);
            Assert.AreEqual(actual.Result, expected3.Result);
        }

        [TestMethod]
        public void TestCollection()
        {
            var actual = provider.Resolve<IEnumerable<ICollectionClass>>();

            var expected = new ICollectionClass[] { new CollectionClass1(), new CollectionClass2(), new CollectionClass3(), new CollectionClass4() };

            CollectionAssert.AreEqual((System.Collections.ICollection)actual, expected);
        }

        [TestMethod]
        public void OpenConstrainedClassTest()
        {
            var actual = provider.Resolve<IOpenGeneric<IOpen>>();
            var expected = new OpenGenericConstrainedClass<IOpen>(new OpenConstrainedClass());

            Assert.AreEqual(expected, actual);
        }
    }
}
