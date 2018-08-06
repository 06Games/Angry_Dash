using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mur : MonoBehaviour {

    Player player;
    public float colider;
    public float boostMultiplier = 0;

    void OnTriggerEnter2D(Collider2D collision)
    {
        player = collision.gameObject.GetComponent<Player>();

        if ((int)colider == 0) //Stop
        {
            player.PeutAvancer = false;
            player.vitesse = 0;
            StartCoroutine(colid(0.5F));
        }
        else if ((int)colider == 1) //Kill
        {
            player.PeutAvancer = false;
            player.vitesse = 0;
            player.transform.position = player.PositionInitiale;
            StartCoroutine(colid(0.1F));
        }
        else if (colider >= 2.1F & colider < 3) //No Collision + Boost
            player.vitesse = boostMultiplier;
        else if ((int)colider == 3) //Bounce
        {
            Vector3 rotpos = player.transform.rotation.eulerAngles;
            Quaternion rot = new Quaternion();

            float z = rotpos.z;
            if (z > 180)
                z = 180 - z;
            else if (z < -180)
                z = 360 + z;
            
            if (rotpos.z < 90 & rotpos.z > 0)
                z = z - 90;
            else if (rotpos.z < -90 & rotpos.z > -180)
                z = z - 90;
            else if (rotpos.z > 90 & rotpos.z < 180)
                z = z - 90;
            else z = z + 90;

            if (z > 180)
                z = 180 - z;
            else if (z < -180)
                z = 360 + z;

            rot.eulerAngles = new Vector3(0, 0, z);
            player.transform.rotation = rot;
            if (colider >= 3.1F & boostMultiplier > 0)
                player.vitesse = boostMultiplier;
        }
    }
    void OnTriggerExit2D(Collider2D collision)
    {
    }

    private void Start()
    {
        if ((int)colider == 0 | (int)colider == 3)
            GetComponents<Collider2D>()[1].enabled = true;
        else GetComponents<Collider2D>()[1].enabled = false;

        if (colider != (int)colider)
            boostMultiplier = int.Parse(colider.ToString().Split(new string[1] { "." }, System.StringSplitOptions.None)[1]);
        else boostMultiplier = 0;
    }

    IEnumerator colid(float wait)
    {
        yield return new WaitForSeconds(wait);
        player.PeutAvancer = true;
        player.vitesse = 1;
    }
}
