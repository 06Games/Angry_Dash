using AngryDash.Game.RewardChecker;
using AngryDash.Image.Reader;
using AngryDash.Language;
using UnityEngine;
using UnityEngine.UI;

namespace AngryDash.Game.Events
{
    public class Ending : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision) { EndGame(collision.GetComponent<Player>()); }
        public static void EndGame(Player player)
        {
            var EndPanel = GameObject.Find("Base").transform.GetChild(4).GetChild(0);
            if (EndPanel.parent.gameObject.activeInHierarchy) return;
            player.PeutAvancer = false;

            EndPanel.GetChild(0).GetComponent<Text>().text = player.LP.level.name; //Sets the level name
            EndPanel.GetChild(3).GetChild(1).gameObject.SetActive(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Online");
            EndPanel.parent.gameObject.SetActive(true);

            var xml = Inventory.xmlDefault;
            var lvlPlayer = GameObject.Find("Main Camera").GetComponent<LevelPlayer>();
            var gain = 0;
            var lastGain = 0;
            var lvlItem = xml.GetItemByAttribute("PlayedLevels", "type", "Official").GetItemByAttribute("level", "name", lvlPlayer.level.name);
            if (lvlItem.node != null) int.TryParse(lvlItem.Value, out lastGain);
            else
            {
                xml.GetItemByAttribute("PlayedLevels", "type", "Official").CreateItem("level").SetAttribute("name", lvlPlayer.level.name);
                lvlItem = xml.GetItemByAttribute("PlayedLevels", "type", "Official").GetItemByAttribute("level", "name", lvlPlayer.level.name);
            }

            int.TryParse(xml.GetItem("Money").Value, out var money);
            if (lvlPlayer.FromScene == "Home/Play/Official Levels")
            {
                var reward = new Official(lvlPlayer.level.name) { turn = lvlPlayer.nbLancer };
                gain = reward.money;

                for (var i = 0; i < 3; i++) UnityThread.executeCoroutine(OfficialLevels.SetStar(EndPanel.GetChild(1).GetChild(i).GetComponent<UImage_Reader>(), reward.stars > i ? 0 : 3));
            }
            else EndPanel.GetChild(1).gameObject.SetActive(false);

            if (gain > lastGain)
            {
                lvlItem.Value = gain.ToString();
                xml.GetItem("Money").Value = (money + gain - lastGain).ToString();
                Social.IncrementEvent("CgkI9r-go54eEAIQBA", (uint)gain); //Statistics about coin winning

                EndPanel.GetChild(2).GetChild(1).gameObject.SetActive(true);
                EndPanel.GetChild(2).GetChild(1).GetChild(2).GetComponent<Text>().text = LangueAPI.Get("native", "levelPlayer.finished.reward.quantity", "x[0]", gain - lastGain);
            }
            else EndPanel.GetChild(2).GetChild(1).gameObject.SetActive(false);
            Inventory.xmlDefault = xml;
        }
    }
}
