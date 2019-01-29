using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nett.Extensions;

namespace Nett
{
    public static partial class TomlObjectFactory
    {
        internal interface IResult { }

        /// <summary>
        /// Result object of a factory operation. Foundation for the fluent API that allows to construct
        /// complex object Graphs via <see cref="TomlObjectFactory"/> methods.
        /// </summary>
        /// <typeparam name="T">The type of TOML object that was created/updated</typeparam>
        public sealed class Result<T> : IResult
            where T : TomlObject
        {
            private readonly TomlTable owner;
            private readonly T added;

            internal Result(TomlTable owner, T added)
            {
                this.owner = owner;
                this.added = added;
            }

            /// <summary>
            /// Gets the table owning the added/updated TOML object.
            /// </summary>
            public TomlTable And
                => this.owner;

            /// <summary>
            /// Gets the table owning the added/updated TOML object.
            /// </summary>
            public TomlTable Owner
                => this.owner;

            /// <summary>
            /// Gets the object that was added/updated.
            /// </summary>
            public T Added
                => this.added;

            /// <summary>
            /// Allows further modification of the added/updated object via a user defined <see cref="Action{T}"/>.
            /// </summary>
            /// <param name="configure">Action configuration action</param>
            /// <returns>Gets <b>this</b> - fluent API continuation.</returns>
            /// <exception cref="ArgumentNullException">Throw if <paramref name="configure"/> is <b>null</b></exception>
            public Result<T> ConfigureAdded(Action<T> configure)
            {
                configure.CheckNotNull(nameof(configure));
                configure(this.added);
                return this;
            }
        }
    }
}
