using System;
using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using Nett.Util;
using Xunit;

namespace Nett.Tests.Internal.Util
{
    public class DictReflectUtilTests
    {
        public static TheoryData<Type, Type, Type> DictInputs
        {
            get
            {
                var data = new TheoryData<Type, Type, Type>();
                data.Add(typeof(IDictionary), typeof(object), typeof(object));
                data.Add(typeof(Hashtable), typeof(object), typeof(object));
                data.Add(typeof(Impl), typeof(object), typeof(object));
                data.Add(typeof(Derived), typeof(object), typeof(object));
                data.Add(typeof(IDictionary<int, string>), typeof(int), typeof(string));
                data.Add(typeof(Dictionary<string, int>), typeof(string), typeof(int));
                data.Add(typeof(GenericImpl), typeof(string), typeof(int));
                data.Add(typeof(GenericDerived), typeof(string), typeof(string));
                data.Add(typeof(GenericDerived2), typeof(string), typeof(string));
                data.Add(typeof(OpenImpl<int, string>), typeof(int), typeof(string));
                data.Add(typeof(ImplBoth<int, string>), typeof(int), typeof(string));
                return data;
            }
        }

        [Theory]
        [MemberData(nameof(DictInputs))]
        public void GetElementType_GivenDictInputType_ReturnsCorrectElementType(Type input, Type _, Type expectedEleType)
        {
            // Act
            var eleType = DictReflectUtil.GetElementType(input);

            // Assert
            eleType.Should().Be(expectedEleType);
        }

        private class GenericDerived2 : GenericDerived { }

        private class GenericDerived : Dictionary<string, string> { }

        private class Derived : Hashtable { }

        private class GenericImpl : IDictionary<string, int>
        {
            public int this[string key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public ICollection<string> Keys => throw new NotImplementedException();

            public ICollection<int> Values => throw new NotImplementedException();

            public int Count => throw new NotImplementedException();

            public bool IsReadOnly => throw new NotImplementedException();

            public void Add(string key, int value)
            {
                throw new NotImplementedException();
            }

            public void Add(KeyValuePair<string, int> item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(KeyValuePair<string, int> item)
            {
                throw new NotImplementedException();
            }

            public bool ContainsKey(string key)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(KeyValuePair<string, int>[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public IEnumerator<KeyValuePair<string, int>> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            public bool Remove(string key)
            {
                throw new NotImplementedException();
            }

            public bool Remove(KeyValuePair<string, int> item)
            {
                throw new NotImplementedException();
            }

            public bool TryGetValue(string key, out int value)
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }

        private class Impl : IDictionary
        {
            public object this[object key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public ICollection Keys => throw new NotImplementedException();

            public ICollection Values => throw new NotImplementedException();

            public bool IsReadOnly => throw new NotImplementedException();

            public bool IsFixedSize => throw new NotImplementedException();

            public int Count => throw new NotImplementedException();

            public object SyncRoot => throw new NotImplementedException();

            public bool IsSynchronized => throw new NotImplementedException();

            public void Add(object key, object value)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(object key)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(Array array, int index)
            {
                throw new NotImplementedException();
            }

            public IDictionaryEnumerator GetEnumerator()
            {
                throw new NotImplementedException();
            }

            public void Remove(object key)
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }

        private class OpenImpl<TKey, TValue> : IDictionary<TKey, TValue>
        {
            public TValue this[TKey key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public ICollection<TKey> Keys => throw new NotImplementedException();

            public ICollection<TValue> Values => throw new NotImplementedException();

            public int Count => throw new NotImplementedException();

            public bool IsReadOnly => throw new NotImplementedException();

            public void Add(TKey key, TValue value)
            {
                throw new NotImplementedException();
            }

            public void Add(KeyValuePair<TKey, TValue> item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(KeyValuePair<TKey, TValue> item)
            {
                throw new NotImplementedException();
            }

            public bool ContainsKey(TKey key)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            public bool Remove(TKey key)
            {
                throw new NotImplementedException();
            }

            public bool Remove(KeyValuePair<TKey, TValue> item)
            {
                throw new NotImplementedException();
            }

            public bool TryGetValue(TKey key, out TValue value)
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }

        private class ImplBoth<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary
        {
            public object this[object key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public TValue this[TKey key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public ICollection Keys => throw new NotImplementedException();

            public ICollection Values => throw new NotImplementedException();

            public bool IsReadOnly => throw new NotImplementedException();

            public bool IsFixedSize => throw new NotImplementedException();

            public int Count => throw new NotImplementedException();

            public object SyncRoot => throw new NotImplementedException();

            public bool IsSynchronized => throw new NotImplementedException();

            ICollection<TKey> IDictionary<TKey, TValue>.Keys => throw new NotImplementedException();

            ICollection<TValue> IDictionary<TKey, TValue>.Values => throw new NotImplementedException();

            public void Add(object key, object value)
            {
                throw new NotImplementedException();
            }

            public void Add(TKey key, TValue value)
            {
                throw new NotImplementedException();
            }

            public void Add(KeyValuePair<TKey, TValue> item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(object key)
            {
                throw new NotImplementedException();
            }

            public bool Contains(KeyValuePair<TKey, TValue> item)
            {
                throw new NotImplementedException();
            }

            public bool ContainsKey(TKey key)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(Array array, int index)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public IDictionaryEnumerator GetEnumerator()
            {
                throw new NotImplementedException();
            }

            public void Remove(object key)
            {
                throw new NotImplementedException();
            }

            public bool Remove(TKey key)
            {
                throw new NotImplementedException();
            }

            public bool Remove(KeyValuePair<TKey, TValue> item)
            {
                throw new NotImplementedException();
            }

            public bool TryGetValue(TKey key, out TValue value)
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }
    }
}
