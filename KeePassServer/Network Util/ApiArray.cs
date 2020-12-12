using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//======================================================================
//
//        filename : ApiArray.cs
//        description : This is a utility class used to combine, 
//                      slice arraries and add headers to arraries
//        created by Erni Gao at  Nov 2020
//   
//======================================================================


namespace KeePassServer.Network_Util
{
    class ApiArray
    {
        /// <summary>
        /// add one byte in front of byte array
        /// </summary>
        /// <param name="header">a byte functions as a header in online transmission</param>
        /// <param name="msg">msg in byte array</param>
        /// <returns>a byte array with one byte added in front</returns>
        public static byte[] addHeader(byte header, byte[] msg)
        {
            List<byte> list = new List<byte>();
            list.Add(header);
            list.AddRange(msg);
            return list.ToArray();
        }

        /// <summary>
        /// add two sequentail bytes in front of a byte array
        /// </summary>
        /// <param name="header1">one byte functions as a header in online transmission</param>
        /// <param name="header2">one byte functions as a header in online transmission</param>
        /// <param name="msg">msg in byte array</param>
        /// <returns>a byte array attached two bytes in front of it</returns>
        public static byte[] addHeader(byte header1, byte header2, byte[] msg)
        {
            List<byte> list = new List<byte>();
            list.Add(header1);
            list.Add(header2);
            list.AddRange(msg);
            return list.ToArray();
        }

        /// <summary>
        /// cut part of the array
        /// </summary>
        /// <param name="source">the array that will be cut</param>
        /// <param name="length">the length of data that will be discarded</param>
        /// <returns>array data left after slicing</returns>
        public static byte[] sliceArray(byte[] source, int len)
        {
            byte[] discardData;
            byte[] restData;
            splitArray(source, len, out discardData, out restData);
            
            return restData;
        }

        /// <summary>
        /// split one array into two based on the lenght of first destination array
        /// </summary>
        /// <param name="source">array that will be splitted</param>
        /// <param name="firstLength">length of the first destination array</param>
        /// <param name="array1">first destination array</param>
        /// <param name="array2">second destination array</param>
        public static void splitArray(byte[] source, int firstLength, out byte[] array1, out byte[] array2)
        {
            
            array1 = new byte[firstLength];
            array2 = new byte[source.Length - firstLength];
            Array.Copy(source, 0, array1, 0, array1.Length);
            Array.Copy(source, array1.Length, array2, 0, array2.Length);

        }

        /// <summary>
        /// concatenate two byte arrays into one 
        /// </summary>
        /// <param name="array1">a byte array with length at least 2</param>
        /// <param name="array2">another byte array with lenght at least 2</param>
        /// <returns>the long array after concatenation</returns>
        public static byte[] concatArray(byte[] array1, byte[] array2)
        {
            List<byte> list = new List<byte>();
            list.AddRange(array1);
            list.AddRange(array2);

            return list.ToArray();
        }
    }
}
