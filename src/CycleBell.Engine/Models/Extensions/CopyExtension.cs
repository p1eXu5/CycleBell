
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace CycleBell.Engine.Models.Extensions
{
    public static class CopyExtension
    {
        /// <summary>
        /// Makes object deep copy through serialization to memory.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T DeepCopy<T>(this object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj), @"object cannot be null.");

            using (MemoryStream stream = new MemoryStream()) {

                BinaryFormatter formatter = new BinaryFormatter();

                formatter.Serialize(stream, obj);

                stream.Position = 0;

                return (T)formatter.Deserialize(stream);

            }

        }

    }
}
