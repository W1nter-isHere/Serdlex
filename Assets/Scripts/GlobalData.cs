using System.Collections.Generic;

public static class GlobalData
{
    public static readonly Dictionary<string, object> Data;

    static GlobalData()
    {
        Data = new Dictionary<string, object>();
    }

    public static void Set(string key, object value)
    {
        Data.Remove(key);
        Data.Add(key, value);
    }
}