using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LangueSelector : MonoBehaviour {

    public Transform Langues;
    public GameObject RestartRequire;
    public LoadingScreenControl LS;

    public bool AutomatiqueUpdate;

    public string[] LangueDispo;
    public int actuel = 0;

    void Start() { NewStart(); }

	void NewStart () {
        if (string.IsNullOrEmpty(LangueAPI.LangGet()))
            ReloadScene();

        for(int i = 0; i < LangueDispo.Length; i++)
        {
            if (LangueAPI.LangGet() == LangueDispo[i])
                actuel = i;
        }

        if (AutomatiqueUpdate)
        {
            StartCoroutine(LangueAPI.UpdateFiles());
            StartCoroutine(GetLangDispo());
        }
	}

    void Update()
    {
        for(int i = 0; i < LangueDispo.Length; i++)
        {
            if (i == actuel)
                Langues.GetChild(i).GetChild(0).gameObject.SetActive(true);
            else Langues.GetChild(i).GetChild(0).gameObject.SetActive(false);
        }
    }

    public void Chang(int i){
        if (i != actuel)
        {
            actuel = i;
            RestartRequire.SetActive(true);
        }
    }
    public void Cancel() { RestartRequire.SetActive(false); NewStart(); }
    public void ReloadScene() { Apply(); LS.LoadScreen(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name); }
    void Apply() { LangueAPI.LangSet(LangueDispo[actuel]); }

    IEnumerator GetLangDispo()
    {
        WWW www = new WWW("https://raw.githubusercontent.com/06-Games/Angry-Dash/master/Langues/index");
        yield return www;
        string[] All = www.text.Split(new string[] { "\n" }, StringSplitOptions.None);

        int lines = All.Length;
        if (string.IsNullOrEmpty(All[lines - 1]))
            lines = lines - 1;

        LangueDispo = new string[lines];
        for (int dispo = 0; dispo < lines; dispo++)
        {
            LangueDispo[dispo] = All[dispo].Split(new string[] { "[" }, StringSplitOptions.None)[0];
        }

        if (LangueAPI.LangGet() == null)
            Apply();
    }
}
