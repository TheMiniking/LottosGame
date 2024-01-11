using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Serializer
{
    public static class SerializerPool
    {
        static Dictionary<int, List<byte[]>> pool = new Dictionary<int, List<byte[]>>();
        static Dictionary<Type, FieldInfo[]> cached = new Dictionary<Type, FieldInfo[]>();
        public static byte[] RentBytes(int size)
        {
            if (pool.TryGetValue(size, out List<byte[]> list))
            {
                //Debug.Log(string.Join(" ", pool.Select(a => a.Key)) + " pool " + list.Count);
                lock (pool)
                {
                    if (list.Count > 0)
                    {
                        byte[] array;

                        array = list[0];
                        list.RemoveAt(0);
                        return array;
                    }
                    else
                    {
                        //Debug.Log("a new array list count" + size);
                        return new byte[size];
                    }
                }
            }
            else
            {
                lock (pool)
                {
                    pool.Add(size, new List<byte[]>());
                    //Debug.Log("b new array " + size);
                    return new byte[size];
                }
            }
        }
        public static void ReturnBytes(byte[] array)
        {
            //Debug.Log("ReturnBytes " + array.Length);
            lock (pool)
            {
                if (pool.ContainsKey(array.Length))
                {
                    pool[array.Length].Add(array);
                }
                else
                {
                    pool.Add(array.Length, new List<byte[]>() { array });
                }
            }
        }

        public static FieldInfo[] GetFieldInfos(Type type)
        {
            try
            {
                return cached[type];
            }
            catch (Exception)
            {
                FieldInfo[] f;
                lock (cached)
                {
                    f = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
                    cached.Add(type, f);
                }
                return f;
                throw;
            }
        }

    }
}