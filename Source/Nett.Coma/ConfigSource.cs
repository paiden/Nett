using System;
using Nett.Extensions;

namespace Nett.Coma
{
    public interface IConfigSource
    {
        string Name { get; }
    }

    internal sealed class ConfigSource : IConfigSource, IEquatable<IConfigSource>
    {
        public ConfigSource(string name)
        {
            this.Name = name.CheckNotNull(nameof(name));
        }

        public string Name { get; }

        public override bool Equals(object obj) => this.Equals(obj as ConfigSource);

        public bool Equals(IConfigSource other) => other != null && other.Name == this.Name;

        public override int GetHashCode() => this.Name.GetHashCode();
    }
}
