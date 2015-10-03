using System;
using System.Diagnostics;

namespace Nett.Parser
{
    [DebuggerDisplay("{DebuggerDisplay}")]
    internal sealed class LookaheadBuffer<T> where T : struct
    {

        private readonly T[] buffer;
        private readonly Func<T?> read;

        private int readIndex = 0;
        private int writeIndex = -1;

        public int ItemsAvailable { get; private set; }

        public LookaheadBuffer(Func<T?> read, int lookAhead)
        {
            buffer = new T[lookAhead];
            this.read = read;

            for (int i = 0; i < this.buffer.Length; i++)
            {
                this.Read();
            }
        }

        public T Peek()
        {
            return this.PeekAt(0);
        }

        public T PeekAt(int la)
        {
            if (la > this.buffer.Length - 1)
            {
                throw new ArgumentOutOfRangeException($"Argument {nameof(la)} with value '{la}' is out of the valid range '[0 - {this.buffer.Length - 1}']");
            }

            var index = (this.readIndex + la) % this.buffer.Length;
            return this.buffer[index];
        }

        public bool ExpectAt(int la, T expected)
        {
            var laVal = this.PeekAt(la);
            return object.Equals(laVal, expected);
        }

        public bool TryExpect(T expected)
        {
            return !this.End && object.Equals(this.Peek(), expected);
        }

        public bool HasNext()
        {
            return readIndex != this.writeIndex;
        }

        public bool End => readIndex == -1 && this.writeIndex == -1;

        public T Consume()
        {
            T ret = this.buffer[this.readIndex];

            if (this.readIndex == this.writeIndex) // We are at the end of the stream there isn't any more data
            {
                this.readIndex = this.writeIndex = -1;
            }
            else
            {
                this.IncIndex(ref this.readIndex);
                this.ItemsAvailable--;
                this.Read();
            }

            return ret;
        }

        private void Read()
        {
            T? val = this.read();
            if (val.HasValue)
            {
                this.IncIndex(ref writeIndex);
                this.buffer[writeIndex] = val.Value;
                this.ItemsAvailable++;
            }
        }

        private void IncIndex(ref int index)
        {
            if (++index > this.buffer.Length - 1)
            {
                index = 0;
            }
        }

        private T[] DebuggerDisplay => buffer.SubArray(this.readIndex, this.buffer.Length - this.readIndex);
    }
}
