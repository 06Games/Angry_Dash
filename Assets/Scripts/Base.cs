using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Base : MonoBehaviour
{

    [Serializable] public class OnCompleteEvent : UnityEvent { }
    [SerializeField] private OnCompleteEvent OnUpdate = new OnCompleteEvent();

    private void Update() { OnUpdate.Invoke(); }

    public void Scene(string levelName) { UnityEngine.SceneManagement.SceneManager.LoadScene(levelName); /* Charge la scene */ }

    public void Quit()
    {
        Quit(true);
    }
    public static void Quit(bool forceEditor = true)
    {
#if UNITY_EDITOR
        if (forceEditor)
            UnityEditor.EditorApplication.isPlaying = false;
        else Debug.Log("The game as been close");
#else
            Debug.Log("The game as been close");
            Application.Quit();
#endif
    }

    public void CharacterCounter(Text display)
    {
        InputField IF = GetComponent<InputField>();
        display.text = IF.text.Length + " / " + IF.characterLimit;
    }

    public void OpenFolder(string path) { OpenFolderStatic(path); }
    public static void OpenFolderStatic(string path)
    {
        path = path.Replace("%PERSISTENT%", Application.persistentDataPath);
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        path = path.Replace("/", "\\");
        System.Diagnostics.Process.Start("explorer.exe", "\"" + path + "\"");
#elif UNITY_STANDALONE_MACOS
        bool openInsidesOfFolder = false;
        string macPath = path.Replace("\\", "/"); // mac finder doesn't like backward slashes

        if (System.IO.Directory.Exists(macPath)) // if path requested is a folder, automatically open insides of that folder
            openInsidesOfFolder = true;

        if (!macPath.StartsWith("\"")) macPath = "\"" + macPath;
        if (!macPath.EndsWith("\"")) macPath = macPath + "\"";

        string arguments = (openInsidesOfFolder ? "" : "-R ") + macPath;
        System.Diagnostics.Process.Start("open", arguments);
#elif UNITY_ANDROID
        Application.OpenURL("file://" + path);
#elif UNITY_IOS
        Application.OpenURL("file://" + path);
        //NativeShare.Share("", path);
#endif
    }


    public void ActiveObject(GameObject go) { go.SetActive(true); }
    public static void ActiveObjectStatic(GameObject go) { UnityThread.executeInUpdate(() => go.SetActive(true)); }

    public void DeactiveObject(GameObject go) { go.SetActive(false); }
    public static void DeactiveObjectStatic(GameObject go) { UnityThread.executeInUpdate(() => go.SetActive(false)); }

    public void OpenURL(string URL) { Application.OpenURL(URL); }

    public static string GetVersion() { return Application.version; }

    public void Gold(Text t)
    {
        if (string.IsNullOrEmpty(PlayerPrefs.GetString("money")))
            t.text = "0";
        else t.text = PlayerPrefs.GetString("money");
    }
}
