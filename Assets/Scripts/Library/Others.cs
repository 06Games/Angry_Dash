using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class InspectorUtilities
{
    public static void ClearConsole()
    {
#if UNITY_EDITOR
        var logEntries = Type.GetType("UnityEditor.LogEntries,UnityEditor.dll");
        var clearMethod = logEntries.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
        clearMethod.Invoke(null, null);
#endif
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
            get => new Vector2(UnityEngine.Screen.width, UnityEngine.Screen.height);
            set => UnityEngine.Screen.SetResolution((int)value.x, (int)value.y, fullScreen);
        }

        public static bool fullScreen
        {
            get
            {
#if UNITY_EDITOR
                return EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).maximized;
#else
                return UnityEngine.Screen.fullScreen;
#endif
            }
            set
            {
#if UNITY_EDITOR
                EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).maximized = value;
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
            var type = objectToCheck.GetType();
            var methodInfo = type.GetMethod(methodName);
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
            var components = gameobject.GetComponents<MonoBehaviour>();
            foreach (var m in components)
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
            var components = gameobject.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var m in components)
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
            var tranform = gameobject.transform;
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
