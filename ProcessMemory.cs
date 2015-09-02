using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
namespace InjectCS
{


    public static class ProcessMemory
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool VirtualProtect(IntPtr address, uint size, uint newProtect, out uint oldProtect);

        public static void Read(IntPtr address,out byte[] data, int length)
        {
            var buffer = new byte[length];
            Marshal.Copy(address, buffer, 0, length);
            data = buffer;
        }

        public static void Write(IntPtr address, byte[] buffer)
        {
            uint oldProtect;
            VirtualProtect(address, (uint)buffer.Length, 0x40, out oldProtect);
            Marshal.Copy(buffer, 0, address, buffer.Length);
            VirtualProtect(address, (uint)buffer.Length, oldProtect, out oldProtect);
        }
        #region Write
        public static void Write<T>(IntPtr address, T value, int offset, int length) where T : struct
        {
            byte[] data = TToBytes<T>(value);
            Write(address, data, offset, length);
        }
        public static void Write(IntPtr address, byte[] data, int offset, int length)
        {
            byte[] writeData = new byte[length];
            Array.Copy(data, offset, writeData, 0, writeData.Length);
            Write((IntPtr)(address.ToInt32() + offset), writeData);
        }
        public static void Write<T>(IntPtr address, T value) where T : struct
        {
            Write(address, TToBytes<T>(value));
        }
        public static void WriteString(IntPtr address, string text, Encoding encoding)
        {
            Write(address, encoding.GetBytes(text));
        } 
        #endregion

        #region READ
        /// <summary>
        /// Reads a string from memory using the given encoding
        /// </summary>
        /// <param name="address">The address of the string to read</param>
        /// <param name="length">The length of the string</param>
        /// <param name="encoding">The encoding of the string</param>
        /// <returns>The string read from memory</returns>
        public static string ReadString(IntPtr address, int length, Encoding encoding)
        {
            byte[] data;
            Read(address, out data, length);
            string text = encoding.GetString(data);
            if (text.Contains("\0"))
                text = text.Substring(0, text.IndexOf('\0'));
            return text;
            //return encoding.GetString(data);
        }
        /// <summary>
        /// Generic function to read data from memory using the given type
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="address">The address to read data at</param>
        /// <param name="defVal">The default value of this operation (which is returned in case the Read-operation fails)</param>
        /// <returns>The value read from memory</returns>
        public static T Read<T>(IntPtr address, T defVal = default(T)) where T : struct
        {
            byte[] data;
            int size = Marshal.SizeOf(typeof(T));

            Read(address, out data, size);
            return BytesToT<T>(data, defVal);
        }
        /// <summary>
        /// Generic function to read an array of data from memory using the given type
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="address">The address to read data at</param>
        /// <param name="length">The number of elements to read</param>
        /// <returns></returns>
        public static T[] ReadArray<T>(IntPtr address, int length) where T : struct
        {
            byte[] data;
            int size = Marshal.SizeOf(typeof(T));

            Read(address, out data, size * length);
            T[] result = new T[length];
            for (int i = 0; i < length; i++)
                result[i] = BytesToT<T>(data, i * size);

            return result;
        }
        /// <summary>
        /// Generic function to read data from memory using the given type
        /// Applies the given offsets to read multilevel-pointers
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="address">The address to read data at</param>
        /// <param name="offsets">Array of offsets to apply</param>
        /// <returns></returns>
        public static T ReadMultilevelPointer<T>(IntPtr address, params int[] offsets) where T : struct
        {
            for (int i = 0; i < offsets.Length - 1; i++)
                address = Read<IntPtr>((IntPtr)(address.ToInt64() + offsets[i]));
            return Read<T>((IntPtr)(address.ToInt64() + offsets[offsets.Length - 1]), default(T));
        }
        ///// <summary>
        ///// Reads a matrix from memory
        ///// </summary>
        ///// <param name="address">The address of the matrix in memory</param>
        ///// <param name="rows">The number of rows of this matrix</param>
        ///// <param name="columns">The number of columns of this matrix</param>
        ///// <returns>The matrix read from memory</returns>
        //public static Matrix ReadMatrix(IntPtr address, int rows, int columns)
        //{
        //    Matrix matrix = new Matrix(rows, columns);
        //    byte[] data;
        //    Read(address, out data, SIZE_FLOAT * rows * columns);
        //    matrix.Read(data);

        //    return matrix;
        //}
        /// <summary>
        /// Generic function to read an array from memory using the given type and offsets.
        /// Offsets will be added to the address. (They will not be summed up but rather applied individually)
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="address">The address to read data at</param>
        /// <param name="offsets">Offsets that will be applied to the address</param>
        /// <returns></returns>
        public static T[] Read<T>(IntPtr address, params int[] offsets) where T : struct
        {
            T[] values = new T[offsets.Length];
            for (int i = 0; i < offsets.Length; i++)
                values[i] = Read<T>((IntPtr)(address.ToInt32() + offsets[i]));
            return values;
        }
        #endregion

        #region MARSHALLING
        /// <summary>
        /// Converts the given array of bytes to the specified type.
        /// Uses either marshalling or unsafe code, depending on UseUnsafeReadWrite
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="data">Array of bytes</param>
        /// <param name="defVal">The default value of this operation (which is returned in case the Read-operation fails)</param>
        /// <returns></returns>
        public static unsafe T BytesToT<T>(byte[] data, T defVal = default(T)) where T : struct
        {
            T structure = defVal;

            //if (UseUnsafeReadWrite)
            {
                fixed (byte* b = data)
                    structure = (T)Marshal.PtrToStructure((IntPtr)b, typeof(T));
            }
            //else
            //{
            //    GCHandle gcHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            //    structure = (T)Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof(T));
            //    gcHandle.Free();
            //}
            return structure;
        }
        /// <summary>
        /// Converts the given array of bytes to the specified type.
        /// Uses either marshalling or unsafe code, depending on UseUnsafeReadWrite
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="data">Array of bytes</param>
        /// <param name="index">Index of the data to convert</param>
        /// <param name="defVal">The default value of this operation (which is returned in case the Read-operation fails)</param>
        /// <returns></returns>
        public static unsafe T BytesToT<T>(byte[] data, int index, T defVal = default(T)) where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));
            byte[] tmp = new byte[size];
            Array.Copy(data, index, tmp, 0, size);
            return BytesToT<T>(tmp, defVal);
        }
        /// <summary>
        /// Converts the given struct to a byte-array
        /// </summary>
        /// <typeparam name="T">The type of the struct</typeparam>
        /// <param name="value">Value to conver to bytes</param>
        /// <returns></returns>
        public static unsafe byte[] TToBytes<T>(T value) where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));
            byte[] data = new byte[size];

            //if (UseUnsafeReadWrite)
            {
                fixed (byte* b = data)
                    Marshal.StructureToPtr(value, (IntPtr)b, true);
            }
            //else
            //{
            //    IntPtr ptr = Marshal.AllocHGlobal(size);
            //    Marshal.StructureToPtr(value, ptr, true);
            //    Marshal.Copy(ptr, data, 0, size);
            //    Marshal.FreeHGlobal(ptr);
            //}

            return data;
        }
        #endregion
    }
}
