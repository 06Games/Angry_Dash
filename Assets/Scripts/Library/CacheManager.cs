using System.Collections.Generic;

[System.Serializable]
public class Cache
{
    [System.NonSerialized] public static Dictionary<string, Cache> Dictionary = new Dictionary<string, Cache>();
    [System.NonSerialized] Dictionary<string, object> cache;

    public static Cache Open(string name)
    {
        if (Dictionary.ContainsKey(name)) return Dictionary[name];
        else
        {
            var cache = new Cache { cache = new Dictionary<string, object>() };
            Dictionary.Add(name, cache);
            return cache;
        }
    }

    public void Set(string id, object obj)
    {
        if (string.IsNullOrEmpty(id)) return;
        else if (cache.ContainsKey(id)) cache[id] = obj;
        else cache.Add(id, obj);
    }

    public T Get<T>(string id) { return (T)Get(id); }
    public object Get(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        else if (cache.ContainsKey(id)) return cache[id];
        else return null;
    }

    public bool ValueExist(string id) { if (id == null) return false; else return cache.ContainsKey(id); }
}
