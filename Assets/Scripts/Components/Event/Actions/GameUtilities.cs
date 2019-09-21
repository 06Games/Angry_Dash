using UnityEngine;

namespace AngryDash.Game.API
{
    public class GameUtilities
    {
        public static void End() { Events.Ending.EndGame(Player.userPlayer); }
        public static void Lose() { LevelPlayer.Lost(GameObject.Find("Base").transform); }

        public static void BackgroundColor(byte r, byte g, byte b)
        {
            Color32 color = new Color32(r, g, b, 255);
            foreach (var img in Player.userPlayer.LP.ArrierePlan.GetComponentsInChildren<UnityEngine.UI.Image>())
                img.color = color;
        }
    }
}
