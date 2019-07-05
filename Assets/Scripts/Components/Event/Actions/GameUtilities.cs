using UnityEngine;

namespace AngryDash.Game.Event.Action
{
    public class GameUtilities : MonoBehaviour
    {
        EventUtilities EventUtilities;

        public static void End() { Ending.EndGame(Player.userPlayer); }
        public static void Lose() { LevelPlayer.Lost(GameObject.Find("Base").transform); }
    }
}
