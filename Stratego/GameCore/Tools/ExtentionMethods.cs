﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using GameCore;

namespace GameCore
{
    public static class ExtentionMethods
    {
        
        public static T TryIndex<T>(this T[,] array, int a, int b, out bool success) where T : class
        {
            var result = default(T);
            success = false;

            if (array != null && a < array.GetLength(0) && b < array.GetLength(0))
            {
                result = array.GetValue(a, b) as T;
                success = true;
            }

            return result;
        }

        public static T TryIndex<T>(this T[,] array, int a, int b) where T : class
        {
            bool success;
            return TryIndex(array, a, b, out success);
        }


        public static string ToReadable(this TimeSpan t)
        {
            string answer = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                t.Hours,
                t.Minutes,
                t.Seconds,
                t.Milliseconds);
            return answer;
        }

        public static T GetRandom<T>(this T collection) where T : IEnumerable
        {
            throw new NotImplementedException();
        }

        [Obsolete("Not practical", true)]
        public static T DeepClone<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }
    }
}
