using UnityEngine;
using System;
using System.Net;
using UnityEngine.Events;
using System.Linq;
using System.IO;

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
        return Application.internetReachability != NetworkReachability.NotReachable;
    }

    public static bool CanAccessToInternet()
    {
        string resource = "http://google.com";
        string html = string.Empty;
        string HtmlText = "";
        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(resource);
        try
        {
            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
            {
                bool isSuccess = (int)resp.StatusCode < 299 && (int)resp.StatusCode >= 200;
                if (isSuccess)
                {
                    using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                    {
                        //We are limiting the array to 80 so we don't have
                        //to parse the entire html document feel free to 
                        //adjust (probably stay under 300)
                        char[] cs = new char[80];
                        reader.Read(cs, 0, cs.Length);
                        foreach (char ch in cs)
                        {
                            html += ch;
                        }
                    }
                }
            }
            HtmlText = html;
        }
        catch
        {
            HtmlText = "";
        }

        BaseControl.LogNewMassage("Internet connection : " + (HtmlText != "").ToString());
        return HtmlText != "";
    }

    /// <summary> Vérifie si l'appareil est connecter à internet grace à un réseau mobile </summary> 
    public static bool IsOnMobileData()
    {
        return Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork; 
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
        else if (URL.Contains("file:///"))
            start = "file:///";
        else Debug.LogWarning("L'URL n'est pas valide, ou le format est non supporté\n" + URL);

        Uri URi = URL.StartsWith(start, StringComparison.OrdinalIgnoreCase) ? new Uri(URL) : new Uri(start + URL);
        return URi;
    }

    public void ActiveGameObjectIfDisconected(GameObject GO){ GO.SetActive(!IsConnected()); }

    public static bool ValidateIPv4(string ipString)
    {
        if (String.IsNullOrEmpty(ipString))
        {
            return false;
        }

        ipString = ipString.Split(new string[1] { ":" }, System.StringSplitOptions.None)[0];

        if (ipString == "localhost")
            return true;

        string[] splitValues = ipString.Split('.');
        if (splitValues.Length != 4)
        {
            return false;
        }

        byte tempForParsing;

        return splitValues.All(r => byte.TryParse(r, out tempForParsing));
    }
}
