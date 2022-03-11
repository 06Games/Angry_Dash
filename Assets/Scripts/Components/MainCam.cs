using AngryDash.Game;
using UnityEngine;

public class MainCam : MonoBehaviour
{
    public bool OnPlayer = true;

    private void Update()
    {
        if (Player.userPlayer != null & OnPlayer)
        {
            Vector2 playerPos = Player.userPlayer.transform.position;
            transform.position = new Vector3(playerPos.x, playerPos.y + (Screen.height * 0.25F), -10);
        }
    }
}
