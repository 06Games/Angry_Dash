﻿using System;
using System.Collections;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class LangueSelector : MonoBehaviour
{

    public Transform Langues;
    public GameObject RestartRequire;
    public LoadingScreenControl LS;
    public Transform DownloadingFilesPanel;
    public DependenciesManager dependenciesManager;

    public bool AutomatiqueUpdate;

    public string[] LangueDispo;
    public int actuel = 0;
    Sprite[] LangFlag;

    void Start() { NewStart(); transform.GetChild(0).gameObject.SetActive(false); }

    void NewStart()
    {
        if (!Directory.Exists(Application.persistentDataPath + "/Languages/"))
            Directory.CreateDirectory(Application.persistentDataPath + "/Languages/");

        if (string.IsNullOrEmpty(LangueAPI.LangGet()))
            ReloadScene();

        if (AutomatiqueUpdate)
        {
            string title = "";
            if (Directory.GetFiles(Application.persistentDataPath + "/Languages/").Length > 1)
                title = LangueAPI.String("downloadLangTitleUpdate");
            else title = LangueAPI.String("downloadLangTitle");
            if (!string.IsNullOrEmpty(title)) DownloadingFilesPanel.GetChild(2).GetComponent<Text>().text = title;
            StartCoroutine(LangueAPI.UpdateFiles(DownloadingFilesPanel, this));
        }
        if (Langues != null)
            if (Langues.childCount <= 1)
                GetLangDispo(AutomatiqueUpdate);

        for (int i = 0; i < LangueDispo.Length; i++)
        {
            if (LangueAPI.LangGet() == LangueDispo[i])
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
        {
            actuel = i;
            if (Langues != null)
                RestartRequire.SetActive(true);
        }
    }
    public void Cancel() { RestartRequire.SetActive(false); NewStart(); }
    public void ReloadScene() { Apply(); LS.LoadScreen(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name); }
    void Apply() { if (LangueDispo.Length > actuel) LangueAPI.LangSet(LangueDispo[actuel]); else Debug.LogError((actuel + 1) + " is more than the number of language file : " + LangueDispo.Length); }

    void GetLangDispo(bool forceUpdate = false)
    {
        string[] languages = Directory.GetFiles(Application.persistentDataPath + "/Languages/");
        LangFlag = new Sprite[languages.Length];
        for (int i = 0; i < languages.Length; i++)
        {
            Transform go = Instantiate(Langues.GetChild(0).gameObject, new Vector3(), new Quaternion(), Langues).transform;
            go.name = Path.GetFileNameWithoutExtension(languages[i]);
            go.GetComponent<Button>().onClick.RemoveAllListeners();
            go.GetComponent<Button>().onClick.AddListener(() => Chang(i));

            if (File.Exists(Application.persistentDataPath + "/Languages/Flags/" + Path.GetFileNameWithoutExtension(languages[i]) + ".png"))
            {
                Texture2D tex = new Texture2D(1, 1);
                tex.LoadImage(File.ReadAllBytes(Application.persistentDataPath + "/Languages/Flags/" + Path.GetFileNameWithoutExtension(languages[i]) + ".png"));
                LangFlag[i] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
            }
            else LangFlag[i] = Langues.GetChild(0).GetComponent<Image>().sprite;
            go.GetComponent<Image>().sprite = LangFlag[i];
            go.gameObject.SetActive(true);
        }

        int paddingX = (int)((Langues.GetComponent<RectTransform>().sizeDelta.x - (167 * languages.Length)) / 2); ;
        Langues.GetComponent<HorizontalLayoutGroup>().padding.left = paddingX;
        Langues.GetComponent<HorizontalLayoutGroup>().padding.right = paddingX;

        if (LangueAPI.LangGet() == null)
            Apply();

        for (int i = 0; i < LangueDispo.Length; i++)
        {
            if (LangueAPI.LangGet() == LangueDispo[i])
                actuel = i;
        }
    }

    public void End()
    {
        UnityThread.executeInUpdate(() =>
        {
            DownloadingFilesPanel.gameObject.SetActive(false);
            if (dependenciesManager != null)
                dependenciesManager.DownloadAllRequiredTex();
        });
    }
}
