using System;
using System.Collections;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class LangueSelector : MonoBehaviour
{

    public Transform Langues;
    public LoadingScreenControl LS;

    public string[] LangueDispo;
    public int actuel = 0;
    Sprite[] LangFlag;

    void Start() { NewStart(); if (Langues != null) Langues.GetChild(0).gameObject.SetActive(false); }

    void NewStart()
    {
        if (string.IsNullOrEmpty(LangueAPI.selectedLanguage))
            ReloadScene();

        if (Langues != null)
            if (Langues.childCount <= 1)
                GetLangDispo(false);

        for (int i = 0; i < LangueDispo.Length; i++)
        {
            if (LangueAPI.selectedLanguage == LangueDispo[i])
                actuel = i;
        }
    }

    void Update()
    {
        if (Langues != null & LangFlag != null)
        {
            for (int i = 0; i < LangueDispo.Length & i < Langues.childCount - 1; i++)
            {
                if (i == actuel)
                    Langues.GetChild(i + 1).GetChild(0).gameObject.SetActive(true);
                else Langues.GetChild(i + 1).GetChild(0).gameObject.SetActive(false);
            }
        }
    }

    public void Chang(int i)
    {
        if (i != actuel)
            actuel = i;
    }
    public void ReloadScene() { if (LangueAPI.selectedLanguage == LangueDispo[actuel]) return; Apply(); LS.LoadScreen(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, new string[] { "Settings", "Langues" }); }
    void Apply() { if (LangueDispo.Length > actuel) LangueAPI.selectedLanguage = LangueDispo[actuel]; else Debug.LogError((actuel + 1) + " is more than the number of language file : " + LangueDispo.Length); }

    void GetLangDispo(bool forceUpdate = false)
    {
        string[] languages = Directory.GetFiles(Application.persistentDataPath + "/Ressources/default/languages/native/");
        LangFlag = new Sprite[languages.Length];
        for (int i = 0; i < languages.Length; i++)
        {
            Transform go = Instantiate(Langues.GetChild(0).gameObject, new Vector3(), new Quaternion(), Langues).transform;
            go.name = Path.GetFileNameWithoutExtension(languages[i]);
            int dispo = i;
            go.GetComponent<Button>().onClick.RemoveAllListeners();
            go.GetComponent<Button>().onClick.AddListener(() => Chang(dispo));

            if (File.Exists(Sprite_API.Sprite_API.spritesPath("native/GUI/settingsMenu/languages/" + Path.GetFileNameWithoutExtension(languages[i]) + ".png")))
            {
                Texture2D tex = new Texture2D(1, 1);
                tex.LoadImage(File.ReadAllBytes(Sprite_API.Sprite_API.spritesPath("native/GUI/settingsMenu/languages/" + Path.GetFileNameWithoutExtension(languages[i]) + ".png")));
                LangFlag[i] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
            }
            else LangFlag[i] = Langues.GetChild(0).GetComponent<Image>().sprite;
            go.GetComponent<Image>().sprite = LangFlag[i];
            go.gameObject.SetActive(true);
        }

        if (LangueAPI.selectedLanguage == null)
            Apply();

        for (int i = 0; i < LangueDispo.Length; i++)
        {
            if (LangueAPI.selectedLanguage == LangueDispo[i])
                actuel = i;
        }
    }
}
