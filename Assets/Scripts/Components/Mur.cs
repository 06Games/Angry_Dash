using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mur : MonoBehaviour {

    Player player;
    public bool trigger;
    public int colider;

    void OnTriggerEnter2D(Collider2D collision)
    {
        player = collision.gameObject.GetComponent<Player>();
        trigger = true;

        if (colider == 0) //Stop
        {
            player.PeutAvancer = false;
            StartCoroutine(colid(0.5F));
        }
        else if(colider == 1) //Kill
        {
            player.PeutAvancer = false;
            player.transform.position = player.PositionInitiale;
            StartCoroutine(colid(0.1F));
        }
        else if (colider == 3) //Bounce
        {
            Vector3 rotpos = transform.rotation.eulerAngles;
            Quaternion rot = new Quaternion();
            rot.eulerAngles = new Vector3(0, 0, rotpos.z+45);
            player.transform.rotation = rot;

        }
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        trigger = false;
    }

    private void Update()
    {

    }

    IEnumerator colid(float wait)
    {
        yield return new WaitForSeconds(wait);
        player.PeutAvancer = true;


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
        else player.PeutAvancer = true;
        if (player.PositionInitiale.x < transform.position.x)
            player.transform.position = new Vector2(player.transform.position.x - 2, player.transform.position.y);
        else if (player.PositionInitiale.x > transform.position.x)
            player.transform.position = new Vector2(player.transform.position.x + 2, player.transform.position.y);
        else if (player.PositionInitiale.y < transform.position.y)
            player.transform.position = new Vector2(player.transform.position.x, player.transform.position.y - 2);
        else if (player.PositionInitiale.y > transform.position.y)
            player.transform.position = new Vector2(player.transform.position.x, player.transform.position.y + 2);*/
    }
}
