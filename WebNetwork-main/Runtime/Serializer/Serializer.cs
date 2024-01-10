using System;
using System.Collections.Generic;
using System.IO;
using Unity.IO.Compression;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Serializer
{
    public static class MesageExtend
    {
        public static byte[] GetData<T>(this T data) where T : INetSerializable
        {
            try
            {
                ushort msgType = (ushort)(typeof(T).FullName.GetHashCode() & 0xFFFF);
                Serializer.NetSerializer(data, out byte[] bytes, out bool compressed);
                var b = SerializerPool.RentBytes(bytes.Length + 3);
                FastBitConverter.GetBytes(b, 0, msgType);
                b[2] = (byte)(compressed ? 1 : 0);
                Array.Copy(bytes, 0, b, 3, bytes.Length);
                Dispatcher.Instance.ExecuteInMainThread(() =>
                {
                    SerializerPool.ReturnBytes(b);
                }, 1);
                //Debug.Log(typeof(T) + " GetData 5 b Length " + b.Length);
                return b;
            }
            catch (Exception e)
            {
                Debug.Log("GetData Exception " + e.Message);

                return new byte[0];
            }
        }
        public static ushort GetClass<T>(this T data, byte[] bytes) where T : INetSerializable, new()
        {
            ushort msgType = BitConverter.ToUInt16(bytes, 0);
            Serializer.NetDeserialize(bytes, 3, out data);
            return msgType;
        }

    }
    public static class Serializer
    {

        static List<DataWriter> dataWriters = new List<DataWriter>();
        static List<DataReader> dataReaders = new List<DataReader>();

        static List<PackageWriter> packageWriters = new List<PackageWriter>();
        static List<PackageReader> packageReaders = new List<PackageReader>();

        public static byte[] CompressToArray(byte[] value)
        {
            using (var inputStream = new MemoryStream(value))
            {
                using (var outputStream = new MemoryStream())
                {
                    using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress, true))
                    {
                        inputStream.CopyTo(gzipStream);
                    }
                    return outputStream.ToArray();
                }
            }
        }
        public static byte[] CompressToArray2(byte[] value)
        {
            using (var inputStream = new MemoryStream(value))
            {
                using (var outputStream = new MemoryStream())
                {
                    using (var gzipStream = new DeflateStream(outputStream, CompressionMode.Compress))
                    {
                        inputStream.CopyTo(gzipStream);
                    }
                    return outputStream.ToArray();
                }
            }
        }
        public static byte[] Decompress(byte[] bytes, int offset)
        {
            using (var compressedStream = new MemoryStream(bytes, offset, bytes.Length - offset))
            using (var zipStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
            using (var resultStream = new MemoryStream())
            {
                zipStream.CopyTo(resultStream);
                return resultStream.ToArray();
            }
        }

        public static void NetSerializer<T>(T data, out byte[] bytes, out bool compressed, bool compress = true) where T : INetSerializable
        {
            DataWriter dataWriter;
            compressed = false;
            lock (dataWriters)
            {
                if (dataWriters.Count == 0)
                {
                    dataWriter = new DataWriter();
                }
                else
                {
                    dataWriter = dataWriters[0];
                    dataWriters.Remove(dataWriter);
                }
            }
            var b = dataWriter.GetBytes(data);
            if (compress)
            {
                bytes = CompressToArray2(b);
                if (bytes.Length > b.Length)
                {
                    bytes = b;
                }
                else compressed = true;
            }
            else
            {
                bytes = b;
            }
            lock (dataWriters)
            {
                dataWriters.Add(dataWriter);
            }
            if (compressed)
                SerializerPool.ReturnBytes(b);
        }
        public static bool NetDeserialize<T>(byte[] bytes, int offset, out T data, bool compress = true) where T : INetSerializable, new()
        {
            DataReader dataReader;
            object output = new T();
            lock (dataReaders)
            {
                if (dataReaders.Count == 0)
                {
                    dataReader = new DataReader();
                }
                else
                {
                    dataReader = dataReaders[0];
                    dataReaders.RemoveAt(0);
                }
            }
            if (compress && bytes[2] == 1)
            {
                var d = Decompress(bytes, offset);
                dataReader.SetData(d, 0);
                Debug.Log("NetDeserialize  d.Length " + d.Length);
            }
            else
            {
                dataReader.SetData(bytes, offset);
            }
            data = (T)output;
            data.Deserialize(dataReader);
            lock (dataReaders)
            {
                dataReaders.Add(dataReader);
            }
            return true;
        }
        public static void SaveSerializer<T>(T data, out byte[] bytes, bool compress = true) where T : ISaveSerializable
        {
            ////Debug.Log("NetSerializer");
            PackageWriter dataWriter;
            lock (packageWriters)
            {
                if (packageWriters.Count == 0)
                {
                    dataWriter = new PackageWriter();
                }
                else
                {
                    dataWriter = packageWriters[0];
                    packageWriters.Remove(dataWriter);
                }
            }
            var b = dataWriter.GetBytes(data);
            if (compress)
            {
                bytes = CompressToArray2(b);
                Debug.Log("SaveSerializer  bytes.Length " + bytes.Length);
            }
            else
            {
                bytes = b;
            }
            lock (packageWriters)
            {
                packageWriters.Add(dataWriter);
            }
            SerializerPool.ReturnBytes(b);
        }
        public static bool SaveDeserialize<T>(byte[] bytes, int offset, out T data, bool compress = true) where T : ISaveSerializable, new()
        {
            PackageReader dataReader;
            object output = new T();
            lock (packageReaders)
            {
                if (packageReaders.Count == 0)
                {
                    dataReader = new PackageReader();
                }
                else
                {
                    dataReader = packageReaders[0];
                    packageReaders.RemoveAt(0);
                }
            }
            if (compress)
            {
                var d = Decompress(bytes, offset);
                dataReader.SetSource(d);
            }
            else
            {
                dataReader.SetSource(bytes);
            }
            data = (T)output;
            Debug.Log(typeof(T).Name+" Deserialize "+ dataReader.GetLineCount);
            data.Deserialize(dataReader);
            lock (packageReaders)
            {
                packageReaders.Add(dataReader);
            }
            return true;
        }


        //private static byte[] Compress(byte[] data)
        //{
        //    Debug.Log("Compress data " + String.Join("-", data));
        //    return data;
        //    //using (MemoryStream output = new MemoryStream())
        //    //{
        //    //    using (DeflateStream compressor = new DeflateStream(output, CompressionMode.Compress))
        //    //    {
        //    //        compressor.Write(data, 0, data.Length);
        //    //    }

        //    //    return output.ToArray();
        //    //}
        //    using (MemoryStream output = new MemoryStream())
        //    {
        //        using (GZipStream dstream = new GZipStream(output, CompressionMode.Compress))
        //        {
        //            dstream.Write(data, 0, data.Length);
        //        }
        //        var outputData = output.ToArray();
        //        Debug.Log("Compress outputData " + String.Join("-", outputData));
        //        return outputData;
        //    }
        //}
        //private static byte[] Decompress(byte[] data, int startOffset)
        //{
        //    Debug.Log("Decompress data " + String.Join("-", data));
        //    using (var ms = new MemoryStream(data.Skip(startOffset).ToArray()))
        //    {
        //        using (var decompressor = new GZipStream(ms, CompressionMode.Decompress))
        //        {
        //            using (var output = new MemoryStream())
        //            {
        //                decompressor.CopyTo(output);
        //                return output.ToArray();
        //            }
        //        }
        //    }
        //    //////Debug.Log(startOffset + " Decompress a " + BitConverter.ToString(data));
        //    //MemoryStream input = new MemoryStream(data, startOffset, data.Length - startOffset);
        //    //MemoryStream output = new MemoryStream();

        //    //////Debug.Log(" Decompress b ");
        //    //using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
        //    //{
        //    //    ////Debug.Log(" Decompress c ");
        //    //    dstream.CopyTo(output);
        //    //    ////Debug.Log(" Decompress d ");
        //    //}
        //    //////Debug.Log(startOffset+" Decompress e " + data.Length);
        //    //return output.ToArray();
        //}


    }
    public class PackageWriter
    {
        Dictionary<ushort, byte[]> lines = new Dictionary<ushort, byte[]>();
        private int m_position;

        public PackageWriter()
        {

        }

        internal byte[] GetBytes<T>(T obj) where T : ISaveSerializable
        {
            m_position = 0;
            lines.Clear();
            obj.Serialize(this);
            //CalculateFields(data, typeof(T));  reflection
            var size = lines.Count * 6 + lines.Values.Sum(x => x.Length);
            var bytes = SerializerPool.RentBytes(size);
            ////Debug.Log(bytes+" GetBytes "+ lines.Count);
            foreach (var line in lines)
            {
                var ls = line.Value.Length;
                FastBitConverter.GetBytes(bytes, m_position, line.Key);
                m_position += 2;
                FastBitConverter.GetBytes(bytes, m_position, ls);
                m_position += 4;
                Buffer.BlockCopy(line.Value, 0, bytes, m_position, ls);
                m_position += ls;
            }
            return bytes;
        }
        ushort GetKeyID(string keyName)
        {
            int key = keyName.GetHashCode();
            ////Debug.LogError($"TGetKeyID {keyName}, id {(ushort)key}");
            return (ushort)key;
        }
        bool CheckID(ushort key)
        {
            if (key < 0)
            {
                ////Debug.LogError($"ID cannot be negative, try insert id {key} never change the key id, or data will be lost when deserializing.");
                return true;
            }
            if (lines.ContainsKey(key))
            {
                ////Debug.LogError($"This key already exists {key}, never change the key id, or data will be lost when deserializing");
                return true;
            }
            return false;
        }
        #region PutLits
        //void PutList(object value, Type type, string keyName)
        //{
        //    var key = GetKeyID(keyName);
        //    if (CheckID(key)) return;
        //    if (type == typeof(string))
        //    {
        //        IEnumerable<string> l = value as IEnumerable<string>;
        //        Put((ushort)l.Count());
        //        foreach (var item in l)
        //        {
        //            Put(item);
        //        }
        //    }
        //    else if (type == typeof(bool))
        //    {

        //    }
        //    else if (type == typeof(byte))
        //    {
        //        IEnumerable<byte> l = value as IEnumerable<byte>;
        //        Put((ushort)l.Count());
        //        foreach (var item in l)
        //        {
        //            Put(item);
        //        }
        //    }
        //    else if (type == typeof(UInt16))
        //    {

        //    }
        //    else if (type == typeof(Int16))
        //    {

        //    }
        //    else if (type == typeof(UInt32))
        //    {

        //    }
        //    else if (type == typeof(Int32))
        //    {

        //    }
        //    else if (type == typeof(UInt64))
        //    {

        //    }
        //    else if (type == typeof(Int64))
        //    {

        //    }
        //    else if (type == typeof(Single))
        //    {

        //    }
        //    else if (type == typeof(Double))
        //    {

        //    }
        //    else if (type == typeof(Vector2))
        //    {

        //    }
        //    else if (type == typeof(Vector3))
        //    {

        //    }
        //}

        #endregion
        public void Put(string v, string keyName)
        {
            var key = GetKeyID(keyName);
            if (v == null || v.Length == 0)
            {
                byte[] d = new byte[2];
                FastBitConverter.GetBytes(d, 0, (short)2);
                lines.Add(key, d);
                return;
            }
            short len = (short)(v == null ? 0 : v.Length);
            byte[] _data = new byte[len + 2];
            var position = 0;
            FastBitConverter.GetBytes(_data, position, len);
            position += 2;
            Encoding.UTF8.GetBytes(v, 0, v.Length, _data, position);
            lines.Add(key, _data);

        }
        public void Put<T>(IEnumerable<T> objs, string keyName) where T : ISaveSerializable
        {
            if (objs == null || objs.Count() == 0) return;
            //Debug.Log("Put<T>(IEnumerable<T> "+ keyName);
            var key = GetKeyID(keyName);
            int len = 2;
            int count = objs == null ? 0 : objs.Count();
            List<byte[]> d = new List<byte[]>();
            int i = 0;
            foreach (var obj in objs)
            {
                if (obj == null)
                {
                    len += 4;
                    d.Add(new byte[0]);
                    continue;
                }
                try
                {
                    var ps = new PackageWriter();
                    var data = ps.GetBytes(obj);
                    d.Add(data);
                    len += 4;
                    len += data.Length;
                    // this.LGDev(typeof(T).Name + " loop PutList<T> end try " + data.Length);
                }
                catch (Exception e)
                {
                    //Debug.LogError(typeof(T).Name + $" {obj.GetType()} loop PutList<T>Exception " + e.Message);
                    d.Add(new byte[0]);
                    len += 4;
                }
                i++;
            }
            var position = 0;
            byte[] _data = new byte[len];
            FastBitConverter.GetBytes(_data, position, (ushort)count);
            position += 2;
            int j = 0;
            foreach (var bytes in d)
            {
                j++;
                try
                {
                    // this.LGDev(typeof(T).Name + " PutList<T> bytes.Length " + bytes.Length);
                    FastBitConverter.GetBytes(_data, position, bytes.Length);
                    position += 4;
                    Buffer.BlockCopy(bytes, 0, _data, position, bytes.Length);
                    position += bytes.Length;
                }
                catch (Exception e)
                {
                    //Debug.LogError(typeof(T).Name + $" index {j} position {position} bytes.Length {bytes.Length} _data.Length {_data.Length} PutList<T> (var bytes in d)  Exception " + e.Message);
                }
            }
            lines.Add(key, _data);
            // this.LGDev("done _data.Length  " + _data.Length);
        }
        public void Put<T>(T v, string keyName) where T : ISaveSerializable
        {
            ////Debug.Log("Put T "+typeof(T).Name);
            var key = GetKeyID(keyName);
            if (CheckID(key)) return;
            if (v == null) return;
            var ps = new PackageWriter();

            var data = ps.GetBytes(v);
            lines.Add(key, data);
        }
        //public void Put(object v, string keyName)
        //{
        //    var key = GetKeyID(keyName);
        //    if (CheckID(key)) return;
        //    if (v == null) return;
        //    Type t = v.GetType();
        //    var ps = new PackageWriter();
        //    byte[] data;
        //    if (t.Name.CompareTo("Int16") == 0)
        //    {
        //        Put((short)v, keyName);
        //        return;
        //    }
        //    if (t.Name.CompareTo("Int32") == 0)
        //    {
        //        Put((int)v, keyName);
        //        return;
        //    }
        //    if (t.Name.CompareTo("Int64") == 0)
        //    {
        //        Put((long)v, keyName);
        //        return;
        //    }
        //    if (t.Name.CompareTo("Single") == 0)
        //    {
        //        Put((float)v, keyName);
        //        return;
        //    }
        //    if (t.Name.CompareTo("Vector2") == 0)
        //    {
        //        Put((Vector2)v, keyName);
        //        return;
        //    }
        //    if (t.Name.CompareTo("Vector3") == 0)
        //    {
        //        Put((Vector3)v, keyName);
        //        return;
        //    }
        //    if (t.Name.CompareTo("String") == 0)
        //    {
        //        Put((string)v, keyName);
        //        return;
        //    }
        //    else
        //    {
        //        v.GetType().GetMethod(ksajdsadjas7689987.AxNIKg4q).Invoke(v, new[] { ps });
        //    }
        //    //v.Serialize(ps);

        //    data = ps.GetBytes(v);
        //    lines.Add(key, data);
        //    //this.LGDev(data.Length + " Put<T> " + key);

        //    //ps.Clear();
        //}
        public void Put(bool value, string keyName)
        {
            var key = GetKeyID(keyName);
            byte[] _data = new byte[1];
            _data[0] = (byte)(value ? 1 : 0);
            lines.Add(key, _data);
        }
        public void Put(Vector3 value, string keyName)
        {
            var key = GetKeyID(keyName);
            byte[] _data = new byte[12];
            var position = 0;
            FastBitConverter.GetBytes(_data, position, value.x);
            position += 4;
            FastBitConverter.GetBytes(_data, position, value.y);
            position += 4;
            FastBitConverter.GetBytes(_data, position, value.z);
            lines.Add(key, _data);
            ////Debug.Log("Put Vector3 " + value);
        }
        public void Put(Vector2 value, string keyName)
        {
            var key = GetKeyID(keyName);
            byte[] _data = new byte[8];
            var position = 0;
            FastBitConverter.GetBytes(_data, position, value.x);
            position += 4;
            FastBitConverter.GetBytes(_data, position, value.y);
            lines.Add(key, _data);
        }
        public void Put(IEnumerable<Vector2> vs, string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key)) return;
            byte[] _data;
            if (vs == null || vs.Count() == 0)
            {
                return;
            }
            List<byte[]> bytes = new List<byte[]>();
            var len = 2;
            foreach (var v in vs)
            {
                byte[] _d = new byte[8];
                var p = 0;
                FastBitConverter.GetBytes(_d, p, v.x);
                p += 4;
                FastBitConverter.GetBytes(_d, p, v.y);
                bytes.Add(_d);
            }
            var position = 0;
            _data = new byte[len];
            FastBitConverter.GetBytes(_data, position, (ushort)vs.Count());
            position += 2;
            foreach (var b in bytes)
            {
                Buffer.BlockCopy(b, 0, _data, position, b.Length);
                position += b.Length;
            }
            lines.Add(key, _data);
        }
        public void Put(Quaternion value, string keyName)
        {
            var key = GetKeyID(keyName);
            var vec = value.eulerAngles;
            byte[] _data = new byte[12];
            var position = 0;
            FastBitConverter.GetBytes(_data, position, vec.x);
            position += 4;
            FastBitConverter.GetBytes(_data, position, vec.y);
            position += 4;
            FastBitConverter.GetBytes(_data, position, vec.z);
            lines.Add(key, _data);
        }
        public void Put(Color value, string keyName)
        {
            var key = GetKeyID(keyName);
            byte[] _data = new byte[12];
            var position = 0;
            FastBitConverter.GetBytes(_data, position, value.r);
            position += 4;
            FastBitConverter.GetBytes(_data, position, value.g);
            position += 4;
            FastBitConverter.GetBytes(_data, position, value.b);
            lines.Add(key, _data);
        }
        
        public void Put(double value, string keyName)
        {
            var key = GetKeyID(keyName);
            byte[] _data = new byte[8];
            var position = 0;
            FastBitConverter.GetBytes(_data, position, value);
            lines.Add(key, _data);
        }
        public void Put(long value, string keyName)
        {
            //Debug.Log("long byte");
            var key = GetKeyID(keyName);
            byte[] _data = new byte[8];
            var position = 0;
            FastBitConverter.GetBytes(_data, position, value);
            lines.Add(key, _data);
        }
        public void Put(ulong value, string keyName)
        {
            var key = GetKeyID(keyName);
            byte[] _data = new byte[8];
            var position = 0;
            FastBitConverter.GetBytes(_data, position, value);
            lines.Add(key, _data);
        }
        public void Put(int value, string keyName)
        {
            //Debug.Log("long byte");
            var key = GetKeyID(keyName);
            byte[] _data = new byte[4];
            var position = 0;
            FastBitConverter.GetBytes(_data, position, value);
            lines.Add(key, _data);
        }
        public void Put(uint value, string keyName)
        {
            //Debug.Log("uint byte");
            var key = GetKeyID(keyName);
            byte[] _data = new byte[4];
            var position = 0;
            FastBitConverter.GetBytes(_data, position, value);
            lines.Add(key, _data);
        }
        public void Put(char value, string keyName)
        {
            var key = GetKeyID(keyName);
            byte[] _data = new byte[2];
            var position = 0;
            FastBitConverter.GetBytes(_data, position, value);
            lines.Add(key, _data);
        }
        public void Put(ushort value, string keyName)
        {
            var key = GetKeyID(keyName);
            byte[] _data = new byte[2];
            var position = 0;
            FastBitConverter.GetBytes(_data, position, value);
            lines.Add(key, _data);
        }
        public void Put(short value, string keyName)
        {
            var key = GetKeyID(keyName);
            byte[] _data = new byte[2];
            var position = 0;
            FastBitConverter.GetBytes(_data, position, value);
            lines.Add(key, _data);
        }
        public void Put(sbyte value, string keyName)
        {
            var key = GetKeyID(keyName);
            byte[] _data = new byte[1];
            var position = 0;
            _data[position] = (byte)value;
            lines.Add(key, _data);
        }
        public void Put(byte value, string keyName)
        {
            //Debug.Log("Put byte");
            var key = GetKeyID(keyName);
            byte[] _data = new byte[1];
            var position = 0;
            _data[position] = (byte)value;
            lines.Add(key, _data);
        }
        public void Put(byte[] data, string keyName)
        {
            if (data == null) return;
            var key = GetKeyID(keyName);
            lines.Add(key, data);
        }
        public void Put(byte[,] value, string keyName)
        {
            if (value == null) return;
            var key = GetKeyID(keyName);
            int lenx = value.GetLength(0);
            int leny = value.GetLength(1);
            int len = lenx * leny;
            byte[] _data = new byte[len + 8];
            ////Debug.Log("PutList " + len);
            var position = 0;
            FastBitConverter.GetBytes(_data, position, lenx);
            position += 4;
            FastBitConverter.GetBytes(_data, position, leny);
            position += 4;
            for (int x = 0; x < lenx; x++)
            {
                for (int y = 0; y < lenx; y++)
                {
                    _data[position] = value[x, y];
                    position += 1;
                }
            }
            lines.Add(key, _data);
        }
        public void Put(IEnumerable<byte> value, string keyName)
        {
            if (value == null) return;
            var key = GetKeyID(keyName);
            int len = value == null ? 0 : value.Count();
            byte[] _data = new byte[len + 4];
            ////Debug.Log("PutList " + len);
            var position = 0;
            FastBitConverter.GetBytes(_data, position, len);
            position += 4;
            for (int i = 0; i < len; i++)
            {
                _data[position] = value.ElementAt(i);
                position += 1;
            }
            lines.Add(key, _data);
        }
        public void Put(IEnumerable<byte[]> value, string keyName)
        {
            if (value == null) return;
            var key = GetKeyID(keyName);
            ushort len = (ushort)(value == null ? 0 : value.Count());
            byte[] _data = new byte[len + value.Sum(s => s.Length + 2) + 2];
            var position = 0;
            FastBitConverter.GetBytes(_data, position, len);
            position += 2;
            for (int i = 0; i < len; i++)
            {
                ushort ls = (ushort)value.ElementAt(i).Length;
                FastBitConverter.GetBytes(_data, position, ls);
                position += 2;
                Buffer.BlockCopy(value.ElementAt(i), 0, _data, position, ls);
                position += ls;
            }
            //this.LGDev("PutList " + _data.Length);
            lines.Add(key, _data);
        }
        public void Put(IEnumerable<float> value, string keyName)
        {
            if (value == null) return;
            var key = GetKeyID(keyName);
            int len = value == null ? 0 : value.Count();
            byte[] _data = new byte[len * 4 + 4];
            ////Debug.Log("PutList " + len);
            var position = 0;
            FastBitConverter.GetBytes(_data, position, len);
            position += 4;
            for (int i = 0; i < len; i++)
            {
                FastBitConverter.GetBytes(_data, position, value.ElementAt(i));
                position += 4;
            }
            lines.Add(key, _data);
        }
        public void Put(IEnumerable<double> value, string keyName)
        {
            if (value == null) return;
            var key = GetKeyID(keyName);
            int len = value == null ? 0 : value.Count();
            byte[] _data = new byte[len * 8 + 4];
            ////Debug.Log("PutList " + len);
            var position = 0;
            FastBitConverter.GetBytes(_data, position, len);
            position += 4;
            for (int i = 0; i < len; i++)
            {
                FastBitConverter.GetBytes(_data, position, value.ElementAt(i));
                position += 8;
            }
            lines.Add(key, _data);
        }
        public void Put(IEnumerable<long> value, string keyName)
        {
            if (value == null) return;
            var key = GetKeyID(keyName);
            int len = value == null ? 0 : value.Count();
            byte[] _data = new byte[len * 8 + 4];
            ////Debug.Log("PutList " + len);
            var position = 0;
            FastBitConverter.GetBytes(_data, position, len);
            position += 4;
            for (int i = 0; i < len; i++)
            {
                FastBitConverter.GetBytes(_data, position, value.ElementAt(i));
                position += 8;
            }
            lines.Add(key, _data);
        }
        public void Put(IEnumerable<ulong> value, string keyName)
        {
            if (value == null) return;
            var key = GetKeyID(keyName);
            int len = value == null ? 0 : value.Count();
            byte[] _data = new byte[len * 8 + 4];
            ////Debug.Log("PutList " + len);
            var position = 0;
            FastBitConverter.GetBytes(_data, position, len);
            position += 4;
            for (int i = 0; i < len; i++)
            {
                FastBitConverter.GetBytes(_data, position, value.ElementAt(i));
                position += 8;
            }
            lines.Add(key, _data);
        }
        public void Put(IEnumerable<uint> value, string keyName)
        {
            if (value == null) return;
            var key = GetKeyID(keyName);
            int len = value == null ? 0 : value.Count();
            byte[] _data = new byte[len * 4 + 4];
            ////Debug.Log("PutList " + len);
            var position = 0;
            FastBitConverter.GetBytes(_data, position, len);
            position += 4;
            for (int i = 0; i < len; i++)
            {
                FastBitConverter.GetBytes(_data, position, value.ElementAt(i));
                position += 4;
            }
            lines.Add(key, _data);
        }
        public void Put(IEnumerable<int> value, string keyName)
        {
            if (value == null) return;
            var key = GetKeyID(keyName);
            int len = value == null ? 0 : value.Count();
            byte[] _data = new byte[len * 4 + 4];
            ////Debug.Log("PutList " + len);
            var position = 0;
            FastBitConverter.GetBytes(_data, position, len);
            position += 4;
            for (int i = 0; i < len; i++)
            {
                FastBitConverter.GetBytes(_data, position, value.ElementAt(i));
                position += 4;
            }
            lines.Add(key, _data);
        }
        public void Put(IEnumerable<ushort> value, string keyName)
        {
            if (value == null) return;
            var key = GetKeyID(keyName);
            ushort len = (ushort)(value == null ? 0 : value.Count());
            byte[] _data = new byte[len * 2 + 2];
            var position = 0;
            FastBitConverter.GetBytes(_data, position, len);
            position += 2;
            for (int i = 0; i < len; i++)
            {
                FastBitConverter.GetBytes(_data, position, value.ElementAt(i));
                position += 2;
            }
            lines.Add(key, _data);
            ////Debug.Log(_data.Length + " data leng PutList len " + len);
        }
        public void Put(IEnumerable<short> value, string keyName)
        {
            if (value == null) return;
            var key = GetKeyID(keyName);
            short len = (short)(value == null ? 0 : value.Count());
            byte[] _data = new byte[len * 2 + 2];
            var position = 0;
            FastBitConverter.GetBytes(_data, position, len);
            position += 2;
            for (int i = 0; i < len; i++)
            {
                FastBitConverter.GetBytes(_data, position, value.ElementAt(i));
                ////Debug.Log("Put(IEnumerable<short> " + value.ElementAt(i));
                position += 2;
            }
            lines.Add(key, _data);
        }
        public void Put(IEnumerable<bool> value, string keyName)
        {
            if (value == null) return;
            var key = GetKeyID(keyName);
            int len = value == null ? 0 : value.Count();
            byte[] _data = new byte[len + 4];
            ////Debug.Log("PutList " + len);
            var position = 0;
            FastBitConverter.GetBytes(_data, position, len);
            position += 4;
            for (int i = 0; i < len; i++)
            {
                _data[position] = (byte)(value.ElementAt(i) ? 1 : 0);
                position += 1;
            }
            lines.Add(key, _data);
        }
        public void Put(IEnumerable<Color> value, string keyName)
        {
            if (value == null) return;
            var key = GetKeyID(keyName);
            ushort len = value == null ? (ushort)0 : (ushort)value.Count();
            byte[] _data = new byte[len * 12 + 2];
            ////Debug.Log("PutList " + len);
            var position = 0;
            FastBitConverter.GetBytes(_data, position, len);
            position += 2;
            for (int i = 0; i < len; i++)
            {
                FastBitConverter.GetBytes(_data, position, value.ElementAt(i).r);
                position += 4;
                FastBitConverter.GetBytes(_data, position, value.ElementAt(i).g);
                ////Debug.Log("PutList g " + value.ElementAt(i).g);
                position += 4;
                FastBitConverter.GetBytes(_data, position, value.ElementAt(i).b);
                position += 4;
            }
            lines.Add(key, _data);
        }
        public void PutEnum(Enum value, string keyName)
        {
            var key = GetKeyID(keyName);
            byte[] _data = new byte[4];
            FastBitConverter.GetBytes(_data, 0, Convert.ToInt32(value));
            lines.Add(key, _data);
        }
    }
    public class PackageReader
    {
        Dictionary<ushort, byte[]> lines = new Dictionary<ushort, byte[]>();
        bool sumDiff = false;
        public int GetLineCount { get=> lines.Count; }
        public PackageReader()
        {

        }
        public void SetSource(byte[] source)
        {
            try
            {
                lines.Clear();
                int position = 0;
                //this.LGDev("SetSource " + source.Length);
                while (position < source.Length)
                {
                    ushort key = BitConverter.ToUInt16(source, position);
                    //this.LGDev("SetSource key " + key);
                    position += 2;
                    int size = BitConverter.ToInt32(source, position);
                    position += 4;
                    byte[] bytes = new byte[size];
                    Buffer.BlockCopy(source, position, bytes, 0, size);
                    lines.Add(key, bytes);
                    //this.LGDev("SetSource line add " + size);
                    position += size;
                }
            }
            catch (Exception e)
            {
                //Debug.LogError("SetSource " + e.Message);
            }
        }
        void Clear()
        {
            lines.Clear();
        }
        bool CheckID(ushort key)
        {
            if (key < 0)
            {
                //this.LGWarning($"Key cannot be negative, try key " + key);
                return true;
            }
            if (lines == null || lines.Count == 0 || !lines.ContainsKey(key))
            {
                //this.LGWarning($"Key does not exist " + key);
                return true;
            }
            return false;
        }

        public byte[] GetLineBytes(ushort key)
        {
            if(lines.TryGetValue(key, out byte[] _data))
            {
                return _data;
            }

            return new byte[0];
        }
        public byte[] GetLine(ushort key)
        {
            lines.TryGetValue(key, out byte[] line);
            if (line == null) line = new byte[0];
            return line;
        }
        ushort GetKeyID(string keyName)
        {
            int key = keyName.GetHashCode();
            return (ushort)key;
        }
        public int GetInt(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key))
            {
                return 0;
            }
            byte[] _data = GetLineBytes(key);
            Debug.Log("GetInt _data "+ _data.Length);
            return BitConverter.ToInt32(_data, 0);
        }
        public string GetString(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key))
            {
                return string.Empty;
            }
            byte[] _data = GetLineBytes(key);
            Debug.Log("GetString _data "+ _data.Length);
            ushort size = BitConverter.ToUInt16(_data, 0);
            Debug.Log("GetString size " + size);
            if (_data.Length <= 2)
                return "";
            return Encoding.UTF8.GetString(_data, 2, size);
        }
        public void Get<T>(ref T obj, string keyName) where T : ISaveSerializable, new()
        {
            var key = GetKeyID(keyName);

            if (CheckID(key))
            {
                obj = new T();
                return;
            }
            byte[] _data = new byte[0];
            var ps = new PackageReader();

            byte[] bytes = lines[key];
            ps.SetSource(bytes);
            obj = new T();
            obj.Deserialize(ps);
        }
        public void Get(ref Vector3 field, string keyName)
        {
            field = GetVector3(keyName);
        }
        public void Get(ref Vector2 field, string keyName)
        {
            field = GetVector2(keyName);
        }
        public void Get(ref List<Vector2> field, string keyName)
        {
            field = GetVector2List(keyName);
        }
        public void Get(ref Quaternion field, string keyName)
        {
            field = GetQuaternion(keyName);
        }
        public void Get(ref bool field, string keyName)
        {
            field = GetBool(keyName);
        }
        public void Get(ref bool[] field, string keyName)
        {
            field = GetBoolArray(keyName);
        }
        public void Get(ref List<bool> field, string keyName)
        {
            field = GetBoolList(keyName);
        }
        public void Get(ref List<ushort> field, string keyName)
        {
            field = GetUShortList(keyName);
        }
        public void Get(ref ushort[] field, string keyName)
        {
            field = GetUShortArray(keyName);
        }
        public void Get(ref long field, string keyName)
        {
            field = GetLong(keyName);
        }
        public void Get(ref long[] field, string keyName)
        {
            field = GetLongArray(keyName);
        }
        public void Get(ref List<long> field, string keyName)
        {
            field = GetLongList(keyName);
        }
        public void Get(ref ulong field, string keyName)
        {
            field = GetULong(keyName);
        }
        public void Get(ref ulong[] field, string keyName)
        {
            field = GetULongArray(keyName);
        }
        public void Get(ref List<ulong> field, string keyName)
        {
            field = GetULongList(keyName);
        }
        public void Get(ref byte field, string keyName)
        {
            ////Debug.Log("get byte field");
            field = GetByte(keyName);
        }
        public void Get(ref byte[] field, string keyName)
        {
            field = GetByteArray(keyName);
        }
        public void Get(ref List<byte> field, string keyName)
        {
            field = GetByteList(keyName);
        }
        public void Get(ref string field, string keyName)
        {
            field = GetString(keyName);
        }
        public void Get(ref string[] field, string keyName)
        {
            field = GetStringArray(keyName);
        }
        public void Get(ref List<string> field, string keyName)
        {
            field = GetStringList(keyName);
        }
        public void Get(ref ushort field, string keyName)
        {
            field = GetUShort(keyName);
        }
        public void Get(ref short field, string keyName)
        {
            field = GetShort(keyName);
        }
        public void Get(ref int field, string keyName)
        {
            Debug.Log("get int field");
            field = GetInt(keyName);
        }
        public void Get(ref int[] field, string keyName)
        {
            field = GetIntArray(keyName);
        }
        public void Get(ref List<int> field, string keyName)
        {
            field = GetIntList(keyName);
        }
        public void Get(ref uint field, string keyName)
        {
            //Debug.Log("get uint field");
            field = GetUInt(keyName);
        }
        public void Get(ref uint[] field, string keyName)
        {
            field = GetUIntArray(keyName);
        }
        public void Get(ref List<uint> field, string keyName)
        {
            field = GetUIntList(keyName);
        }
        public void Get(ref float field, string keyName)
        {
            field = GetFloat(keyName);
        }
        public void Get(ref float[] field, string keyName)
        {
            field = GetFloatArray(keyName);
        }
        public void Get(ref List<float> field, string keyName)
        {
            field = GetFloatList(keyName);
        }
        public void Get(ref double field, string keyName)
        {
            field = GetDouble(keyName);
        }
        public void Get(ref double[] field, string keyName)
        {
            field = GetDoubleArray(keyName);
        }
        public void Get(ref List<double> field, string keyName)
        {
            field = GetDoubleList(keyName);
        }
        public void Get(ref Color field, string keyName)
        {
            field = GetColor(keyName);
        }
        public void Get(ref List<Color> field, string keyName)
        {
            field = GetColorList(keyName);
        }
        public void Get(ref Color[] field, string keyName)
        {
            field = GetColorList(keyName).ToArray();
        }
        public void Get(ref char field, string keyName)
        {
            field = GetChar(keyName);
        }
        public Vector3 GetVector3(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key))
            {
                return Vector3.zero;
            }
            try
            {
                int _position = 0;
                byte[] _data = GetLineBytes(key);
                float x = BitConverter.ToSingle(_data, _position);
                _position += 4;
                float y = BitConverter.ToSingle(_data, _position);
                _position += 4;
                float z = BitConverter.ToSingle(_data, _position);
                _position += 4;
                var vector = new Vector3(x, y, z);
                ////Debug.Log("GetVector3 " + vector);
                return vector;
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetVector3 " + e.Message);
                return Vector3.zero;
            }
        }
        public List<Vector3> GetVector3List(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key)) return new List<Vector3>();
            try
            {
                int _position = 0;
                byte[] _data = GetLineBytes(key);
                ushort size = BitConverter.ToUInt16(_data, _position);
                // this.LGDev("GetVector3List size " + size);
                _position += 2;
                var arr = new List<Vector3>();
                for (int i = 0; i < size; i++)
                {
                    float x = BitConverter.ToSingle(_data, _position);
                    _position += 4;
                    float y = BitConverter.ToSingle(_data, _position);
                    _position += 4;
                    float z = BitConverter.ToSingle(_data, _position);
                    _position += 4;
                    var vec = new Vector3(x, y, z);
                    arr.Add(vec);
                    // this.LGDev("GetVector3List vec " + vec);
                }
                return arr;
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetBoolList " + e.Message);
                return new List<Vector3>();
            }
        }
        public Vector2 GetVector2(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key))
            {
                return Vector2.zero;
            }
            try
            {
                int _position = 0;
                byte[] _data = GetLineBytes(key);
                float x = BitConverter.ToSingle(_data, _position);
                _position += 4;
                float y = BitConverter.ToSingle(_data, _position);
                _position += 4;
                return new Vector2(x, y);
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetVector2 " + e.Message);
                return Vector2.zero;
            }
        }

        public List<Vector2> GetVector2List(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key)) return new List<Vector2>();
            try
            {
                int _position = 0;
                byte[] _data = GetLineBytes(key);
                ushort size = BitConverter.ToUInt16(_data, _position);
                // this.LGDev("GetVector3List size " + size);
                _position += 2;
                var arr = new List<Vector2>();
                for (int i = 0; i < size; i++)
                {
                    float x = BitConverter.ToSingle(_data, _position);
                    _position += 4;
                    float y = BitConverter.ToSingle(_data, _position);
                    _position += 4;
                    var vec = new Vector2(x, y);
                    arr.Add(vec);
                    // this.LGDev("GetVector3List vec " + vec);
                }
                return arr;
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetBoolList " + e.Message);
                return new List<Vector2>();
            }
        }
        public Quaternion GetQuaternion(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key)) return Quaternion.identity;
            try
            {
                int _position = 0;
                byte[] _data = GetLineBytes(key);
                float x = BitConverter.ToSingle(_data, _position);
                _position += 4;
                float y = BitConverter.ToSingle(_data, _position);
                _position += 4;
                float z = BitConverter.ToSingle(_data, _position);
                _position += 4;
                return Quaternion.Euler(x, y, z);
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetQuaternion " + e.Message);
                return Quaternion.identity;
            }
        }
        public byte GetByte(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key)) return new byte();
            try
            {
                byte[] _data = GetLineBytes(key);
                byte res = _data[0];
                return res;
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetByte " + e.Message);
                return new byte();
            }
        }
        public sbyte GetSByte(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key)) return new sbyte();
            try
            {
                byte[] _data = GetLineBytes(key);
                var b = (sbyte)_data[0];
                return b;
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetByte " + e.Message);
                return new sbyte();
            }
        }
        public bool[] GetBoolArray(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key)) return new bool[0];
            try
            {
                int _position = 0;
                byte[] _data = lines[key];
                ushort size = BitConverter.ToUInt16(_data, _position);
                _position += 2;
                var arr = new bool[size];
                Buffer.BlockCopy(_data, _position, arr, 0, size);
                return arr;
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetBoolArray " + e.Message);
                return new bool[0];
            }
        }
        public List<bool> GetBoolList(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key)) return new List<bool>();
            try
            {
                int _position = 0;
                byte[] _data = lines[key];
                ushort size = BitConverter.ToUInt16(_data, _position);
                _position += 2;
                var arr = new bool[size];
                Buffer.BlockCopy(_data, _position, arr, 0, size);
                return new List<bool>(arr);
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetBoolList " + e.Message);
                return new List<bool>();
            }
        }
        public ushort[] GetUShortArray(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key)) return new ushort[0];
            try
            {
                int _position = 0;
                byte[] _data = lines[key];
                int size = BitConverter.ToInt32(_data, _position);
                _position += 4;
                var arr = new ushort[size];
                for (int i = 0; i < size; i++)
                {
                    arr[i] = BitConverter.ToUInt16(_data, _position);
                    _position += 2;
                }
                return arr;
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetUShortArray " + e.Message);
                return new ushort[0];
            }
        }
        public List<ushort> GetUShortList(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key)) return new List<ushort>();
            try
            {
                int _position = 0;
                byte[] _data = lines[key];
                int size = BitConverter.ToInt16(_data, _position);
                _position += 2;
                var arr = new List<ushort>();
                for (int i = 0; i < size; i++)
                {
                    arr.Add(BitConverter.ToUInt16(_data, _position));
                    _position += 2;
                }
                return arr;
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetUShortList " + e.Message);
                return new List<ushort>();
            }
        }
        public short[] GetShortArray(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key)) return new short[0];
            try
            {
                int _position = 0;
                byte[] _data = lines[key];
                ushort size = BitConverter.ToUInt16(_data, _position);
                _position += 2;
                var arr = new short[size];
                Buffer.BlockCopy(_data, _position, arr, 0, size * 2);
                _position += size * 2;
                return arr;
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetShortArray " + e.Message);
                return new short[0];
            }
        }
        public List<short> GetShortList(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key)) return new List<short>();
            try
            {
                int _position = 0;
                byte[] _data = lines[key];
                ushort size = BitConverter.ToUInt16(_data, _position);
                _position += 2;
                var arr = new List<short>();
                for (int i = 0; i < size; i++)
                {
                    arr.Add(BitConverter.ToInt16(_data, _position));
                    ////Debug.Log("List<short> GetShortList " + arr[i]);
                    _position += 2;
                }
                return arr;
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetShortList " + e.Message);
                return new List<short>();
            }
        }
        public long[] GetLongArray(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key)) return new long[0];
            try
            {
                int _position = 0;
                byte[] _data = lines[key];
                ushort size = BitConverter.ToUInt16(_data, _position);
                _position += 2;
                var arr = new long[size];
                Buffer.BlockCopy(_data, _position, arr, 0, size * 8);
                _position += size * 8;
                return arr;
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetLongArray " + e.Message);
                return new long[0];
            }
        }
        public List<long> GetLongList(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key)) return new List<long>();
            try
            {
                int _position = 0;
                byte[] _data = lines[key];
                ushort size = BitConverter.ToUInt16(_data, _position);
                _position += 2;
                var arr = new long[size];
                Buffer.BlockCopy(_data, _position, arr, 0, size * 8);
                _position += size * 8;
                return new List<long>(arr);
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetLongList " + e.Message);
                return new List<long>();
            }
        }
        public ulong[] GetULongArray(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key)) return new ulong[0];
            try
            {
                int _position = 0;
                byte[] _data = lines[key];
                ushort size = BitConverter.ToUInt16(_data, _position);
                _position += 2;
                var arr = new ulong[size];
                Buffer.BlockCopy(_data, _position, arr, 0, size * 8);
                _position += size * 8;
                return arr;
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetULongArray " + e.Message);
                return new ulong[0];
            }
        }
        public List<ulong> GetULongList(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key)) return new List<ulong>();
            try
            {
                int _position = 0;
                byte[] _data = lines[key];
                ushort size = BitConverter.ToUInt16(_data, _position);
                _position += 2;
                var arr = new ulong[size];
                Buffer.BlockCopy(_data, _position, arr, 0, size * 8);
                _position += size * 8;
                return new List<ulong>(arr);
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetULongList " + e.Message);
                return new List<ulong>();
            }
        }
        public int[] GetIntArray(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key)) return new int[0];
            try
            {
                int _position = 0;
                byte[] _data = lines[key];
                ushort size = BitConverter.ToUInt16(_data, _position);
                _position += 2;
                var arr = new int[size];
                Buffer.BlockCopy(_data, _position, arr, 0, size * 4);
                _position += size * 4;
                return arr;
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetIntArray " + e.Message);
                return new int[0];
            }
        }
        public List<int> GetIntList(string keyName)
        {
            var key = GetKeyID(keyName);
            //this.LGDev(key + " GetIntList ");
            if (CheckID(key)) return new List<int>();
            try
            {
                int _position = 0;
                byte[] _data = lines[key];
                ushort size = BitConverter.ToUInt16(_data, _position);
                _position += 2;
                var arr = new List<int>();
                for (int i = 0; i < size; i++)
                {
                    int value = BitConverter.ToInt32(_data, _position);
                    _position += 4;
                    arr.Add(value);
                }
                return arr;
            }
            catch (Exception e)
            {
                ////Debug.LogError(key + " GetIntList " + e.Message);
                return new List<int>();
            }
        }
        public uint[] GetUIntArray(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key)) return new uint[0];
            try
            {
                int _position = 0;
                byte[] _data = lines[key];
                ushort size = BitConverter.ToUInt16(_data, _position);
                _position += 2;
                var arr = new uint[size];
                Buffer.BlockCopy(_data, _position, arr, 0, size * 4);
                _position += size * 4;
                return arr;
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetUIntArray " + e.Message);
                return new uint[0];
            }
        }
        public List<uint> GetUIntList(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key)) return new List<uint>();
            try
            {
                int _position = 0;
                byte[] _data = lines[key];
                ushort size = BitConverter.ToUInt16(_data, _position);
                _position += 2;
                var arr = new uint[size];
                Buffer.BlockCopy(_data, _position, arr, 0, size * 4);
                _position += size * 4;
                return new List<uint>(arr);
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetUIntList " + e.Message);
                return new List<uint>();
            }
        }
        public byte[] GetByteArray(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key)) return new byte[0];
            try
            {
                int _position = 0;
                return lines[key];
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetByteArray " + e.Message);
                return new byte[0];
            }
        }
        public List<byte> GetByteList(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key)) return new List<byte>();
            try
            {
                int _position = 0;
                byte[] _data = lines[key];
                int size = BitConverter.ToInt32(_data, _position);
                _position += 4;
                var arr = new List<byte>();
                for (int i = 0; i < size; i++)
                {
                    arr.Add(_data[_position]);
                    _position += 1;
                }
                return arr;
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetByteList " + e.Message);
                return new List<byte>();
            }
        }
        public byte[,] GetByte2dArray(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key)) return new byte[0, 0];
            try
            {
                int _position = 0;
                byte[] _data = lines[key];
                int sizex = BitConverter.ToInt32(_data, _position);
                _position += 4;
                int sizey = BitConverter.ToInt32(_data, _position);
                _position += 4;
                var arr = new byte[sizex, sizey];
                for (int x = 0; x < sizex; x++)
                {
                    for (int y = 0; y < sizey; y++)
                    {
                        arr[x, y] = _data[_position];
                        _position += 1;
                    }
                }
                return arr;
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetByteList " + e.Message);
                return new byte[0, 0];
            }
        }
        public List<byte[]> GetByteArrayList(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key)) return new List<byte[]>();
            try
            {
                int _position = 0;
                byte[] _data = lines[key];
                int size = BitConverter.ToInt16(_data, _position);
                _position += 2;
                var arr = new List<byte[]>();
                for (int i = 0; i < size; i++)
                {
                    int len = BitConverter.ToInt16(_data, _position);
                    _position += 2;
                    byte[] bytes = new byte[len];
                    Buffer.BlockCopy(_data, _position, bytes, 0, len);
                    _position += len;
                    arr.Add(bytes);
                }
                //this.LGDev("GetByteArrayList " + _data.Length);
                return arr;
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetByteArrayList " + e.Message);
                return new List<byte[]>();
            }
        }
        public float[] GetFloatArray(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key)) return new float[0];
            try
            {
                int _position = 0;
                byte[] _data = lines[key];
                int size = BitConverter.ToInt32(_data, _position);
                _position += 4;
                var arr = new float[size];
                for (int i = 0; i < size; i++)
                {
                    arr[i] = BitConverter.ToSingle(_data, _position);
                    _position += 4;
                }
                return arr;
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetByteList " + e.Message);
                return new float[0];
            }
        }
        public List<float> GetFloatList(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key)) return new List<float>();
            try
            {
                int _position = 0;
                byte[] _data = lines[key];
                int size = BitConverter.ToInt32(_data, _position);
                _position += 4;
                var arr = new List<float>();
                for (int i = 0; i < size; i++)
                {
                    arr.Add(BitConverter.ToSingle(_data, _position));
                    _position += 4;
                }
                return arr;
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetFloatList " + e.Message);
                return new List<float>();
            }
        }
        public double[] GetDoubleArray(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key)) return new double[0];
            try
            {
                int _position = 0;
                byte[] _data = lines[key];
                ushort size = BitConverter.ToUInt16(_data, _position);
                _position += 2;
                var arr = new double[size];
                Buffer.BlockCopy(_data, _position, arr, 0, size * 8);
                _position += size * 8;
                return arr;
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetDoubleArray " + e.Message);
                return new double[0];
            }
        }
        public List<double> GetDoubleList(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key)) return new List<double>();
            try
            {
                int _position = 0;
                byte[] _data = lines[key];
                ushort size = BitConverter.ToUInt16(_data, _position);
                _position += 2;
                var arr = new double[size];
                Buffer.BlockCopy(_data, _position, arr, 0, size * 8);
                _position += size * 8;
                return new List<double>(arr);
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetDoubleList " + e.Message);
                return new List<double>();
            }
        }
        public string[] GetStringArray(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key))
            {
                return new string[0];
            }
            try
            {
                int _position = 0;
                byte[] _data = lines[key];
                ushort length = BitConverter.ToUInt16(_data, _position);
                _position += 2;
                var arr = new string[length];
                //this.LGDev("GetStringArray length " + length);
                for (int i = 0; i < length; i++)
                {
                    ushort size = BitConverter.ToUInt16(_data, _position);
                    _position += 2;
                    string result = Encoding.UTF8.GetString(_data, _position, size);
                    _position += size;
                    arr[i] = result;
                }
                return arr;
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetStringArray " + e.Message);
                return new string[0];
            }
        }
        public List<string> GetStringList(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key))
            {
                return new List<string>();
            }
            try
            {
                int _position = 0;
                byte[] _data = lines[key];
                ushort length = BitConverter.ToUInt16(_data, _position);
                _position += 2;
                var arr = new string[length];
                for (int i = 0; i < length; i++)
                {
                    ushort size = BitConverter.ToUInt16(_data, _position);
                    _position += 2;
                    string result = Encoding.UTF8.GetString(_data, _position, size);
                    _position += size;
                    arr[i] = result;
                }
                return new List<string>(arr);
            }
            catch (Exception e)
            {
                ////Debug.LogError("Deserialize GetStringList error " + e.Message);
                return new List<string>();
            }
        }
        public List<Color> GetColorList(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key)) return new List<Color>();
            try
            {
                int _position = 0;
                byte[] _data = lines[key];
                ushort size = BitConverter.ToUInt16(_data, _position);
                _position += 2;
                var arr = new Color[size];
                for (int i = 0; i < size; i++)
                {
                    var r = BitConverter.ToSingle(_data, _position);
                    _position += 4;
                    var g = BitConverter.ToSingle(_data, _position);
                    //this.LGDev("GetColorList 1" + g);
                    _position += 4;
                    var b = BitConverter.ToSingle(_data, _position);
                    _position += 4;
                    arr[i] = new Color(r, g, b);
                    //this.LGDev("GetColorList 123" + arr[i]);
                }
                return new List<Color>(arr);
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetColorList " + e.Message);
                return new List<Color>();
            }
        }
        public Color GetColor(string keyName)
        {
            var key = GetKeyID(keyName);
            int _position = 0;
            byte[] _data = GetLineBytes(key);
            if (CheckID(key)) return Color.white;
            try
            {
                var x = BitConverter.ToSingle(_data, _position);
                _position += 4;
                var y = BitConverter.ToSingle(_data, _position);
                _position += 4;
                var z = BitConverter.ToSingle(_data, _position);
                _position += 4;
                return new Color(x, y, z);
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetColor " + e.Message);
                return Color.white;
            }
        }
        public bool GetBool(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key)) return false;
            try
            {
                int _position = 0;
                byte[] _data = GetLineBytes(key);
                bool res = _data[_position] > 0;
                _position += 1;
                return res;
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetBool " + e.Message);
                return false;
            }
        }
        public char GetChar(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key)) return new char();
            try
            {
                int _position = 0;
                byte[] _data = GetLineBytes(key);
                char result = BitConverter.ToChar(_data, _position);
                _position += 2;
                return result;
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetChar " + e.Message);
                return new char();
            }
        }
        public ushort GetUShort(string keyName, ushort defaultValue = 0)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key)) return defaultValue;
            try
            {
                int _position = 0;
                byte[] _data = GetLineBytes(key);
                ushort result = BitConverter.ToUInt16(_data, _position);
                _position += 2;
                return result;
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetUShort " + e.Message);
                return defaultValue;
            }
        }
        public short GetShort(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key)) return 0;
            try
            {
                int _position = 0;
                byte[] _data = GetLineBytes(key);
                short result = BitConverter.ToInt16(_data, _position);
                _position += 2;
                return result;
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetShort " + e.Message);
                return new short();
            }
        }
        public long GetLong(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key)) return 0;
            try
            {
                int _position = 0;
                byte[] _data = GetLineBytes(key);
                long result = BitConverter.ToInt64(_data, _position);
                _position += 8;
                return result;
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetLong " + e.Message);
                return new long();
            }
        }
        public ulong GetULong(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key)) return 0;
            try
            {
                byte[] _data = GetLineBytes(key);
                ulong result = BitConverter.ToUInt64(_data, 0);
                return result;
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetULong " + e.Message);
                return new ulong();
            }
        }
        public uint GetUInt(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key)) return 0;
            try
            {
                byte[] _data = GetLineBytes(key);
                uint result = BitConverter.ToUInt32(_data, 0);
                return result;
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetUInt " + e.Message);
                return new uint();
            }
        }
        public float GetFloat(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key)) return 0;
            try
            {
                byte[] _data = GetLineBytes(key);
                float result = BitConverter.ToSingle(_data, 0);
                return result;
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetFloat " + e.Message);
                return new float();
            }
        }
        public double GetDouble(string keyName)
        {
            var key = GetKeyID(keyName);
            if (CheckID(key)) return 0;
            try
            {
                byte[] _data = GetLineBytes(key);
                double result = BitConverter.ToDouble(_data, 0);
                return result;
            }
            catch (Exception e)
            {
                ////Debug.LogError("GetDouble " + e.Message);
                return new double();
            }
        }
        public void Get<T>(ref T[] objs, string keyName) where T : ISaveSerializable, new()
        {
            var key = GetKeyID(keyName);
            if (CheckID(key))
            {
                objs = new T[0];
                return;
            }
            try
            {
                var position = 0;
                var ps = new PackageReader();

                byte[] _data = lines[key];
                ushort count = 0;
                try
                {
                    count = BitConverter.ToUInt16(_data, position);
                }
                catch (Exception e)
                {
                    //this.LGDev(key + " GetList count error " + _data.Length);
                }
                position += 2;
                var list = new T[count];
                for (int i = 0; i < count; i++)
                {
                    var size = BitConverter.ToInt32(_data, position);
                    position += 4;
                    var obj = new T();
                    if (size == 0)
                    {
                        list[i] = (obj);
                        continue;
                    }
                    byte[] _bytes = new byte[size];
                    //this.LGDev(typeof(T).Name + "GetList size " + size);
                    Buffer.BlockCopy(_data, position, _bytes, 0, size);
                    position += size;
                    ps.SetSource(_bytes);
                    try
                    {
                        obj.Deserialize(ps);
                    }
                    catch (Exception f)
                    {
                        ////Debug.LogError(typeof(T).Name + " - " + key + " Deserialize error count " + i + " " + f.Message);
                    }
                    list[i] = (obj);
                }
                objs = list;
                return;
            }
            catch (Exception e)
            {
                ////Debug.LogError(key + " GetList " + typeof(T).Name + " " + e.Message);
                objs = new T[0];
                return;
            }
        }
        public void Get<T>(ref List<T> objs, string keyName) where T : ISaveSerializable, new()
        {
            var key = GetKeyID(keyName);
            if (CheckID(key))
            {
                objs = new List<T>();
                return;
            }
            try
            {
                var position = 0;
                var ps = new PackageReader();

                byte[] _data = lines[key];
                ushort count = 0;
                try
                {
                    count = BitConverter.ToUInt16(_data, position);
                }
                catch (Exception e)
                {
                    Debug.LogError(key + $" GetList count error {_data.Length } " + e.Message);
                }
                position += 2;
                var list = new List<T>();
                for (int i = 0; i < count; i++)
                {
                    var size = BitConverter.ToInt32(_data, position);
                    position += 4;
                    var obj = new T();
                    if (size == 0)
                    {
                        list.Add(obj);
                        continue;
                    }
                    byte[] _bytes = new byte[size];
                    //this.LGDev(typeof(T).Name + "GetList size " + size);
                    Buffer.BlockCopy(_data, position, _bytes, 0, size);
                    position += size;
                    ps.SetSource(_bytes);
                    try
                    {
                        Debug.Log(typeof(T).Name+" Deserialize "+ps.GetLineCount);
                        obj.Deserialize(ps);
                    }
                    catch (Exception f)
                    {
                        Debug.LogError(typeof(T).Name + " - " + key + " Deserialize error count " + i + " " + f.Message);
                    }
                    list.Add(obj);
                }
                objs = list;
                return;
            }
            catch (Exception e)
            {
                ////Debug.LogError(key + " GetList " + typeof(T).Name + " " + e.Message);
                objs = new List<T>();
                return;
            }
        }
        public void GetEnum<T>(ref T obj, string keyName) where T : Enum
        {
            //Debug.Log("GetEnum");
            var key = GetKeyID(keyName);
            try
            {
                if (CheckID(key))
                {
                    obj = default(T);
                    return;
                }
                byte[] _data = GetLineBytes(key);
                int value = BitConverter.ToInt32(_data, 0);
                obj = (T)Enum.ToObject(typeof(T), value);
                return;
            }
            catch (Exception e)
            {
                //Debug.LogError(key + " GetEnum " + typeof(T) + " " + e.Message);
                obj = default(T);
                return;
            }
        }
    }
    public class DataWriter
    {
        private byte[] m_buffer;
        private int m_position;

        public DataWriter()
        {

        }

        internal byte[] GetBytes<T>(T obj) where T : INetSerializable
        {
            m_buffer = SerializerPool.RentBytes(1024 * 4);
            m_position = 0;
            obj.Serialize(this);
            //CalculateFields(data, typeof(T));  reflection
            var bytes = SerializerPool.RentBytes(m_position);
            Array.Copy(m_buffer, 0, bytes, 0, bytes.Length);
            SerializerPool.ReturnBytes(m_buffer);
            return bytes;
        }

        void CalculateFields(object data, Type type)
        {
            var fields = SerializerPool.GetFieldInfos(type);
            foreach (var field in fields)
            {
                var value = field.GetValue(data);
                //Debug.Log(field.FieldType + $" {field.FieldType.IsGenericType}  CalculateFields " + type.Name);
                if (field.FieldType == typeof(string))
                {
                    Put(value as string);
                }
                else if (field.FieldType == typeof(byte))
                {
                    Put((byte)value);
                }
                else if (field.FieldType == typeof(bool))
                {
                    Put((bool)value);
                }
                else if (field.FieldType == typeof(Int16))
                {
                    Put((short)value);
                }
                else if (field.FieldType == typeof(UInt16))
                {
                    Put((ushort)value);
                }
                else if (field.FieldType == typeof(Int32))
                {
                    Put((int)value);
                }
                else if (field.FieldType == typeof(UInt32))
                {
                    Put((uint)value);
                }
                else if (field.FieldType == typeof(Int64))
                {
                    Put((long)value);
                }
                else if (field.FieldType == typeof(UInt64))
                {
                    Put((ulong)value);
                }
                else if (field.FieldType == typeof(Single))
                {
                    Put((float)value);
                }
                else if (field.FieldType == typeof(Double))
                {
                    Put((double)value);
                }
                else if (field.FieldType == typeof(Vector3))
                {
                    Put((Vector3)value);
                }
                else if (field.FieldType == typeof(Vector2))
                {
                    Put((Vector2)value);
                }
                else if (field.FieldType.IsGenericType)
                {
                    //Debug.Log("Lista " + field.FieldType.GetGenericTypeDefinition());
                    if (field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        Type t = field.FieldType.GetGenericArguments().Single();
                        //Debug.Log("Lista typeof(List<>) " + t);
                        if (typeof(INetSerializable).IsAssignableFrom(t))
                        {
                            IEnumerable<object> l = value as IEnumerable<object>;
                            Put((ushort)l.Count());
                            for (int i = 0; i < l.Count(); i++)
                            {
                                CalculateFields(l.ElementAt(i), field.FieldType);
                            }
                        }
                        else
                        {
                            PutList(value, t);
                        }
                    }
                }
                else if (typeof(INetSerializable).IsAssignableFrom(field.FieldType))
                {
                    CalculateFields(value, field.FieldType);
                }
            }

        }
        #region PutLits
        void PutList(object value, Type type)
        {
            if (type == typeof(string))
            {
                IEnumerable<string> l = value as IEnumerable<string>;
                Put((ushort)l.Count());
                foreach (var item in l)
                {
                    Put(item);
                }
            }
            else if (type == typeof(bool))
            {

            }
            else if (type == typeof(byte))
            {
                IEnumerable<byte> l = value as IEnumerable<byte>;
                Put((ushort)l.Count());
                foreach (var item in l)
                {
                    Put(item);
                }
            }
            else if (type == typeof(UInt16))
            {

            }
            else if (type == typeof(Int16))
            {

            }
            else if (type == typeof(UInt32))
            {

            }
            else if (type == typeof(Int32))
            {

            }
            else if (type == typeof(UInt64))
            {

            }
            else if (type == typeof(Int64))
            {

            }
            else if (type == typeof(Single))
            {

            }
            else if (type == typeof(Double))
            {

            }
            else if (type == typeof(Vector2))
            {

            }
            else if (type == typeof(Vector3))
            {

            }
        }

        #endregion
        public void Put(IEnumerable<byte> vs)
        {
            if(vs == null)
                vs = new List<byte>();
            Put((ushort)vs.Count());
            foreach (var v in vs)
            {
                Put(v);
            }
        }
        public void Put(IEnumerable<int> vs)
        {
            if (vs == null)
                vs = new List<int>();
            Put((ushort)vs.Count());
            foreach (var v in vs)
            {
                Put(v);
            }
        }
        public void Put(IEnumerable<uint> vs)
        {
            if (vs == null)
                vs = new List<uint>();
            Put((ushort)vs.Count());
            foreach (var v in vs)
            {
                Put(v);
            }
        }
        public void Put<T>(IEnumerable<T> v) where T : INetSerializable, new()
        {
            int a = 0;
            try
            {
                if (v == null)
                {
                    Put((ushort)0);
                    return;
                }
                Put((ushort)v.Count());
                Debug.Log("put "+ v.Count());
                foreach (var obj in v)
                {
                    if (obj == null)
                    {
                        Debug.Log("obj == null "+a);
                        Put((short)-1);
                    }
                    else
                        obj.Serialize(this);
                    a++;
                }
            }
            catch (Exception e)
            {
                Debug.Log(a+" Exception "+e.StackTrace);
            }
        }
        public void Put<T>(T v) where T : INetSerializable, new()
        {
            if (v == null)
                v = new T();
            v.Serialize(this);
        }
        public void Put(string v)
        {
            if (v == null || v.Length == 0)
            {
                Put((ushort)0);
                ////Debug.Log("Put null string");
                return;
            }

            ushort bytesCount = (ushort)Encoding.UTF8.GetByteCount(v);
            Put(bytesCount);
            ////Debug.Log("Put string "+ bytesCount);

            //put string
            Encoding.UTF8.GetBytes(v, 0, v.Length, m_buffer, m_position);
            m_position += bytesCount;

        }
        public void Put(bool v)
        {
            m_buffer[m_position] = (byte)(v ? 1 : 0);
            m_position += 1;
        }
        public void Put(byte v)
        {
            m_buffer[m_position] = v;
            m_position += 1;
        }
        public void Put(ushort v)
        {
            FastBitConverter.GetBytes(m_buffer, m_position, v);
            m_position += 2;
        }
        public void Put(short v)
        {
            FastBitConverter.GetBytes(m_buffer, m_position, v);
            m_position += 2;
        }
        public void Put(uint v)
        {
            FastBitConverter.GetBytes(m_buffer, m_position, v);
            m_position += 4;
        }
        public void Put(int v)
        {
            //Debug.Log(m_buffer.Length+" Put "+ m_position);
            FastBitConverter.GetBytes(m_buffer, m_position, v);
            m_position += 4;
        }
        public void Put(float v)
        {
            FastBitConverter.GetBytes(m_buffer, m_position, v);
            m_position += 4;
        }
        public void Put(double v)
        {
            FastBitConverter.GetBytes(m_buffer, m_position, v);
            m_position += 8;
        }
        public void Put(long v)
        {
            FastBitConverter.GetBytes(m_buffer, m_position, v);
            m_position += 8;
        }
        public void Put(ulong v)
        {
            FastBitConverter.GetBytes(m_buffer, m_position, v);
            m_position += 8;
        }
        public void Put(Vector3 v)
        {
            Put(v.x);
            Put(v.y);
            Put(v.z);
        }
        public void Put(Vector2 v)
        {
            Put(v.x);
            Put(v.y);
        }
    }
    public class DataReader
    {
        private byte[] m_data;
        private int m_position;

        public void SetData(byte[] source, int offset = 0)
        {
            m_data = source;
            m_position = offset;
            Debug.Log("SetData "+ offset);
            //SetFieldsFields(ref obj, type);
        }
        void SetFieldsFields(ref object data, Type type)
        {
            var fields = SerializerPool.GetFieldInfos(type);
            foreach (var field in fields)
            {
                object value = field.GetValue(data);
                //Debug.Log(field.FieldType + $"  SetFieldsFields " + type.Name);
                if (field.FieldType == typeof(string))
                {
                    var r = (string)value;
                    Get(ref r);
                    field.SetValue(data, r);
                }
                else if (field.FieldType == typeof(byte))
                {
                    var r = (byte)value;
                    Get(ref r);
                    field.SetValue(data, r);
                }
                else if (field.FieldType == typeof(bool))
                {
                    var r = (bool)value;
                    Get(ref r);
                    field.SetValue(data, r);
                }
                else if (field.FieldType == typeof(Int16))
                {
                    var r = (short)value;
                    Get(ref r);
                    field.SetValue(data, r);
                }
                else if (field.FieldType == typeof(Int32))
                {
                    var r = (int)value;
                    Get(ref r);
                    field.SetValue(data, r);
                }
                else if (field.FieldType == typeof(Int64))
                {
                    var r = (long)value;
                    Get(ref r);
                    field.SetValue(data, r);
                }
                else if (field.FieldType == typeof(UInt16))
                {
                    var r = (ushort)value;
                    Get(ref r);
                    field.SetValue(data, r);
                }
                else if (field.FieldType == typeof(UInt32))
                {
                    var r = (uint)value;
                    Get(ref r);
                    field.SetValue(data, r);
                }
                else if (field.FieldType == typeof(UInt64))
                {
                    var r = (ulong)value;
                    Get(ref r);
                    field.SetValue(data, r);
                }
                else if (field.FieldType == typeof(Single))
                {
                    var r = (float)value;
                    Get(ref r);
                    field.SetValue(data, r);
                }
                else if (field.FieldType == typeof(Double))
                {
                    var r = (double)value;
                    Get(ref r);
                    field.SetValue(data, r);
                }
                else if (field.FieldType == typeof(Vector3))
                {
                    var r = (Vector3)value;
                    Get(ref r);
                    field.SetValue(data, r);
                }
                else if (field.FieldType == typeof(Vector2))
                {
                    var r = (Vector2)value;
                    Get(ref r);
                    field.SetValue(data, r);
                }
                else if (field.FieldType.IsGenericType)
                {
                    //Debug.Log("Lista " + field.FieldType.GetGenericTypeDefinition());
                    if (field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        //Debug.Log("Lista typeof(List<>) " + field.FieldType.GetGenericArguments().Single());
                        //Type itemType = type.;
                        GetList(ref value, field.FieldType.GetGenericArguments().Single());

                        field.SetValue(data, value);
                    }
                }
                else if (typeof(INetSerializable).IsAssignableFrom(field.FieldType))
                {
                    object output = Activator.CreateInstance(field.FieldType);
                    SetFieldsFields(ref output, field.FieldType);
                    field.SetValue(data, output);
                }
            }
        }
        public void GetList(ref object r, Type type)
        {
            if (type == typeof(string))
            {
                ushort size = 0;
                Get(ref size);
                List<string> l = new List<string>();
                for (int i = 0; i < size; i++)
                {
                    string v = "";
                    Get(ref v);
                    l.Add(v);
                }
                r = l;
            }
            else if (type == typeof(bool))
            {

            }
            else if (type == typeof(byte))
            {
                ushort size = 0;
                Get(ref size);
                List<byte> l = new List<byte>();
                //Debug.Log("List size " + size);
                for (int i = 0; i < size; i++)
                {
                    byte v = 0;
                    Get(ref v);
                    l.Add(v);
                    //Debug.Log("List byte value " + v);
                }
                r = l;
            }
            else if (type == typeof(UInt16))
            {

            }
            else if (type == typeof(Int16))
            {

            }
            else if (type == typeof(UInt32))
            {

            }
            else if (type == typeof(Int32))
            {

            }
            else if (type == typeof(UInt64))
            {

            }
            else if (type == typeof(Int64))
            {

            }
            else if (type == typeof(Single))
            {

            }
            else if (type == typeof(Double))
            {

            }
            else if (type == typeof(Vector2))
            {

            }
            else if (type == typeof(Vector3))
            {

            }
        }
        bool IsEmpty()
        {
            Debug.Log(m_data.Length+" IsEmpty "+ m_position);
            if (m_position >= m_data.Length)
                return true;
            var r = BitConverter.ToInt16(m_data, m_position) == -1;
            if (r)
                m_position += 2;
            return r;
        }
        public void Get(ref byte[] r)
        {
            try
            {
                ushort size = 0;
                Get(ref size);
                r = new byte[size];
                for (int i = 0; i < size; i++)
                {
                    byte v = 0;
                    Get(ref v);
                    r[i]=v;
                }
            }
            catch (Exception e)
            {
                ////Debug.LogError(key + " GetIntList " + e.Message);
                r = new byte[0];
            }
        }
        public void Get(ref List<byte> r)
        {
            try
            {
                ushort size = 0;
                Get(ref size);
                r = new List<byte>();
                for (int i = 0; i < size; i++)
                {
                    byte v = 0;
                    Get(ref v);
                    r.Add(v);
                }
            }
            catch (Exception e)
            {
                ////Debug.LogError(key + " GetIntList " + e.Message);
                r = new List<byte>();
            }
        }
        public void Get(ref List<int> r)
        {
            try
            {
                ushort size = 0;
                Get(ref size);
                r = new List<int>();
                for (int i = 0; i < size; i++)
                {
                    int v = 0;
                    Get(ref v);
                    r.Add(v);
                }
            }
            catch (Exception e)
            {
                ////Debug.LogError(key + " GetIntList " + e.Message);
                r = new List<int>();
            }
        }
        public void Get(ref List<uint> r)
        {
            try
            {
                ushort size = 0;
                Get(ref size);
                r = new List<uint>();
                for (int i = 0; i < size; i++)
                {
                    uint v = 0;
                    Get(ref v);
                    r.Add(v);
                }
            }
            catch (Exception e)
            {
                ////Debug.LogError(key + " GetIntList " + e.Message);
                r = new List<uint>();
            }
        }
        public void Get<T>(ref T[] r) where T : INetSerializable, new()
        {
            ushort size = 0;
            Get(ref size);
            var list = new T[size];
            for (int i = 0; i < size; i++)
            {
                var obj = new T();
                if (!IsEmpty())
                    obj.Deserialize(this);
                list[i] = (obj);
            }
            r = list;
        }
        public void Get<T>(ref List<T> r) where T : INetSerializable, new()
        {
            ushort size = 0;
            Get(ref size);
            Debug.Log(typeof(T)+" Get size "+size);
            var list = new List<T>();
            for (int i = 0; i < size; i++)
            {
                var obj = new T();
                if (!IsEmpty())
                    obj.Deserialize(this);
                list.Add(obj);
            }
            r = list;
        }
        public void Get<T>(ref T r) where T : INetSerializable, new()
        {
            var obj = new T();
            obj.Deserialize(this);
            r = obj;
        }
        public void Get(ref string r)
        {
            ushort size = 0;
            Get(ref size);
            ////Debug.Log("Get string size " + size);
            if (size <= 0)
            {
                r = string.Empty;
                return;
            }
            r = Encoding.UTF8.GetString(m_data, m_position, size);
            ////Debug.Log("Get string r " + r);
            m_position += size;
        }
        public void Get(ref bool r)
        {
            try
            {
                r = m_data[m_position] == 1;
                m_position += 1;
            }
            catch
            {
                r = false;
            }
        }
        public void Get(ref byte r)
        {
            try
            {
                r = m_data[m_position];
                m_position += 1;
            }
            catch
            {
                r = 0;
            }
        }
        public void Get(ref ushort r)
        {
            try
            {
                r = BitConverter.ToUInt16(m_data, m_position);
                ////Debug.Log(r + " Get(ref ushort " + BitConverter.ToString(m_data));
                m_position += 2;
            }
            catch
            {
                r = 0;
            }
        }
        public void Get(ref short r)
        {
            try
            {
                r = BitConverter.ToInt16(m_data, m_position);
                m_position += 2;
            }
            catch
            {
                r = 0;
            }
        }
        public void Get(ref uint r)
        {
            try
            {
                r = BitConverter.ToUInt32(m_data, m_position);
                m_position += 4;
            }
            catch
            {
                r = 0;
            }
        }
        public void Get(ref int r)
        {
            try
            {
                r = BitConverter.ToInt32(m_data, m_position);
                m_position += 4;
            }
            catch
            {
                r = 0;
            }
        }
        public void Get(ref ulong r)
        {
            try
            {
                r = BitConverter.ToUInt64(m_data, m_position);
                m_position += 8;
            }
            catch
            {
                r = 0;
            }
        }
        public void Get(ref long r)
        {
            try
            {
                r = BitConverter.ToInt64(m_data, m_position);
                m_position += 8;
            }
            catch
            {
                r = 0;
            }
        }
        public void Get(ref Double r)
        {
            try
            {
                r = BitConverter.ToDouble(m_data, m_position);
                m_position += 8;
            }
            catch
            {
                r = 0;
            }
        }
        public void Get(ref float r)
        {
            try
            {
                r = BitConverter.ToSingle(m_data, m_position);
                m_position += 4;
            }
            catch
            {
                r = 0;
            }
        }
        public void Get(ref Vector3 r)
        {
            try
            {
                float x = 0, y = 0, z = 0;
                Get(ref x);
                Get(ref y);
                Get(ref z);
                r.x = x;
                r.y = y;
                r.z = z;
            }
            catch
            {
                r = new Vector3();
            }
        }
        public void Get(ref Vector2 r)
        {
            try
            {
                float x = 0, y = 0;
                Get(ref x);
                Get(ref y);
                r.x = x;
                r.y = y;
            }
            catch
            {
                r = new Vector2();
            }
        }
    }
    public interface INetSerializable
    {
        void Deserialize(DataReader reader);

        void Serialize(DataWriter write);
    }
    public interface ISaveSerializable
    {
        void Deserialize(PackageReader reader);

        void Serialize(PackageWriter write);
    }
}