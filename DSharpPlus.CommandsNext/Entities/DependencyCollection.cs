using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace DSharpPlus.CommandsNext
{
    /// <summary>
    /// Represents a collection of dependencies for CommandsNext.
    /// </summary>
    public sealed class DependencyCollection : IEnumerable<object>
    {
        private List<DependencyInfo> Dependencies { get; }

        internal DependencyCollection(List<DependencyInfo> deps)
        {
            Dependencies = new List<DependencyInfo>(deps);
        }

        /// <summary>
        /// Retrieves a specified dependency from this <see cref="DependencyCollection"/>.
        /// </summary>
        /// <typeparam name="T">Type of dependency to retrieve.</typeparam>
        /// <returns>Specified dependency.</returns>
        public T GetDependency<T>() where T : class
        {
            var deps = Dependencies.Where(xdi => xdi.Instance is T);
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
        /// Retrieves a specified dependency from this <see cref="DependencyCollection"/>.
        /// </summary>
        /// <param name="t">Type of dependency to retrieve.</param>
        /// <returns>Specified dependency</returns>
        public object GetDependency(Type t)
        {
            var deps = Dependencies.Where(xdi => t.GetTypeInfo().IsAssignableFrom(xdi.ImplementationType.GetTypeInfo()));
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
        /// Gets an enumerator for this <see cref="DependencyCollection"/>.
        /// </summary>
        /// <returns>Enumerator of dependencies.</returns>
        public IEnumerator<object> GetEnumerator() =>
            Dependencies.Select(xdep => xdep.Instance).GetEnumerator();

        /// <summary>
        /// Gets an enumerator for this <see cref="DependencyCollection"/>.
        /// </summary>
        /// <returns>Enumerator of dependencies.</returns>
        IEnumerator IEnumerable.GetEnumerator() =>
            Dependencies.Select(xdep => xdep.Instance).GetEnumerator();
    }
}
