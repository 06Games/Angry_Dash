using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Recent : MonoBehaviour
{

    public static string HistoryFile { get { return Application.persistentDataPath + "/history.txt"; } }

    public Editeur editeur;

    void Start() { Refresh(); }
    public void Refresh()
    {
        Initialise();

        Transform Content = transform.GetChild(1).GetChild(0).GetChild(0);
        for (int i = 1; i < Content.childCount; i++)
            Destroy(Content.GetChild(i).gameObject);

        string[] history = File.ReadAllLines(HistoryFile);
        for (int i = 1; i < history.Length; i++)
        {
            string[] a = history[history.Length - i].Split(new string[] { "[" }, System.StringSplitOptions.None);
            string[] b = a[1].Split(new string[] { "] " }, System.StringSplitOptions.None);
            string[] c = b[1].Split(new string[] { " |" }, System.StringSplitOptions.None);
            string action = a[0];
            System.DateTime time = System.DateTime.Parse(b[0]).ToLocalTime();
            string file = c[0];

            if (action == "O")
                file = c[0].Split(new string[] { "/", "_" }, System.StringSplitOptions.None)[1];
            else if (action == "S")
                file = c[0];
            else
            {
                string[] dirToFile = c[0].Split(new string[] { "/", "\\" }, System.StringSplitOptions.None);
                file = dirToFile[dirToFile.Length - 1].Replace(".level", "");
            }
            string author = c[1].Replace("|", "");

            Content.GetChild(0).gameObject.SetActive(false);
            Transform go = Instantiate(Content.GetChild(0).gameObject, Content).transform;

            go.GetChild(0).GetComponent<Text>().text = file;
            go.GetChild(1).GetComponent<Text>().text = author;
            go.GetChild(2).GetComponent<Text>().text = time.ToString("dd/MM/yyyy HH:mm");
            go.GetChild(3).GetChild(0).gameObject.SetActive(action == "P");
            go.GetChild(3).GetChild(1).gameObject.SetActive(action == "E");
            go.GetChild(3).GetChild(2).gameObject.SetActive(action == "O");
            go.GetChild(3).GetChild(3).gameObject.SetActive(action == "S");

            int actual = history.Length - i;
            go.GetComponent<Button>().onClick.AddListener(() => Open(actual));
            go.gameObject.SetActive(true);
        }
    }

    public void Open(int actual)
    {
        string[] history = File.ReadAllLines(HistoryFile);
        string[] a = history[actual].Split(new string[] { "[" }, System.StringSplitOptions.None);
        string[] b = a[1].Split(new string[] { "] " }, System.StringSplitOptions.None);
        string[] c = b[1].Split(new string[] { " |" }, System.StringSplitOptions.None);
        string action = a[0];
        string file = c[0];

        if (action == "P" | file.Contains(Application.persistentDataPath + "/Level/Solo/"))
        {
            File.WriteAllLines(Application.temporaryCachePath + "/play.txt", new string[2] { file, "Editor/History" });
            GameObject.Find("LoadingScreen").GetComponent<LoadingScreenControl>().LoadScreen("Player");
        }
        else if (action == "E")
        {
            if (GameObject.Find("Audio") != null)
                GameObject.Find("Audio").GetComponent<menuMusic>().Stop();
            editeur.EditFile(file);
            LvlPlayed(file, "E");

            transform.parent.GetChild(0).gameObject.SetActive(true);
            gameObject.SetActive(false);
        }
        else if (action == "O")
        {
            Transform OnlinePanel = transform.parent.GetChild(3);
            OnlinePanel.gameObject.SetActive(true);
            OnlinePanel.GetComponent<EditorOnline>().Search(file);
            OnlinePanel.GetComponent<EditorOnline>().OpenLevelMenu(0);
            gameObject.SetActive(false);
        }
        else if (action == "S")
            GameObject.Find("LoadingScreen").GetComponent<LoadingScreenControl>().LoadScreen("Online", new string[] { file });
    }

    static void Initialise()
    {
        if (!File.Exists(HistoryFile))
            File.WriteAllLines(HistoryFile, new string[] { "# History of levels played" });
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

        string date = System.DateTime.UtcNow.ToString();

        if ((type == "E" | type == "P") & author == "")
        {
            if (!File.Exists(file)) return;
            int u = -1;
            string[] f = File.ReadAllLines(file);
            for (int x = 0; x < f.Length; x++)
            {
                if (f[x].Contains("author = ") & u == -1)
                    u = x;
            }
            if (u != -1) author = f[u].Replace("author = ", "");
        }

        string line = type + "[" + date + "] " + file + " |" + author + "|";

        string[] history = File.ReadAllLines(HistoryFile);
        for (int i = 0; i < history.Length; i++)
        {
            string[] b = history[i].Split(new string[] { "] " }, System.StringSplitOptions.None);
            if (b.Length >= 2)
            {
                string[] splited = b[1].Split(new string[] { " |" }, System.StringSplitOptions.None);
                if (splited.Length >= 2)
                {
                    if (splited[0] == file)
                    {
                        List<string> historyList = history.ToList();
                        historyList.RemoveAt(i);
                        history = historyList.ToArray();
                        i = i - 1;
                    }
                }
            }
        }
        File.WriteAllLines(HistoryFile, history.Union(new string[] { line }).ToArray());
    }


    /// <summary>
    /// Supprime une entrée du menu récent
    /// </summary>
    /// <param name="file">Le chemin d'accès au fichier (Local, URL ou IP)</param>
    public static void LvlRemoved(string file)
    {
        string[] history = File.ReadAllLines(HistoryFile);
        for (int i = 0; i < history.Length; i++)
        {
            string[] b = history[i].Split(new string[] { "] " }, System.StringSplitOptions.None);
            if (b.Length >= 2)
            {
                string[] splited = b[1].Split(new string[] { " |" }, System.StringSplitOptions.None);
                if (splited.Length >= 2)
                {
                    if (splited[0] == file)
                    {
                        List<string> historyList = history.ToList();
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
