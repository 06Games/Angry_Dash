using System.IO;
using System.Linq;
using Unity.Jobs;
using UnityEngine;

namespace Sprite_API
{
    public class Sprite_API : MonoBehaviour
    {
        /// <summary>
        /// Get path to a ressource
        /// </summary>
        /// <param name="id">The id of the ressource</param>
        public static string spritesPath(string id)
        {
#if UNITY_EDITOR
            if (!id.Contains("bg") & !id.Contains("languages/") & !id.Contains("common/") & !string.IsNullOrEmpty(id))
            {
                string fid = id.Replace(" basic", "").Replace(" hover", "").Replace(" pressed", "").Replace(" disabled", "");
                string idPath = Application.dataPath + "/rpID.txt";
                string[] lines = new string[0];
                if (File.Exists(idPath)) lines = File.ReadAllLines(idPath);
                fid = fid.Replace(".png", "").Replace(".json", "");
                if (!string.IsNullOrEmpty(fid)) File.WriteAllLines(idPath, lines.Union(new string[] { fid }));
            }

#endif

            if (ConfigAPI.GetString("ressources.pack") == null)
                ConfigAPI.SetString("ressources.pack", "default");
            string path = Application.persistentDataPath + "/Ressources/" + ConfigAPI.GetString("ressources.pack") + "/textures/" + id;
            if (File.Exists(path)) return path;
            else return Application.persistentDataPath + "/Ressources/default/textures/" + id;
        }

        /// <summary>
        /// Request an animation (or a sprite)
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="border">Border of the Sprites</param>
        /// <returns></returns>
        public static Sprite_API_Data GetSprites(string filePath, Vector4 border = new Vector4(), bool forcePNG = false)
        {
            Load(filePath, border, forcePNG);
            return new CacheManager.Cache("Ressources/textures").Get<Sprite_API_Data>(filePath);
        }

        public static void Load(string filePath, Vector4 border = new Vector4(), bool forcePNG = false)
        {
            System.IntPtr path = System.Runtime.InteropServices.Marshal.StringToBSTR(filePath);
            Data[] data = new Data[] { new Data() { path = path, border = border, forcePNG = forcePNG ? 1 : 0 } };
            var nativeData = new Unity.Collections.NativeArray<Data>(data, Unity.Collections.Allocator.Persistent);
            Sprite_API_Job job = new Sprite_API_Job() { data = nativeData };
            JobHandle handle = job.Schedule(data.Length, 1);
            JobHandle.ScheduleBatchedJobs();
            handle.Complete();
            nativeData.Dispose();
        }
    }
}
