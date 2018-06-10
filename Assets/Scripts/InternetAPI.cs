using UnityEngine;
using System;
using System.Net;
using UnityEngine.Events;

public class InternetAPI : MonoBehaviour {

    void Start()
    {
        OnStart.Invoke();
    }

    [SerializeField] private OnCompleteEvent OnStart;
    [Serializable] public class OnCompleteEvent : UnityEvent { }

    /// <summary> Donne la vitesse de téléchargement en kb/sec </summary>
    public static int DownloadSpeed(string URL = "http://06games.000webhostapp.com/1Mb.txt")
    {
        WebClient wc = new WebClient(); // Initialisation du téléchargement
        double starttime = Environment.TickCount; // ms avant téléchargement
        wc.DownloadData(UrlTransformator(URL)); // Télécharge un fichier de 1MB dans la mémoire vive
        double endtime = Environment.TickCount; // ms après téléchargement

        double secs = Math.Floor(endtime - starttime) / 1000; // Convertis les ms en sec (1000 ms = 1 sec)
        //double secs2 = Math.Round(secs, 0); // arrondis le nombre de second
        double kbsec = Math.Round(1024 / secs); // Calcule la vitesse de download en kb/sec

        return (int)kbsec; // Retourne la vitesse en kb/sec
    }

    /// <summary> Vérifie si l'appareil est connecter à internet </summary> 
    public static bool IsConnected()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
            return false; // Pas de réseau
        else return true; // Réseau
    }

    /// <summary> Initialise une url en URi pour le bon fonctionnement d'un téléchargement avec la classe WebClient </summary> 
    public static Uri UrlTransformator(string URL)
    {
        string start = "";
        if (URL.Contains("http://"))
            start = "http://";
        else if (URL.Contains("https://"))
            start = "https://";
        else if (URL.Contains("file://"))
            start = "file://";
        else Debug.LogWarning("L'URL n'est pas valide, ou le format est non supporté\n" + URL);

        Uri URi = URL.StartsWith(start, StringComparison.OrdinalIgnoreCase) ? new Uri(URL) : new Uri(start + URL);
        return URi;
    }

    public void ActiveGameObjectIfDisconected(GameObject GO){ GO.SetActive(!IsConnected()); }
}
