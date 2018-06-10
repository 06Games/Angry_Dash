using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCam : MonoBehaviour {

    public GameObject Player;
	
	void Update () {
        //if(Player.GetComponent<Player>().Ar != null)
            transform.position = new Vector3(Player.transform.position.x, Player.transform.position.y + 300, -10);
        //else transform.position = new Vector3(Player.transform.position.x, Player.transform.position.y, -10);
    }
}
