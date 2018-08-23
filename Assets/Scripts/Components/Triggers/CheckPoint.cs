﻿using UnityEngine;

public class CheckPoint : MonoBehaviour {

    Player player;
    void OnTriggerEnter2D(Collider2D collision)
    {
        player = collision.gameObject.GetComponent<Player>();
        if (player.respawnMode == 1) player.PositionInitiale = transform.position;
        if (player.respawnMode == 1) GetComponent<SpriteRenderer>().color = new Color32(255, 130, 0, 255);
    }

    private void Update()
    {
        if(player != null)
            if (player.PositionInitiale != (Vector2)transform.position)
                GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 255);
    }
}