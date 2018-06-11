using CnControls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using PlayerPrefs = PreviewLabs.PlayerPrefs;

public class Player : MonoBehaviour {

    //Dépendances
    public LevelPlayer LP;

    //Joystick
    public GameObject JoyStick; //Le Joystick
    public int[] Sensibility; //Min et max de l'aléatoire
    public int Sesibility; //Sesi actuelle (distance du point d'arrivé)
    int x; //pos x du joystick
    int y; //pos y du joystick

    //Paramètres
    float Speed = 60; //Vitesse de déplacement

    //Point d'arrivé
    public GameObject Arrivé; //Prefab de l'arrivé
    public Transform Parents; //Zone Unity pour spawn de l'arrivé
    public GameObject Ar; //Pt d'arrivé actuel

    //Avancer
    public bool PeutAvancer; //Pas de mur
    public Vector2 PositionInitiale; //Dernier point d'arrivé valide
    
    public int move = 0;
    public float vitesse = 1;

    void Start()
    {
        int playerSkin = PlayerPrefs.GetInt("PlayerSkin");
        Texture2D tex = new Texture2D(1, 1);
        tex.LoadImage(System.IO.File.ReadAllBytes(Application.persistentDataPath + "/Textures/1/" + playerSkin + ".png"));
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
        GetComponent<SpriteRenderer>().sprite = sprite;
        vitesse = 1;
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
        }
        else JoyStick.SetActive(false);

        if (xa == 0 & ya == 0 & t & Ar == null & move == 0) //si le joueur a laché le joystick
        {
            LP.nbLancer = LP.nbLancer + 1;
            Quaternion rot = new Quaternion(0, 0, 0, 0);
            Vector3 pos = new Vector3(transform.position.x - x, transform.position.y - y, 0);
            Ar = Instantiate(Arrivé, pos, rot, Parents);
            Ar.name = "Arrivé";

            float px = Ar.transform.position.x - transform.position.x;
            float py = Ar.transform.position.y - transform.position.y;

            PositionInitiale = transform.position;
                StartCoroutine(Navigate(px, py));
        }

        x = xa; //x d'avant
        y = ya; //y d'avant
    }

    public IEnumerator Navigate(float px, float py)
    {
        move = 0;

        float adjacent = Ar.transform.position.x - transform.position.x;
        float oppose = Ar.transform.position.y - transform.position.y;
        float hypothenuse = (float)Math.Sqrt(Math.Pow(adjacent,2) + Math.Pow(oppose, 2));
        float cos = adjacent / hypothenuse;
        double z = (Math.Acos(cos) * 180) / Mathf.PI;
        
        if (transform.position.y < Ar.transform.position.y)
            z = z - 90;
        else z = z * -1 - 90;
        
        Quaternion rot = new Quaternion();
        rot.eulerAngles = new Vector3(0,0,(float)z);
        transform.rotation = rot;

        while (move < Speed)
        {
            if (PeutAvancer)
            {
                float v = vitesse * (move * 20 / (Speed / 2));
                if (move > Speed / 2)
                    v = vitesse * (move / -(Speed / 2) + 2);
                transform.Translate(Vector3.up * v, Space.Self);

                //transform.localPosition = new Vector2(transform.localPosition.x + px / Speed, transform.localPosition.y + py / Speed);
                move++;
                yield return new WaitForSeconds(0.01F);
            }
            else move = (int)Speed;
        }
        move = 0;
        vitesse = 1;
        StartCoroutine(destroy());
    }

    public IEnumerator destroy()
    {
        yield return new WaitForSeconds(0.5F);
        Destroy(Ar);
    }
}   
