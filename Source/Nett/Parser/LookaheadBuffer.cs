namespace Nett.Parser
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay("{DebuggerDisplay}")]
    internal abstract class LookaheadBuffer<T>
        where T : struct
    {
        private readonly Func<T?> read;
        private T[] buffer;

        private int readIndex = 0;
        private int writeIndex = -1;

        public LookaheadBuffer(Func<T?> read, int lookAhead)
        {
            this.buffer = new T[lookAhead];
            this.read = read;

            bool couldRead = true;
            for (int i = 0; i < this.buffer.Length && couldRead; i++)
            {
                couldRead = this.Read();
            }
        }

        public virtual bool End => this.EndInternal;

        protected bool EndInternal => this.ItemsAvailable <= 0;

        protected int ItemsAvailable { get; private set; }

        private T[] DebuggerDisplay => this.buffer.SubArray(this.readIndex, this.buffer.Length - this.readIndex);

        public virtual T Consume()
        {
            T ret = this.buffer[this.readIndex];

            if (this.readIndex == this.writeIndex)
            {
                // We are at the end of the stream there isn't any more data
                this.readIndex = this.writeIndex = -1;
                this.ItemsAvailable = 0;
            }
            else
            {
                this.IncIndex(ref this.readIndex);
                this.ItemsAvailable--;
                this.Read();
            }

            return ret;
        }

        public bool HasNext()
        {
            return this.readIndex != this.writeIndex;
        }

        public T Peek()
        {
            return this.EndInternal ? default(T) : this.PeekAt(0);
        }

        public T PeekAt(int la)
        {
            if (la > this.ItemsAvailable - 1)
            {
                if (la > this.buffer.Length - 1)
                {
                    this.GrowBuffer(la + 1);
                }

                if (la > this.ItemsAvailable - 1)
                {
                    throw new ArgumentOutOfRangeException($"Argument {nameof(la)} with value '{la}' is out of the valid range '[0 - {this.ItemsAvailable - 1}']");
                }
            }

            return this.PeekAtInternal(la);
        }

        public bool TryExpect(T expected)
        {
            return !this.EndInternal && object.Equals(this.Peek(), expected);
        }

        public bool TryExpectAt(int la, T expected)
        {
            if (this.ItemsAvailable < la + 1) { return false; }

            var laVal = this.PeekAt(la);
            return object.Equals(laVal, expected);
        }

        private void GrowBuffer(int minLength)
        {
            var newBuffer = new T[Math.Max(minLength, this.buffer.Length * 2)];
            int i = 0;
            while (i < newBuffer.Length)
            {
                T val;
                if (i < this.ItemsAvailable)
                {
                    val = this.PeekAtInternal(i);
                }
                else
                {
                    var r = this.read();
                    if (!r.HasValue)
                    {
                        break;
                    }

                    val = r.Value;
                }

                newBuffer[i++] = val;
            }

            this.buffer = newBuffer;
            this.readIndex = 0;
            this.writeIndex = i - 1;
            this.ItemsAvailable = i;
        }

        private T PeekAtInternal(int la)
        {
            var index = (this.readIndex + la) % this.buffer.Length;
            return this.buffer[index];
        }

        private void IncIndex(ref int index)
        {
            if (++index > this.buffer.Length - 1)
            {
                index = 0;
            }
        }

        private bool Read()
        {
            T? val = this.read();
            if (val.HasValue)
            {
                this.IncIndex(ref this.writeIndex);
                this.buffer[this.writeIndex] = val.Value;
                this.ItemsAvailable++;
                return true;
            }

            return false;
        }
    }
}
