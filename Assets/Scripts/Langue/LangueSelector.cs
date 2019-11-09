using AngryDash.Image.Reader;
using System;
using System.Collections;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

namespace AngryDash.Language
{
    public class LangueSelector : MonoBehaviour
    {

        public Transform Langues;

        public string[] LangueDispo;
        public int actuel = 0;
        Sprite[] LangFlag;

        void Start() { NewStart(); if (Langues != null) Langues.GetChild(0).gameObject.SetActive(false); }

        void NewStart()
        {
            if (string.IsNullOrEmpty(LangueAPI.selectedLanguage)) ReloadScene();

            if (Langues != null)
                if (Langues.childCount <= 1) GetLangDispo();

            for (int i = 0; i < LangueDispo.Length; i++)
            {
                if (LangueAPI.selectedLanguage == LangueDispo[i]) actuel = i;
            }
        }

        void Update()
        {
            if (Langues != null & LangFlag != null)
            {
                for (int i = 0; i < LangueDispo.Length & i < Langues.childCount - 1; i++)
                    Langues.GetChild(i + 1).GetComponent<Button>().interactable = i != actuel;
            }
        }

        public void Chang(int i)
        {
            if (i != actuel) actuel = i;
        }
        public void ReloadScene() { if (LangueAPI.selectedLanguage == LangueDispo[actuel]) return; Apply(); SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, new string[] { "Settings", "Langues" }); }
        void Apply() { if (LangueDispo.Length > actuel) LangueAPI.selectedLanguage = LangueDispo[actuel]; else Debug.LogError((actuel + 1) + " is more than the number of language file : " + LangueDispo.Length); }

        void GetLangDispo()
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
                go.GetComponent<UImage_Reader>().SetID("native/GUI/settingsMenu/languages/" + Path.GetFileNameWithoutExtension(languages[i])).Load();
                go.gameObject.SetActive(true);
            }

            if (LangueAPI.selectedLanguage == null) Apply();

            for (int i = 0; i < LangueDispo.Length; i++)
            {
                if (LangueAPI.selectedLanguage == LangueDispo[i]) actuel = i;
            }
        }
    }
}
