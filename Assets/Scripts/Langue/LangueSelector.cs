using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LangueSelector : MonoBehaviour {

    public Text ZoneDeTexte;
    public Button Precedant;
    public Button Suivant;

    public bool AutomatiqueUpdate;

    public string[] LangueDispo;
    public int actuel = 0;

	void Start () {
        actuel = 0;
        if (AutomatiqueUpdate)
        {
            StartCoroutine(LangueAPI.UpdateFiles());
            StartCoroutine(GetLangDispo());
        }
	}

    void Update()
    {
        if (actuel >= LangueDispo.Length-1)
            Suivant.interactable = false;
        else Suivant.interactable = true;

        if (actuel == 0)
            Precedant.interactable = false;
        else Precedant.interactable = true;
        
        ZoneDeTexte.text = LangueDispo[actuel];
    }

    public void PrecedantButton() { actuel = actuel - 1; }
    public void SuivantButton() { actuel = actuel + 1; }
    public void Apply() { LangueAPI.LangSet(LangueDispo[actuel]); }

    IEnumerator GetLangDispo()
    {
        WWW www = new WWW("https://raw.githubusercontent.com/06Games/06GamesLauncher/master/Asset/Langue/index");
        yield return www;
        string[] All = www.text.Split(new string[] { "\n" }, StringSplitOptions.None);
        
        LangueDispo = new string[All.Length - 1];
        int dispo;
        for (dispo = 0; dispo < All.Length - 1; dispo++)
        {
            LangueDispo[dispo] = All[dispo].Split(new string[] { "[" }, StringSplitOptions.None)[0];
        }

        if (LangueAPI.LangGet() == null)
        {
            LangueAPI.LangSet(LangueDispo[actuel]);
            ZoneDeTexte.text = LangueAPI.LangGet();
        }
    }
}
