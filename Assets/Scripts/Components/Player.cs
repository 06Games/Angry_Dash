using CnControls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using PlayerPrefs = PreviewLabs.PlayerPrefs;

public class Player : MonoBehaviour
{

    //Dépendances
    public LevelPlayer LP;

    //Joystick
    public GameObject JoyStick; //Le Joystick
    public int[] Sensibility; //Min et max de l'aléatoire
    public int Sesibility; //Sesi actuelle (distance du point d'arrivé)
    int x; //pos x du joystick
    int y; //pos y du joystick

    //Point d'arrivé
    public GameObject Arrivé; //Prefab de l'arrivé
    public Transform Parents; //Zone Unity pour spawn de l'arrivé
    public Vector2 Ar = new Vector2(); //Pt d'arrivé actuel

    //Avancer
    public bool PeutAvancer; //Pas de mur
    public Vector2 PositionInitiale; //Dernier point d'arrivé valide

    //Paramètres
    public int move = 0; //Frame actuel du déplacemnt du joueur
    public float vitesse = 1; //Multiplicateur de la vitesse du joueur
    public int respawnMode = 0; //Action à effectuer en cas de mort


    void Start()
    {
        if (LP == null)
            LP = GameObject.Find("Main Camera").GetComponent<LevelPlayer>();
        if (JoyStick == null)
            JoyStick = GameObject.Find("SensitiveJoystick");
        if (Parents == null)
            Parents = GameObject.Find("Base").transform;

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Online")
        {
            if (GetComponent<NetworkIdentity>().isLocalPlayer)
            {
                LP.GetComponent<MainCam>().Player = gameObject;
                GetComponent<SpriteRenderer>().color = new Color32(255, 185, 0, 255);
                GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
            }
            else GetComponent<Player>().enabled = false;
        }

        int playerSkin = PlayerPrefs.GetInt("PlayerSkin");
        Texture2D tex = new Texture2D(1, 1);
        tex.LoadImage(System.IO.File.ReadAllBytes(Application.persistentDataPath + "/Textures/1/" + playerSkin + ".png"));
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
        GetComponent<SpriteRenderer>().sprite = sprite;
        vitesse = 1;

        float resolutionDecalage = ((Screen.height / 2F) - ((1080F / 1920F * Screen.width) / 2F)) / 2F;
        JoyStick.GetComponent<RectTransform>().anchoredPosition = new Vector2(-110, -308 - resolutionDecalage);

        PositionInitiale = transform.position;
    }

    void Update()
    {
        Sesibility = UnityEngine.Random.Range(Sensibility[0], Sensibility[1]);
        int xa = (int)(CnInputManager.GetAxis("Horizontal") * Sesibility); //x actuel
        int ya = (int)(CnInputManager.GetAxis("Vertical") * Sesibility); //y actuel

        bool t = false;
        if (x != 0 | y != 0)
            t = true;

        if (Ar == new Vector2())
            JoyStick.SetActive(true);
        else JoyStick.SetActive(false);

        if (xa == 0 & ya == 0 & t & Ar == new Vector2() & move == 0) //si le joueur a laché le joystick
        {
            LP.nbLancer = LP.nbLancer + 1;
            Vector3 pos = new Vector3(transform.position.x - x, transform.position.y - y, 0);
            Ar = pos;

            if(respawnMode == 0) PositionInitiale = transform.position;
            StartCoroutine(Navigate());
        }

        x = xa; //x d'avant
        y = ya; //y d'avant
    }

    public IEnumerator Navigate()
    {
        //Rotation du Player
        float adjacent = Ar.x - transform.position.x;
        float oppose = Ar.y - transform.position.y;
        float hypothenuse = (float)Math.Sqrt(Math.Pow(adjacent, 2) + Math.Pow(oppose, 2));
        float cos = adjacent / hypothenuse;
        double z = (Math.Acos(cos) * 180) / Mathf.PI;
        if (transform.position.y < Ar.y)
            z = z - 90;
        else z = z * -1 - 90;
        Quaternion rot = new Quaternion();
        rot.eulerAngles = new Vector3(0, 0, (float)z);
        transform.rotation = rot;


        move = 0; //Reset le nb de Frame du Player
        int FPS = int.Parse(Parents.GetChild(1).GetComponent<Text>().text.Replace(" FPS", "")); //Recup le nombre de FPS
        while (move < FPS)
        {
            if (PeutAvancer)
            {
                float v = move + 1;
                if (move > FPS / 2)
                    v = FPS - move;
                v = 5 * (v / (FPS / 6));

                float Mouvement = vitesse * (1 / (FPS / 60F)) * v;
                transform.Translate(new Vector2(0, Mouvement), Space.Self);
                move++;
                yield return new WaitForSeconds(0.00001F);
            }
            else move = FPS;
        }
        move = 0;
        vitesse = 1;
        Ar = new Vector2();
    }
}
