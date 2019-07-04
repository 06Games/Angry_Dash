using UnityEngine;

namespace AngryDash.Game.Event.Action
{
    public class PlayerUtilities
    {
        static Vector2 Coordinate(double x, double y) { return new Vector2((float)x * 50F + 25, (float)y * 50F + 25); }

        public static void Teleport(double x, double y) { Player.userPlayer.transform.position = Coordinate(x, y); }
        public static void Checkpoint(double x, double y) { Player.userPlayer.PositionInitiale = Coordinate(x, y); }
    }
}
