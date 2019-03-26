using UnityEngine;

public static class InspectorUtilities
{
    public static void ClearConsole()
    {
#if UNITY_EDITOR
        var logEntries = System.Type.GetType("UnityEditor.LogEntries,UnityEditor.dll");
        var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        clearMethod.Invoke(null, null);
#endif
    }
}


namespace Level
{
    [System.Serializable]
    public class LevelItem
    {
        public string Name = "";
        public string Author = "";
        public string Description = "";
        public string Music = "";
        public string Data = "";
        public LevelItem() { }
        public LevelItem(string name, string author = "", string data = null, string description = "", string music = "")
        {
            Name = name;
            Author = author;
            if (data == null) Data = "";
            else Data = data;
            Description = description;
            Music = music;
        }

        public override string ToString() { return FileFormat.XML.Utils.ClassToXML(this); }
        public static LevelItem Parse(string data) { return FileFormat.XML.Utils.XMLtoClass<LevelItem>(data); }
    }

    [System.Serializable]
    public class SongItem
    {
        public string Name = "";
        public string Artist = "";
        public string Licence = "";
        public string URL = "";
        public SongItem() { }
        public SongItem(string name, string artist = "", string licence = "", string url = "")
        {
            Name = name;
            Artist = artist;
            Licence = licence;
            URL = url;
        }

        public override string ToString() { return FileFormat.XML.Utils.ClassToXML(this); }
        public static SongItem Parse(string data) { return FileFormat.XML.Utils.XMLtoClass<SongItem>(data); }
    }
}

namespace Display
{
    public static class Screen
    {
        /// <summary>
        /// Get the main screen resolution as a Vector2
        /// </summary>
        public static Vector2 Resolution
        {
            get { return new Vector2(UnityEngine.Screen.width, UnityEngine.Screen.height); }
            set { UnityEngine.Screen.SetResolution((int)value.x, (int)value.y, fullScreen); }
        }

        public static bool fullScreen
        {
            get
            {
#if UNITY_EDITOR
                return UnityEditor.EditorWindow.GetWindow(System.Type.GetType("UnityEditor.GameView,UnityEditor")).maximized;
#else
                return UnityEngine.Screen.fullScreen;
#endif
            }
            set
            {
#if UNITY_EDITOR
                UnityEditor.EditorWindow.GetWindow(System.Type.GetType("UnityEditor.GameView,UnityEditor")).maximized = value;
#else
                UnityEngine.Screen.fullScreen = value;
#endif
            }
        }

        public static void SetResolution(int width, int height, bool fullscreen)
        {
#if UNITY_EDITOR
            fullScreen = fullscreen;
#else
            UnityEngine.Screen.SetResolution(width, height, fullscreen);
#endif
        }
    }
}



namespace MessengerExtensions
{
    /// <summary>
    /// Broadcast messages between objects and components, including inactive ones (which Unity doesn't do)
    /// </summary>
    public static class MessengerThatIncludesInactiveElements
    {

        /// <summary>
        /// Determine if the object has the given method
        /// </summary>
        private static void InvokeIfExists(this object objectToCheck, string methodName, params object[] parameters)
        {
            System.Type type = objectToCheck.GetType();
            System.Reflection.MethodInfo methodInfo = type.GetMethod(methodName);
            if (type.GetMethod(methodName) != null)
            {
                methodInfo.Invoke(objectToCheck, parameters);
            }
        }

        /// <summary>
        /// Invoke the method if it exists in any component of the game object, even if they are inactive
        /// </summary>
        public static void BroadcastToAll(this GameObject gameobject, string methodName, params object[] parameters)
        {
            MonoBehaviour[] components = gameobject.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour m in components)
            {
                m.InvokeIfExists(methodName, parameters);
            }
        }
        /// <summary>
        /// Invoke the method if it exists in any component of the component's game object, even if they are inactive
        /// </summary>
        public static void BroadcastToAll(this Component component, string methodName, params object[] parameters)
        {
            component.gameObject.BroadcastToAll(methodName, parameters);
        }

        /// <summary>
        /// Invoke the method if it exists in any component of the game object and its children, even if they are inactive
        /// </summary>
        public static void SendMessageToAll(this GameObject gameobject, string methodName, params object[] parameters)
        {
            MonoBehaviour[] components = gameobject.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (MonoBehaviour m in components)
            {
                m.InvokeIfExists(methodName, parameters);
            }
        }
        /// <summary>
        /// Invoke the method if it exists in any component of the component's game object and its children, even if they are inactive
        /// </summary>
        public static void SendMessageToAll(this Component component, string methodName, params object[] parameters)
        {
            component.gameObject.SendMessageToAll(methodName, parameters);
        }

        /// <summary>
        /// Invoke the method if it exists in any component of the game object and its ancestors, even if they are inactive
        /// </summary>
        public static void SendMessageUpwardsToAll(this GameObject gameobject, string methodName, params object[] parameters)
        {
            Transform tranform = gameobject.transform;
            while (tranform != null)
            {
                tranform.gameObject.BroadcastToAll(methodName, parameters);
                tranform = tranform.parent;
            }
        }
        /// <summary>
        /// Invoke the method if it exists in any component of the component's game object and its ancestors, even if they are inactive
        /// </summary>
        public static void SendMessageUpwardsToAll(this Component component, string methodName, params object[] parameters)
        {
            component.gameObject.SendMessageUpwardsToAll(methodName, parameters);
        }
    }
}
