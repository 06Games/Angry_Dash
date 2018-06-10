using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mur : MonoBehaviour {

    Player player;
    public bool trigger;
    void OnTriggerEnter2D(Collider2D collision)
    {
        player = collision.gameObject.GetComponent<Player>();
        trigger = true;

        player.PeutAvancer = false;
        StartCoroutine(colid());
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        trigger = false;
    }

    private void Update()
    {
        //if(trigger)
    }

    IEnumerator colid()
    {
        yield return new WaitForSeconds(0.1F);
        /*if(trigger)
        {
            if (player.PositionInitiale.x < transform.position.x)
                player.transform.position = new Vector2(player.transform.position.x - 2, player.transform.position.y);
            else if (player.PositionInitiale.x > transform.position.x)
                player.transform.position = new Vector2(player.transform.position.x + 2, player.transform.position.y);
            else if (player.PositionInitiale.y < transform.position.y)
                player.transform.position = new Vector2(player.transform.position.x, player.transform.position.y - 2);
            else if (player.PositionInitiale.y > transform.position.y)
                player.transform.position = new Vector2(player.transform.position.x, player.transform.position.y + 2);

            StartCoroutine(colid());
        }
        else*/ player.PeutAvancer = true;
        /*if (player.PositionInitiale.x < transform.position.x)
            player.transform.position = new Vector2(player.transform.position.x - 2, player.transform.position.y);
        else if (player.PositionInitiale.x > transform.position.x)
            player.transform.position = new Vector2(player.transform.position.x + 2, player.transform.position.y);
        else if (player.PositionInitiale.y < transform.position.y)
            player.transform.position = new Vector2(player.transform.position.x, player.transform.position.y - 2);
        else if (player.PositionInitiale.y > transform.position.y)
            player.transform.position = new Vector2(player.transform.position.x, player.transform.position.y + 2);*/
    }
}
