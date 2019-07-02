using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    AngryDash.Game.Player player;

    void OnTriggerEnter2D(Collider2D collision)
    {
        player = collision.gameObject.GetComponent<AngryDash.Game.Player>();
        if (player.levelSettings.respawnMode == 1) player.PositionInitiale = transform.position;
        if (player.levelSettings.respawnMode == 1) GetComponent<SpriteRenderer>().color = new Color32(255, 130, 0, 255);
    }

    private void Update()
    {
        if (player != null)
            if (player.PositionInitiale != (Vector2)transform.position)
                GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 255);
    }
}
