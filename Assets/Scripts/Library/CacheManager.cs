﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CacheManager
{
    public class Dictionary : MonoBehaviour
    {
        public Dictionary<string, Cache> dictionary;
        public Dictionary()
        { dictionary = new Dictionary<string, Cache>(); }

        public static Dictionary Static()
        {
            if (GameObject.Find("Cache") != null)
                return GameObject.Find("Cache").GetComponent<Dictionary>();
            else
            {
                GameObject cache = new GameObject("Cache");
                DontDestroyOnLoad(cache);
                return cache.AddComponent<Dictionary>();
            }
        }
        public static bool Exist() { return GameObject.Find("Cache") != null; }

#if UNITY_EDITOR
        public string[] key;
        public Cache[] value;
        private void Update()
        {
            key = dictionary.Keys.ToArray();
            value = dictionary.Values.ToArray();
        }
#endif
    }

    [System.Serializable]
    public class Cache
    {
        Dictionary<string, object> dictionary;

        public Cache(string name)
        {
            Dictionary dic = Dictionary.Static();
            if (dic.dictionary.ContainsKey(name))
                dictionary = dic.dictionary[name].dictionary;
            else
            {
                dictionary = new Dictionary<string, object>();
                dic.dictionary.Add(name, this);
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

        public bool ValueExist(string id) { return dictionary.ContainsKey(id); }
    }
}
