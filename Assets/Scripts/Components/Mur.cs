using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mur : MonoBehaviour
{

    Player player;
    public float colider;
    public float boostMultiplier = 0;
    public float blockID = 1;

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
            player.vitesse = (boostMultiplier/10F) + 1;
        else if ((int)colider == 3) //Bounce
        {
            Vector3 rotpos = player.transform.rotation.eulerAngles;
            Quaternion rot = new Quaternion();

            float z = rotpos.z + 180;
            if ((int)blockID == 2 | (int)blockID == 3)
            {            
                Vector2 pos = player.transform.position;
                Vector2 direction = new Vector2();

                if (pos.x > (transform.position.x + transform.lossyScale.x / 2))
                    direction.x = 1;
                else if (pos.x < (transform.position.x - transform.lossyScale.x / 2))
                    direction.x = -1;
                else direction.x = 0;

                if (pos.y > (transform.position.y + transform.lossyScale.y / 2))
                    direction.y = 1;
                else if (pos.y < (transform.position.y - transform.lossyScale.y / 2))
                    direction.y = -1;
                else direction.y = 0;
                

                if (direction.x == 1 & direction.y == 0)
                    z = 0;
                else if (direction.x == 0 & direction.y == 1)
                    z = -90;
                else if (direction.x == 0 & direction.y == 0) //Bug, le player arrive trop vite
                    z = 0;
                else z = rotpos.z + 180;

                if(transform.rotation.eulerAngles.z >= 0)
                z = z + (int)transform.rotation.eulerAngles.z;
                else z = z + 180 + ((int)transform.rotation.eulerAngles.z*-1);
            }

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
