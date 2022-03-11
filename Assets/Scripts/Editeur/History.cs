using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class History : MonoBehaviour
{

    public static string HistoryFile => Application.persistentDataPath + "/history.txt";

    public EditorPublishedLevels PublishedLevels;

    private void Start() { Refresh(); }
    public void Refresh()
    {
        Initialise();

        var Content = transform.GetChild(1).GetChild(0).GetChild(0);
        for (var i = 1; i < Content.childCount; i++)
            Destroy(Content.GetChild(i).gameObject);

        var history = File.ReadAllLines(HistoryFile);
        for (var i = 1; i < history.Length; i++)
        {
            var a = history[history.Length - i].Split(new[] { "[" }, StringSplitOptions.None);
            var b = a[1].Split(new[] { "] " }, StringSplitOptions.None);
            var c = b[1].Split(new[] { " |" }, StringSplitOptions.None);
            var action = a[0];
            var time = DateTime.Parse(b[0]).ToLocalTime();
            var file = c[0];

            if (action == "O")
                file = c[0].Split(new[] { "/", "_" }, StringSplitOptions.None)[1];
            else if (action == "S")
                file = c[0];
            else
            {
                var dirToFile = c[0].Split(new[] { "/", "\\" }, StringSplitOptions.None);
                file = dirToFile[dirToFile.Length - 1].Replace(".level", "");
            }
            var author = c[1].Replace("|", "");

            Content.GetChild(0).gameObject.SetActive(false);
            var go = Instantiate(Content.GetChild(0).gameObject, Content).transform;

            go.GetChild(0).GetComponent<Text>().text = file;
            go.GetChild(1).GetComponent<Text>().text = author;
            go.GetChild(2).GetComponent<Text>().text = time.ToString("dd/MM/yyyy HH:mm");
            go.GetChild(3).GetChild(0).gameObject.SetActive(action == "P");
            go.GetChild(3).GetChild(1).gameObject.SetActive(action == "E");
            go.GetChild(3).GetChild(2).gameObject.SetActive(action == "O");
            go.GetChild(3).GetChild(3).gameObject.SetActive(action == "S");

            var actual = history.Length - i;
            go.GetComponent<Button>().onClick.AddListener(() => Open(actual));
            go.gameObject.SetActive(true);
        }
    }

    public void Open(int actual)
    {
        var history = File.ReadAllLines(HistoryFile);
        var a = history[actual].Split(new[] { "[" }, StringSplitOptions.None);
        var b = a[1].Split(new[] { "] " }, StringSplitOptions.None);
        var c = b[1].Split(new[] { " |" }, StringSplitOptions.None);
        var action = a[0];
        var file = c[0];

        if (action == "P" | file.Contains(Application.persistentDataPath + "/Levels/Official Levels/")) SceneManager.LoadScene("Player", new[] { "Home/Play/History", "File", file });
        else if (action == "E") SceneManager.LoadScene("Editor", new[] { "Home/Play/History", "Edit", file });
        else if (action == "O")
        {
            PublishedLevels.transform.parent.parent.gameObject.SetActive(true);
            PublishedLevels.transform.parent.GetComponent<MenuManager>().Array(1);
            PublishedLevels.Filter(file);
            PublishedLevels.Select(0);
            transform.parent.parent.gameObject.SetActive(false);
        }
        else if (action == "S") SceneManager.LoadScene("Online", new[] { "Connect", file });
    }

    private static void Initialise()
    {
        if (!File.Exists(HistoryFile))
            File.WriteAllLines(HistoryFile, new[] { "# History of levels played" });
    }

    /// <summary>
    /// Indexe le niveau qui vient d'être lancé
    /// </summary>
    /// <param name="file">Le chemin d'accès au fichier (Local, URL ou IP)</param>
    /// <param name="type">L'identifiant de l'action (E: Edition, P: Jouer, O: En Ligne, S: Serveur)</param>
    /// <param name="author">(Paramètre obligatoire pour le type O) Permet de définir l'auteur du niveau au lieu de scanner le fichier</param>
    public static void LvlPlayed(string file, string type, string author = "")
    {
        Initialise();

        var date = DateTime.UtcNow.ToString();

        if ((type == "E" | type == "P") & author == "")
        {
            if (!File.Exists(file)) return;
            var u = -1;
            var f = File.ReadAllLines(file);
            for (var x = 0; x < f.Length; x++)
            {
                if (f[x].Contains("author = ") & u == -1)
                    u = x;
            }
            if (u != -1) author = f[u].Replace("author = ", "");
        }

        var line = type + "[" + date + "] " + file + " |" + author + "|";

        var history = File.ReadAllLines(HistoryFile);
        for (var i = 0; i < history.Length; i++)
        {
            var b = history[i].Split(new[] { "] " }, StringSplitOptions.None);
            if (b.Length >= 2)
            {
                var splited = b[1].Split(new[] { " |" }, StringSplitOptions.None);
                if (splited.Length >= 2)
                {
                    if (splited[0] == file)
                    {
                        var historyList = history.ToList();
                        historyList.RemoveAt(i);
                        history = historyList.ToArray();
                        i = i - 1;
                    }
                }
            }
        }
        File.WriteAllLines(HistoryFile, history.Union(new[] { line }).ToArray());
    }


    /// <summary>
    /// Supprime une entrée du menu récent
    /// </summary>
    /// <param name="file">Le chemin d'accès au fichier (Local, URL ou IP)</param>
    public static void LvlRemoved(string file)
    {
        var history = File.ReadAllLines(HistoryFile);
        for (var i = 0; i < history.Length; i++)
        {
            var b = history[i].Split(new[] { "] " }, StringSplitOptions.None);
            if (b.Length >= 2)
            {
                var splited = b[1].Split(new[] { " |" }, StringSplitOptions.None);
                if (splited.Length >= 2)
                {
                    if (splited[0] == file)
                    {
                        var historyList = history.ToList();
                        historyList.RemoveAt(i);
                        history = historyList.ToArray();
                        i = i - 1;
                    }
                }
            }
        }
        File.WriteAllLines(HistoryFile, history);
    }
}
