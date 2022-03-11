using System;
using System.Collections;
using System.IO;
using System.Linq;
using AngryDash.Game.RewardChecker;
using AngryDash.Image.Reader;
using AngryDash.Language;
using FileFormat.XML;
using Tools;
using UnityEngine;
using UnityEngine.UI;

public class OfficialLevels : MonoBehaviour
{
    public int lvlNumber = 15;
    public readonly int[] CoinsStars = { 10, 15, 25 };
    private Item lvlItems;

    private void OnEnable()
    {
        GetMaxLevel(lvl =>
        {
            Social.Event("CgkI9r-go54eEAIQBw", lvl); //Statistics about the highest level completed
            if (lvl >= 1) Social.Achievement("CgkI9r-go54eEAIQBQ", true, s => { }); //Achievement 'First steps'
            Social.Achievement("CgkI9r-go54eEAIQBg", lvl < 10 ? (lvl * 10) : 100, s => { }); //Achievement 'A good start'

            lvlItems = Inventory.xmlDefault.GetItemByAttribute("PlayedLevels", "type", "Official");
            uint stars = 0;
            for (var i = 0; i < lvlNumber; i++)
            {
                var btn = Instantiate(transform.GetChild(0).gameObject, transform).GetComponent<Button>();
                var LevelName = (i + 1).ToString();
                btn.name = "Level " + LevelName;
                btn.gameObject.SetActive(true);

                btn.interactable = (uint)i <= lvl;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => Select(LevelName));

                if (!btn.interactable) StartCoroutine(SetStar(btn.GetComponent<UImage_Reader>(), 3));

                btn.transform.GetChild(1).GetComponent<Text>().text = LangueAPI.Get("native", "play.officialLevels.level", "Level [0]", LevelName);

                var coins = 0;
                var Item = lvlItems.GetItemByAttribute("level", "name", LevelName);
                if (Item != null) int.TryParse(Item.Value, out coins);
                for (var s = 0; s < 3; s++)
                {
                    if (CoinsStars[s] <= coins) stars++;
                    StartCoroutine(SetStar(btn.transform.GetChild(2).GetChild(s).GetComponent<UImage_Reader>(), CoinsStars[s] <= coins ? 0 : 3));
                }
            }
            Social.Leaderboard("CgkI9r-go54eEAIQAQ", stars, s => { }); //Leaderboard of players according to their star number in the official levels
        });
    }
    private void OnDisable()
    {
        foreach (var go in transform.GetChilds().Where(c => c.name.Contains("Level "))) Destroy(go.gameObject);
    }
    public static IEnumerator SetStar(UImage_Reader obj, int state)
    {
        yield return new WaitForEndOfFrame();
        obj.StartAnimating(state);
    }

    private void GetMaxLevel(Action<ulong> callback)
    {
        var levels = Inventory.xmlDefault.GetItemByAttribute("PlayedLevels", "type", "Official").GetItems("level");
        if (levels.Length > 0) callback(levels.Select(l => { ulong.TryParse(l.Attribute("name"), out var c); return c; }).Max());
        else Social.GetEvent("CgkI9r-go54eEAIQBw", (success, lvl) => callback(success ? lvl : 0));
    }

    private void Select(string levelName)
    {
        var selectedPanel = transform.Find("SelectedPanel").GetChild(0);
        selectedPanel.parent.SetSiblingIndex(transform.childCount - 1);
        selectedPanel.GetChild(0).GetComponent<Text>().text = LangueAPI.Get("native", "play.officialLevels.level", "Level [0]", levelName);
        var stars = selectedPanel.GetChild(1);
        var maxThrows = new Official(levelName).starsGain;
        for (var s = 0; s < 3; s++)
        {
            var coins = 0;
            var Item = lvlItems.GetItemByAttribute("level", "name", levelName);
            if (Item != null) int.TryParse(Item.Value, out coins);
            StartCoroutine(SetStar(stars.GetChild(s).GetChild(0).GetComponent<UImage_Reader>(), CoinsStars[s] <= coins ? 0 : 3));

            stars.GetChild(s).GetChild(1).GetComponent<Text>().text = LangueAPI.Get("native",
                maxThrows[s] <= 1 ? "levelPlayer.throw" : "levelPlayer.throws",
                maxThrows[s] <= 1 ? "[0] throw" : "[0] throws",
                maxThrows[s] == 0 ? "~" : maxThrows[s].ToString());
        }

        var playBtn = selectedPanel.GetChild(2).GetComponent<Button>();
        playBtn.onClick.RemoveAllListeners();
        playBtn.onClick.AddListener(() =>
        {
            if (File.Exists(Application.persistentDataPath + "/Levels/Official Levels/" + levelName + ".level"))
            {
                GameObject.Find("Audio").GetComponent<MenuMusic>().Stop();
                SceneManager.LoadScene("Player", new[] { "Home/Play/Official Levels", "File", Application.persistentDataPath + "/Levels/Official Levels/" + levelName + ".level" });
            }
            else SceneManager.LoadScene("Start");
        });

        selectedPanel.parent.gameObject.SetActive(true);
    }
}
