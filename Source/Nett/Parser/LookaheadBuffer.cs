using System;

namespace Nett.Parser
{
    internal sealed class LookaheadBuffer<T> where T : struct
    {

        private readonly T[] buffer;
        private readonly Func<T?> read;

        private int readIndex = 0;
        private int writeIndex = -1;

        public LookaheadBuffer(Func<T?> read, int lookAhead)
        {
            buffer = new T[lookAhead];
            this.read = read;

            for (int i = 0; i < this.buffer.Length; i++)
            {
                this.Read();
            }
        }



        public T La(int la)
        {
            if (la > this.buffer.Length - 1)
            {
                throw new ArgumentOutOfRangeException($"Argument {nameof(la)} with value '{la}' is out of the valid range '[0 - {this.buffer.Length - 1}']");
            }

            var index = (this.readIndex + la) % this.buffer.Length;
            return this.buffer[index];
        }

        public bool LaIs(int la, T expected)
        {
            var laVal = this.La(la);
            return object.Equals(laVal, expected);
        }

        public bool HasNext()
        {
            return readIndex != this.writeIndex;
        }

        public void Next()
        {
            this.IncIndex(ref this.readIndex);
            this.Read();
        }

        public T Consume()
        {
            T ret = this.buffer[this.readIndex];
            this.Next();
            return ret;
        }

        private void Read()
        {
            T? val = this.read();
            if (val.HasValue)
            {
                this.IncIndex(ref writeIndex);
                this.buffer[writeIndex] = val.Value;
            }
        }

        private void IncIndex(ref int index)
        {
            if (++index > this.buffer.Length - 1)
            {
                index = 0;
            }
        }
    }
}
