using UnityEngine;

public class CheckPoint : MonoBehaviour {

    Player player;
    void OnTriggerEnter2D(Collider2D collision)
    {
        player = collision.gameObject.GetComponent<Player>();
        player.PositionInitiale = transform.position;
    }
}
