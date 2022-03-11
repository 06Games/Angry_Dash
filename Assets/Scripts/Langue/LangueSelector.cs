using System;
using System.IO;
using System.Linq;
using AngryDash.Image.Reader;
using UnityEngine;
using UnityEngine.UI;

namespace AngryDash.Language
{
    public class LangueSelector : MonoBehaviour
    {

        public Transform Langues;

        public string[] LangueDispo;
        public uint actuel;
        private Sprite[] LangFlag;

        private void Start() { NewStart(); if (Langues != null) Langues.GetChild(0).gameObject.SetActive(false); }

        private void NewStart()
        {
            if (string.IsNullOrEmpty(LangueAPI.selectedLanguage)) ReloadScene();

            if (Langues != null)
                if (Langues.childCount <= 1) GetLangDispo();

            actuel = (uint)Array.IndexOf(LangueDispo, LangueAPI.selectedLanguage);
        }

        private void Update()
        {
            if (Langues != null & LangFlag != null)
            {
                for (var i = 0; i < LangueDispo.Length & i < Langues.childCount - 1; i++)
                    Langues.GetChild(i + 1).GetComponent<Button>().interactable = i != actuel;
            }
        }

        public void Chang(uint i)
        {
            if (i != actuel) actuel = i;
        }
        public void ReloadScene() { if (LangueAPI.selectedLanguage == LangueDispo[actuel]) return; Apply(); SceneManager.ReloadScene(new[] { "Settings", "Langues" }); }
        private void Apply() { if (LangueDispo.Length > actuel) LangueAPI.selectedLanguage = LangueDispo[actuel]; else Debug.LogError((actuel + 1) + " is more than the number of language file : " + LangueDispo.Length); }

        private void GetLangDispo()
        {
            LangueDispo = Directory.GetFiles(Application.persistentDataPath + "/Ressources/default/languages/native/").Select(f => Path.GetFileNameWithoutExtension(f)).OrderBy(f => f).ToArray();
            LangFlag = new Sprite[LangueDispo.Length];
            for (uint i = 0; i < LangueDispo.Length; i++)
            {
                var go = Instantiate(Langues.GetChild(0).gameObject, new Vector3(), new Quaternion(), Langues).transform;
                go.name = LangueDispo[i];
                var dispo = i;
                go.GetComponent<Button>().onClick.RemoveAllListeners();
                go.GetComponent<Button>().onClick.AddListener(() => Chang(dispo));
                go.GetComponent<UImage_Reader>().SetID("native/GUI/settingsMenu/languages/" + LangueDispo[dispo]).LoadAsync();
                go.gameObject.SetActive(true);
            }

            if (LangueAPI.selectedLanguage == null) Apply();
            actuel = (uint)Array.IndexOf(LangueDispo, LangueAPI.selectedLanguage);
        }
    }
}
