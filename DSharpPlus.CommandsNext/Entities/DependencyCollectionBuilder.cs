using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace DSharpPlus.CommandsNext
{
    /// <summary>
    /// Represents an interface for building dependency collections for use with CommandsNext.
    /// </summary>
    public sealed class DependencyCollectionBuilder : IDisposable
    {
        private HashSet<Type> RegisteredTypes { get; }
        private List<DependencyInfo> RegisteredDependencies { get; }

        /// <summary>
        /// Creates a new DependencyCollectionBuilder.
        /// </summary>
        public DependencyCollectionBuilder()
        {
            RegisteredTypes = new HashSet<Type>();
            RegisteredDependencies = new List<DependencyInfo>();
        }

        /// <summary>
        /// Retrieves a specified dependency from this <see cref="DependencyCollectionBuilder"/>.
        /// </summary>
        /// <typeparam name="T">Type of dependency to retrieve.</typeparam>
        /// <returns>Specified dependency.</returns>
        public T GetDependency<T>() where T : class
        {
            var deps = RegisteredDependencies.Where(xdi => xdi.Instance is T);
            var dependencyInfos = deps as DependencyInfo[] ?? deps.ToArray();
            var dc = dependencyInfos.Count();

            if (dc < 1)
            {
                throw new ArgumentException("No dependencies with specified type were found.", nameof(T));
            }
            if (dc > 1)
            {
                throw new ArgumentException("Multiple dependencies with specified type were found.", nameof(T));
            }

            return dependencyInfos.Single().Instance as T;
        }

        /// <summary>
        /// Retrieves a specified dependency from this <see cref="DependencyCollectionBuilder"/>.
        /// </summary>
        /// <param name="t">Type of dependency to retrieve.</param>
        /// <returns>Specified dependency</returns>
        public object GetDependency(Type t)
        {
            var deps = RegisteredDependencies.Where(xdi => t.GetTypeInfo().IsAssignableFrom(xdi.ImplementationType.GetTypeInfo()));
            var dependencyInfos = deps as DependencyInfo[] ?? deps.ToArray();
            var dc = dependencyInfos.Count();

            if (dc < 1)
            {
                throw new InvalidOperationException("No dependencies matched the criteria.");
            }
            if (dc > 1)
            {
                throw new InvalidOperationException("More than one dependency matched the criteria.");
            }

            return dependencyInfos.Single().Instance;
        }

        /// <summary>
        /// Creates a new <see cref="DependencyCollection"/> using information contained in this builder.
        /// </summary>
        /// <returns>Built <see cref="DependencyCollection"/>.</returns>
        public DependencyCollection Build() =>
            new DependencyCollection(RegisteredDependencies);

        /// <summary>
        /// Adds an instance of a dependency to the dependency collection.
        /// </summary>
        /// <typeparam name="T">Type of dependency to register.</typeparam>
        /// <param name="instance">Instance to register.</param>
        /// <returns>This <see cref="DependencyCollectionBuilder"/>.</returns>
        public DependencyCollectionBuilder AddInstance<T>(T instance)
            where T : class 
            => AddInstance<T, T>(instance);

        /// <summary>
        /// Adds an instance of a dependency to the dependency collection.
        /// </summary>
        /// <typeparam name="TDep">Type of dependency to register.</typeparam>
        /// <typeparam name="TImpl">Type of implementation to register.</typeparam>
        /// <param name="instance">Instance to register.</param>
        /// <returns>This <see cref="DependencyCollectionBuilder"/>.</returns>
        public DependencyCollectionBuilder AddInstance<TDep, TImpl>(TImpl instance) 
            where TDep : class
            where TImpl : class, TDep
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance), "Dependency cannot be null.");
            }

            var tdep = typeof(TDep);
            var timpl = typeof(TImpl);

            RegisteredTypes.Add(tdep);
            if (tdep != timpl)
            {
                RegisteredTypes.Add(timpl);
            }

            var depinf = new DependencyInfo
            {
                DependencyType = tdep,
                ImplementationType = timpl,
                Instance = instance
            };
            RegisteredDependencies.Add(depinf);

            return this;
        }

        /// <summary>
        /// Constructs and adds a dependency by type.
        /// </summary>
        /// <typeparam name="T">Type of dependency.</typeparam>
        /// <returns>This <see cref="DependencyCollectionBuilder"/>.</returns>
        public DependencyCollectionBuilder Add<T>()
            where T : class
            => Add<T, T>();

        /// <summary>
        /// Constructs and adds a dependency by type.
        /// </summary>
        /// <typeparam name="TDep">Type of dependency.</typeparam>
        /// <typeparam name="TImpl">Type of implementation.</typeparam>
        /// <returns>This <see cref="DependencyCollectionBuilder"/>.</returns>
        public DependencyCollectionBuilder Add<TDep, TImpl>()
            where TDep : class
            where TImpl : class, TDep
        {
            var tdep = typeof(TDep);
            var timpl = typeof(TImpl);

            var ti = timpl.GetTypeInfo();
            var cs = ti.DeclaredConstructors
                .Where(xci => xci.IsPublic)
                .ToArray();

            if (cs.Length != 1)
            {
                throw new ArgumentException("Specified type has more than 1 constructor.", nameof(TImpl));
            }

            var cx = cs[0];
            var ts = cx.GetParameters()
                .Select(xpi => xpi.ParameterType)
                .ToArray();
            var cdeps = new object[ts.Length];
            for (var i = 0; i < ts.Length; i++)
            {
                cdeps[i] = GetDependency(ts[i]);
            }

            var instance = Activator.CreateInstance(timpl, cdeps);

            RegisteredTypes.Add(tdep);
            if (tdep != timpl)
            {
                RegisteredTypes.Add(timpl);
            }

            var depinf = new DependencyInfo
            {
                DependencyType = tdep,
                ImplementationType = timpl,
                Instance = instance
            };
            RegisteredDependencies.Add(depinf);

            return this;
        }

        /// <summary>
        /// Disposes this <see cref="DependencyCollectionBuilder"/>.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
