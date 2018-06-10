using CnControls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using PlayerPrefs = PreviewLabs.PlayerPrefs;

public class Player : MonoBehaviour {

    public int[] Sensibility;
    public float Speed;
    public int Sesibility;
    public int x;
    public int y;
    public GameObject Arrivé;
    public Transform Parents;
    public GameObject Ar;
    public GameObject JoyStick;
    public bool PeutAvancer;
    public Vector2 PositionInitiale;

    void Start()
    {
        int playerSkin = PlayerPrefs.GetInt("PlayerSkin");
        Texture2D tex = new Texture2D(1, 1);
        tex.LoadImage(System.IO.File.ReadAllBytes(Application.persistentDataPath + "/Textures/1/" + playerSkin + ".png"));
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
        GetComponent<SpriteRenderer>().sprite = sprite;
    }

    void Update()
    {
        Sesibility = UnityEngine.Random.Range(Sensibility[0], Sensibility[1]);
        int xa = (int)(CnInputManager.GetAxis("Horizontal") * Sesibility); //x actuel
        int ya = (int)(CnInputManager.GetAxis("Vertical") * Sesibility); //y actuel

        bool t = false;
        if (x != 0 | y != 0)
            t = true;

        if (Ar == null)
            JoyStick.SetActive(true);
        else if (!PeutAvancer)
        {
            Destroy(Ar);
            //StartCoroutine(destroy());
        }
        else JoyStick.SetActive(false);

        if (xa == 0 & ya == 0 & t & Ar == null) //si le joueur à lacher le joystick
        {
            Quaternion rot = new Quaternion(0, 0, 0, 0);
            Vector3 pos = new Vector3(transform.position.x - x, transform.position.y - y, 0);
            Ar = Instantiate(Arrivé, pos, rot, Parents);
            Ar.name = "Arrivé";

            float px = Ar.transform.position.x - transform.position.x;
            float py = Ar.transform.position.y - transform.position.y;

            PositionInitiale = transform.position;
            StartCoroutine(Navigate(px, py));
        }
            //transform.position = new Vector2(transform.position.x - x, transform.position.y - y);

        x = xa; //x d'avant
        y = ya; //y d'avant
    }

    public IEnumerator Navigate(float px, float py)
    {
        int i = 0;
        while (i < Speed)
        {
            if (PeutAvancer)
            {
                transform.position = new Vector2(transform.position.x + px / Speed, transform.position.y + py / Speed);
                i++;
                yield return new WaitForSeconds(0.01F);
            }
            else i = (int)Speed;
        }
        StartCoroutine(destroy());
    }

    public IEnumerator destroy()
    {
        yield return new WaitForSeconds(0.5F);
        Destroy(Ar);
        //PeutAvancer = true;
    }
}   
