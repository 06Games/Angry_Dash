using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Runtime.InteropServices;
 
public class FastTexture2D : ScriptableObject
{
    //(c) Brian Chasalow 2014 - brian@chasalow.com
    // Revisions by Miha Krajnc
    [AttributeUsage (AttributeTargets.Method)]
    public sealed class MonoPInvokeCallbackAttribute : Attribute
    {
        public MonoPInvokeCallbackAttribute (Type t)
        {
        }
    }
 
    [DllImport ("__Internal")]
    private static extern void DeleteFastTexture2DAtTextureID (int id);
 
    [DllImport ("__Internal")]
    private static extern void CreateFastTexture2DFromAssetPath (string assetPath, int uuid, bool resize, int resizeW, int resizeH);
 
    [DllImport ("__Internal")]
    private static extern void RegisterFastTexture2DCallbacks (TextureLoadedCallback callback);
 
    public static void CreateFastTexture2D (string path, int uuid, bool resize, int resizeW, int resizeH)
    {
        #if UNITY_EDITOR
        #elif UNITY_IOS
        CreateFastTexture2DFromAssetPath(path, uuid, resize, resizeW, resizeH);
        #endif
    }
 
    public static void CleanupFastTexture2D (int texID)
    {
        #if UNITY_EDITOR
        #elif UNITY_IOS
        DeleteFastTexture2DAtTextureID(texID);
        #endif
    }
 
 
    private static int tex2DCount = 0;
    private static Dictionary<int, FastTexture2D> instances;
 
    public static Dictionary<int, FastTexture2D> Instances {
        get {
            if (instances == null) {
                instances = new Dictionary<int, FastTexture2D> ();
            }
            return instances;
        }
    }
 
    [SerializeField]
    public string url;
    [SerializeField]
    public int uuid;
    [SerializeField]
    public bool resize;
    [SerializeField]
    public int w;
    [SerializeField]
    public int h;
    [SerializeField]
    public int glTextureID;
    [SerializeField]
    private Texture2D nativeTexture;
 
    public Texture2D NativeTexture{ get { return nativeTexture; } }
 
    [SerializeField]
    public bool isLoaded = false;
 
    public delegate void TextureLoadedCallback (int nativeTexID, int original_uuid, int w, int h);
 
    [MonoPInvokeCallback (typeof(TextureLoadedCallback))]
    public static void TextureLoaded (int nativeTexID, int original_uuid, int w, int h)
    {
        if (Instances.ContainsKey (original_uuid) && nativeTexID > -1) {
            FastTexture2D tex = Instances [original_uuid];
            tex.glTextureID = nativeTexID;
            tex.nativeTexture = Texture2D.CreateExternalTexture (w, h, TextureFormat.ARGB32, false, true, (System.IntPtr)nativeTexID);
            tex.nativeTexture.UpdateExternalTexture ((System.IntPtr)nativeTexID);
            tex.isLoaded = true;
            tex.OnFastTexture2DLoaded (tex);
        }
    }
 
    private Action<FastTexture2D> OnFastTexture2DLoaded;
 
    protected void InitFastTexture2D (string _url, int _uuid, bool _resize, int _w, int _h, Action<FastTexture2D> callback)
    {
        this.url = _url;
        this.uuid = _uuid;
        this.resize = _resize;
        this.w = _w;
        this.h = _h;
        this.glTextureID = -1;
        this.OnFastTexture2DLoaded = callback;
        this.isLoaded = false;
    }
 
    private static bool registeredCallbacks = false;
 
    private static void RegisterTheCallbacks ()
    {
        if (!registeredCallbacks) {
            registeredCallbacks = true;
            #if UNITY_IOS
            if (Application.platform == RuntimePlatform.IPhonePlayer)
                RegisterFastTexture2DCallbacks (TextureLoaded);
            #endif
 
        }
    }
 
 
    //dimensions options: if resize is false, w/h are not used. if true, it will downsample to provided dimensions.
    //to create a new texture, call this with the file path of the texture, resize parameters,
    //and a callback to be notified when the texture is loaded.
    public static FastTexture2D CreateFastTexture2D (string url, bool resize, int assetW, int assetH, Action<FastTexture2D> callback)
    {
        #if !UNITY_IOS
        if(tex2DCount == 9999){
            // Do nothing - to eliminate the editor warning
        }

        UnityEngine.Networking.UnityWebRequest ld = UnityEngine.Networking.UnityWebRequestTexture.GetTexture("file://" + url);
        while(!ld.isDone);
 
        Texture2D t2d = UnityEngine.Networking.DownloadHandlerTexture.GetContent(ld);

        FastTexture2D ft = ScriptableObject.CreateInstance<FastTexture2D>();
        ft.nativeTexture = t2d;
        callback(ft);
        return ft;
        #else
 
        //register that you want a callback when it's been created.
        RegisterTheCallbacks ();
        //the uuid is the instance count at time of creation. you pass this into the method to grab the gl texture, and it returns the gl texture with this uuid
        int uuid = tex2DCount;
        tex2DCount = (tex2DCount + 1) % int.MaxValue;
 
        FastTexture2D tex2D = ScriptableObject.CreateInstance<FastTexture2D> ();
        tex2D.InitFastTexture2D (url, uuid, resize, assetW, assetH, callback);
        //call into the plugin to create the thing
        CreateFastTexture2D (tex2D.url, tex2D.uuid, tex2D.resize, tex2D.w, tex2D.h);
 
        //add the instance to the list
        Instances.Add (uuid, tex2D);
 
        //return the instance, someone might want it (but they'll get it with the callback soon anyway)
        return tex2D;
        #endif
    }
 
    private void CleanupTexture ()
    {
        isLoaded = false;
 
        //delete the gl texture
        if (glTextureID != -1)
            CleanupFastTexture2D (glTextureID);
        glTextureID = -1;
 
        //destroy the wrapper object
        if (nativeTexture)
            Destroy (nativeTexture);
 
        //remove it from the list so further callbacks dont try to find it
        if (Instances.ContainsKey (this.uuid))
            Instances.Remove (this.uuid);
    }
 
    //to destroy a FastTexture2D object, you call Destroy() on it.
    public void OnDestroy ()
    {
        CleanupTexture ();
    }
}