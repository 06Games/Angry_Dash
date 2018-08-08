using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCam : MonoBehaviour
{
    public GameObject Player;

    void Update()
    {
        if (Player != null)
            transform.position = new Vector3(Player.transform.position.x, Player.transform.position.y + 300, -10);
    }
}
