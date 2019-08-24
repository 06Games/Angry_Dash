using UnityEngine;

namespace AngryDash.Game.API
{
    public class GameUtilities
    {
        public static void End() { Events.Ending.EndGame(Player.userPlayer); }
        public static void Lose() { LevelPlayer.Lost(GameObject.Find("Base").transform); }
    }
}
