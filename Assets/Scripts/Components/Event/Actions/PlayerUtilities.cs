﻿using UnityEngine;

namespace AngryDash.Game.Event.Action
{
    public class PlayerUtilities
    {
        public static void Teleport(double x, double y) { Player.userPlayer.transform.position = new Vector2((float)x * 50F, (float)y * 50F); }
    }
}