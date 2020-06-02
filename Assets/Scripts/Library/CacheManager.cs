using System.Collections.Generic;
using UnityEngine;

namespace CacheManager
{
    public static class Dictionary
    {
        public static Dictionary<string, Cache> dictionary = new Dictionary<string, Cache>();
        public static bool Exist() { return GameObject.Find("Cache") != null; }
    }

    [System.Serializable]
    public class Cache
    {
        [System.NonSerialized] Dictionary<string, object> dictionary;

        public Cache() { }
        public Cache(string name)
        {
            if (Dictionary.dictionary.ContainsKey(name))
                dictionary = Dictionary.dictionary[name].dictionary;
            else
            {
                dictionary = new Dictionary<string, object>();
                Dictionary.dictionary.Add(name, this);
            }
        }

        public void Set(string id, object obj)
        {
            if (string.IsNullOrEmpty(id)) return;
            else if (dictionary.ContainsKey(id)) dictionary[id] = obj;
            else dictionary.Add(id, obj);
        }

        public T Get<T>(string id) { return (T)Get(id); }
        public object Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            else if (dictionary.ContainsKey(id)) return dictionary[id];
            else return null;
        }

        public bool ValueExist(string id) { if (id == null) return false; else return dictionary.ContainsKey(id); }
    }
}
