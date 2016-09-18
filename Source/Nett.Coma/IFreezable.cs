using System;

namespace Nett.Coma
{
    internal interface IFreezable
    {
        void Freeze();
    }

    internal abstract class Freezable : IFreezable
    {
        protected volatile bool isFrozen = false;

        public void Freeze() => this.isFrozen = true;

        protected void CheckNotFrozen()
        {
            if (this.isFrozen)
            {
                throw new InvalidOperationException($"Attempted to modify a frozen instance of type '{this.GetType().Name}'");
            }
        }
    }
}
