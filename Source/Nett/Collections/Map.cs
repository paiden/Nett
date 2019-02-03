using System.Collections.Generic;

namespace Nett.Collections
{
    internal sealed class Map<TX, TY>
    {
        public Dictionary<TX, TY> Forward { get; } = new Dictionary<TX, TY>();

        public Dictionary<TY, TX> Reverse { get; } = new Dictionary<TY, TX>();

        public void Add(TX x, TY y)
        {
            this.Forward.Add(x, y);
            this.Reverse.Add(y, x);
        }
    }
}
