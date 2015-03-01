using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nett
{
    public class TomlArray
    {
        private readonly List<object> items = new List<object>();

        public void Add(object o)
        {
            this.items.Add(o);
        }

        public int Count
        {
            get
            {
                return this.items.Count;
            }
        }

        public object this[int index]
        {
            get
            {
                return this.items[index];
            }
        }

        public T Get<T>(int index)
        {
            return Converter.Convert<T>(this.items[index]);
        }
    }
}
