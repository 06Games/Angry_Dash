using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ending : MonoBehaviour {

    Player player;
    void OnTriggerEnter2D(Collider2D collision)
    {
        player = collision.gameObject.GetComponent<Player>();
        player.PeutAvancer = false;

        GameObject.Find("Base").transform.GetChild(3).gameObject.SetActive(true);
    }
}
