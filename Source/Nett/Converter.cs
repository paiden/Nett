using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nett
{
    internal static class Converter
    {
        private static readonly Dictionary<Type, Dictionary<Type, Func<object, object>>> ConvertFunctions = new Dictionary<Type, Dictionary<Type, Func<object, object>>>()
        {
            {
                typeof(long), new Dictionary<Type, Func<object, object>>()
                {
                    { typeof(char), (l) => (char)(long)l },
                    { typeof(double), (l) => (double)(long)l },
                    { typeof(float), (l) => (float)(long)l },
                    { typeof(int), (l) => (int)(long)l },
                    { typeof(short), (l) => (short)(long)l },
                    { typeof(uint), (l) => (uint)(long)l },
                    { typeof(ushort), (l) => (ushort)(long)l },
                }
            },
            {
                typeof(TomlArray), new Dictionary<Type, Func<object, object>>()
                {
                    { typeof(byte[]), (a) => ((TomlArray)a).To<byte>().ToArray() },
                    { typeof(List<byte>), (a) => ((TomlArray)a).To<byte>().ToList() },
                    { typeof(char[]), (a) => ((TomlArray)a).To<char>().ToArray() },
                    { typeof(List<char>), (a) => ((TomlArray)a).To<char>().ToList() },
                    { typeof(double[]), (a) => ((TomlArray)a).To<double>().ToArray() },
                    { typeof(List<double>), (a) => ((TomlArray)a).To<double>().ToList() },
                    { typeof(float[]), (a) => ((TomlArray)a).To<float>().ToArray() },
                    { typeof(List<float>), (a) => ((TomlArray)a).To<float>().ToList() },
                    { typeof(int[]), (a) => ((TomlArray)a).To<int>().ToArray() },
                    { typeof(List<int>), (a) => ((TomlArray)a).To<int>().ToList() },
                    { typeof(short[]), (a) => ((TomlArray)a).To<short>().ToArray() },
                    { typeof(List<short>), (a) => ((TomlArray)a).To<short>().ToList() },
                    { typeof(uint[]), (a) => ((TomlArray)a).To<uint>().ToArray() },
                    { typeof(List<uint>), (a) => ((TomlArray)a).To<uint>().ToList() },
                    { typeof(ushort[]), (a) => ((TomlArray)a).To<ushort>().ToArray() },
                    { typeof(List<ushort>), (a) => ((TomlArray)a).To<ushort>().ToList() },
                }
            },
        };

        public static TRes Convert<TRes>(object src)
        {
            if(typeof(TRes) == src.GetType())
            {
                return (TRes)src;
            }

            Dictionary<Type, Func<object, object>> convertersForSourceType;
            if(ConvertFunctions.TryGetValue(src.GetType(), out convertersForSourceType))
            {
                Func<object, object> convertFunc;
                if(convertersForSourceType.TryGetValue(typeof(TRes), out convertFunc))
                {
                    return (TRes)convertFunc(src);
                }
            }

            throw new Exception(string.Format("Cannot convert from type '{0}' to type '{1}'.", src.GetType().Name, typeof(TRes).Name));
        }

        public static object Convert(Type tRes, object src)
        {
            if (tRes == src.GetType())
            {
                return src;
            }

            Dictionary<Type, Func<object, object>> convertersForSourceType;
            if (ConvertFunctions.TryGetValue(src.GetType(), out convertersForSourceType))
            {
                Func<object, object> convertFunc;
                if (convertersForSourceType.TryGetValue(tRes, out convertFunc))
                {
                    return convertFunc(src);
                }
            }

            throw new Exception(string.Format("Cannot convert from type '{0}' to type '{1}'.", src.GetType().Name, tRes.Name));
        }
    }
}
