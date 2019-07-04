using UnityEngine;

namespace AngryDash.Game.Event.Action
{
    public class GameUtilities
    {
        public static void End() { Ending.EndGame(Player.userPlayer); }
    }
}
