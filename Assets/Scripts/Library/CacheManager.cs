using System;
using System.Collections.Generic;

[Serializable]
public class Cache
{
    [NonSerialized] public static Dictionary<string, Cache> Dictionary = new Dictionary<string, Cache>();
    [NonSerialized] private Dictionary<string, object> cache;

    public static Cache Open(string name)
    {
        if (Dictionary.ContainsKey(name)) return Dictionary[name];
        var cache = new Cache { cache = new Dictionary<string, object>() };
        Dictionary.Add(name, cache);
        return cache;
    }

    public void Set(string id, object obj)
    {
        if (string.IsNullOrEmpty(id)) return;
        if (cache.ContainsKey(id)) cache[id] = obj;
        else cache.Add(id, obj);
    }

    public T Get<T>(string id) { return (T)Get(id); }
    public object Get(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        if (cache.ContainsKey(id)) return cache[id];
        return null;
    }

    public bool ValueExist(string id)
    {
        if (id == null) return false;
        return cache.ContainsKey(id);
    }
}
