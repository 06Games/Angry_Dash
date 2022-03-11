using AngryDash.Game.Events;
using AngryDash.Image.Reader;
using UnityEngine;

namespace AngryDash.Game.API
{
    public class GameUtilities
    {
        public static void End() { Ending.EndGame(Player.userPlayer); }
        public static void Lose() { LevelPlayer.Lost(GameObject.Find("Base").transform); }

        public static void BackgroundColor(byte r, byte g, byte b)
        {
            UnityThread.executeInUpdate(() =>
            {
                var color = new Color32(r, g, b, 255);
                foreach (var img in Player.userPlayer.LP.ArrierePlan.GetComponentsInChildren<UnityEngine.UI.Image>()) img.color = color;
            });
        }
        public static void ChangeBackground(string id)
        {
            UnityThread.executeInUpdate(() =>
            {
                foreach (var reader in Player.userPlayer.LP.ArrierePlan.GetComponentsInChildren<UImage_Reader>()) reader.SetID(id).LoadAsync();
            });
        }
    }
}
