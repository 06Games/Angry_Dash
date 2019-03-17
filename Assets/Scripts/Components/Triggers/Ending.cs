using Tools;
using UnityEngine;

namespace RewardChecker
{
    /// <summary> For Official Levels </summary>
    public class Official
    {
        /// <summary> Level's name </summary>
        public int name;
        /// <summary> Number of turn </summary>
        public int turn { get; set; }

        public Official(string lvlName) { int.TryParse(lvlName, out name); }

        /// <summary> The money to give the player </summary>
        public int money { get
            {
                if(turn == 0)
                {
                    Debug.LogError("You are a cheater !");
                    return 0;
                }
                else if(name == 1)
                {
                    if (turn <= 8) return 25;
                    else if (turn <= 10) return 15;
                    else if (turn <= 12) return 10;
                    else return 5;
                }
                else if(name == 2)
                {
                    if (turn <= 6) return 25;
                    else if (turn == 7) return 15;
                    else if (turn <= 9) return 10;
                    else return 5;
                }
                else if (name == 3)
                {
                    if (turn <= 4) return 25;
                    else if (turn == 5) return 10;
                    else return 5;
                }
                else if (name == 4)
                {
                    if (turn <= 9) return 25;
                    else if (turn <= 11) return 15;
                    else if (turn <= 13) return 10;
                    else return 5;
                }
                else if (name == 5)
                {
                    if (turn <= 15) return 25;
                    else if (turn <= 20) return 15;
                    else if (turn <= 25) return 10;
                    else return 5;
                }
                else if (name >= 6 & name <= 7)
                {
                    if (turn <= 17) return 25;
                    else if (turn <= 20) return 15;
                    else if (turn <= 25) return 10;
                    else return 5;
                }
                else if (name == 8)
                {
                    if (turn <= 5) return 25;
                    else if (turn <= 8) return 15;
                    else if (turn <= 10) return 10;
                    else return 5;
                }
                else if (name == 9)
                {
                    if (turn <= 21) return 25;
                    else if (turn <= 25) return 15;
                    else if (turn <= 29) return 10;
                    else return 5;
                }
                else if (name == 10)
                {
                    if (turn <= 20) return 25;
                    else if (turn <= 25) return 15;
                    else if (turn <= 30) return 10;
                    else return 5;
                }
                else return 0;
            }
        }
    }
}

public class Ending : MonoBehaviour
{
    Player player;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (GameObject.Find("Base").transform.GetChild(3).gameObject.activeInHierarchy) return;
        player = collision.gameObject.GetComponent<Player>();
        player.PeutAvancer = false;

        GameObject.Find("Base").transform.GetChild(3).GetChild(2).GetChild(0).gameObject.SetActive(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Online");
        GameObject.Find("Base").transform.GetChild(3).GetChild(2).GetChild(1).gameObject.SetActive(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Online");
        GameObject.Find("Base").transform.GetChild(3).gameObject.SetActive(true);

        FileFormat.XML.RootElement xml = Inventory.xmlDefault;
        LevelPlayer lvlPlayer = GameObject.Find("Main Camera").GetComponent<LevelPlayer>();
        int gain = 0;
        int lastGain = 0;
        FileFormat.XML.Item lvlItem = xml.GetItemByAttribute("PlayedLevels", "type", "Official").GetItemByAttribute("level", "name", lvlPlayer.level.name);
        if (lvlItem != null)
            int.TryParse(lvlItem.Value, out lastGain);
        else
        {
            xml.GetItemByAttribute("PlayedLevels", "type", "Official").CreateItem("level").CreateAttribute("name", lvlPlayer.level.name);
            lvlItem = xml.GetItemByAttribute("PlayedLevels", "type", "Official").GetItemByAttribute("level", "name", lvlPlayer.level.name);
        }

        int money = 0;
        int.TryParse(xml.GetItem("Money").Value, out money);

        if (lvlPlayer.FromScene == "Home/Play/Official Levels")
        {
            RewardChecker.Official reward = new RewardChecker.Official(lvlPlayer.level.name);
            reward.turn = lvlPlayer.nbLancer;
            gain = reward.money;
        }

        if (gain > lastGain)
        {
            lvlItem.Value = gain.ToString();
            xml.GetItem("Money").Value = (money + gain - lastGain).ToString();

            GameObject.Find("Base").transform.GetChild(3).GetChild(1).GetChild(1).gameObject.SetActive(true);
            GameObject.Find("Base").transform.GetChild(3).GetChild(1).GetChild(1).GetChild(2).GetComponent<UnityEngine.UI.Text>().text =
                LangueAPI.StringWithArgument("native", "levelRewardQuantity", gain - lastGain, "x[0]");
        }
        else GameObject.Find("Base").transform.GetChild(3).GetChild(1).GetChild(1).gameObject.SetActive(false);
        Inventory.xmlDefault = xml;
    }
}
