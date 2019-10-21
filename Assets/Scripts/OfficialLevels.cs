using AngryDash.Image.Reader;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class OfficialLevels : MonoBehaviour
{
    public int lvlNumber = 15;
    public int[] CoinsStars = new int[] { 10, 15, 25 };
    void OnEnable()
    {
        for (int i = 1; i < transform.childCount; i++) Destroy(transform.GetChild(i).gameObject);
        GetMaxLevel((success, lvl) =>
        {
            if (success)
            {
                Social.Event("CgkI9r-go54eEAIQBw", lvl); //Statistics about the highest level completed
                if (lvl >= 1) Social.Achievement("CgkI9r-go54eEAIQBQ", true, (s) => { }); //Achievement 'First steps'
                Social.Achievement("CgkI9r-go54eEAIQBg", lvl < 10 ? (lvl * 10) : 100, (s) => { }); //Achievement 'A good start'
            }

            FileFormat.XML.RootElement xml = Inventory.xmlDefault;
            FileFormat.XML.Item lvlItems = xml.GetItemByAttribute("PlayedLevels", "type", "Official");
            uint stars = 0;
            for (int i = 0; i < lvlNumber; i++)
            {
                var btn = Instantiate(transform.GetChild(0).gameObject, transform).GetComponent<Button>();
                string LevelName = (i + 1).ToString();
                btn.name = "Level " + LevelName;
                btn.gameObject.SetActive(true);

                btn.interactable = !success ? true : (uint)i <= lvl;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    if (System.IO.File.Exists(Application.persistentDataPath + "/Levels/Official Levels/" + LevelName + ".level"))
                    {
                        GameObject.Find("Audio").GetComponent<menuMusic>().Stop();
                        GameObject.Find("LoadingScreen").GetComponent<LoadingScreenControl>().LoadScreen("Player", new string[] { "Home/Play/Official Levels", "File", Application.persistentDataPath + "/Levels/Official Levels/" + LevelName + ".level" });
                    }
                    else GameObject.Find("LoadingScreen").GetComponent<LoadingScreenControl>().LoadScreen("Start");
                });

                if (!btn.interactable) StartCoroutine(e(btn.GetComponent<UImage_Reader>(), 3));

                btn.transform.GetChild(1).GetComponent<Text>().text = AngryDash.Language.LangueAPI.Get("native", "PlayOfficialLevels", "Level [0]", LevelName);

                int coins = 0;
                var Item = lvlItems.GetItemByAttribute("level", "name", LevelName);
                if (Item != null) int.TryParse(Item.Value, out coins);
                for (int s = 0; s < 3; s++)
                {
                    if (CoinsStars[s] <= coins) stars++;
                    StartCoroutine(e(btn.transform.GetChild(2).GetChild(s).GetComponent<UImage_Reader>(), CoinsStars[s] <= coins ? 0 : 3));
                }
            }
            Social.Leaderboard("CgkI9r-go54eEAIQAQ", stars, (s) => { }); //Leaderboard of players according to their star number in the official levels

            IEnumerator e(UImage_Reader obj, int state)
            {
                yield return new WaitForEndOfFrame();
                obj.StartAnimating(state);
            }
        });
    }

    void GetMaxLevel(System.Action<bool, ulong> callback)
    {
        Social.GetEvent("CgkI9r-go54eEAIQBw", (success, lvl) =>
        {
            GetLocalMaxLevel((s, l) => {
                if (success)
                {
                    if (l > lvl) callback.Invoke(s, l);
                    else callback.Invoke(success, lvl);
                }
                else callback.Invoke(s, l);
            });
        });
    }
    void GetLocalMaxLevel(System.Action<bool, ulong> callback)
    {
        ulong max = Inventory.xmlDefault.GetItemByAttribute("PlayedLevels", "type", "Official").GetItems("level").Select(l => { ulong.TryParse(l.Attribute("name"), out ulong c); return c; }).Max();
        callback.Invoke(true, max);
    }
}
