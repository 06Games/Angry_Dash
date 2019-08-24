using UnityEngine;

namespace AngryDash.Game.API
{
    public class PlayerUtilities
    {
        static Vector2 Coordinate(float x, float y) { return new Vector2(x * 50F + 25, y * 50F + 25); }

        public static void Teleport(float x, float y) { Player.userPlayer.transform.position = Coordinate(x, y); }
        public static void Checkpoint(float x, float y) { Player.userPlayer.PositionInitiale = Coordinate(x, y); }
    }
}
