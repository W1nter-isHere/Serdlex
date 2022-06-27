using System;
using System.Collections.Generic;

namespace Utils
{
    public static class GlobalData
    {
        private static readonly Dictionary<string, object> Data;

        static GlobalData()
        {
            Data = new Dictionary<string, object>();
        }

        public static void Set(string key, object value)
        {
            Data.Remove(key);
            Data.Add(key, value);
        }

        public static T GetOrDefault<T>(string key, Func<T> constructor)
        {
            if (Data.ContainsKey(key))
            {
                return (T) Data[key];
            }

            return constructor();
        }

        public static void ClearData()
        {
            Data.Clear();
        }
    }
}