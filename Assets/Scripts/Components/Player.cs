using CnControls;
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
    public Vector3 Sensibility; //Min et max de l'aléatoire + Sensibilité actuelle (distance en nombre blocs)
    int x; //pos x du joystick
    int y; //pos y du joystick

    //Point d'arrivé
    //public Transform Parents; //Zone Unity pour spawn de l'arrivé
    Vector2 Ar = new Vector2(); //Pt d'arrivé actuel

    //Avancer
    public bool PeutAvancer = false; //Pas de mur
    public Vector2 PositionInitiale; //Dernier point d'arrivé valide

    //Paramètres
    public float vitesse = 1; //Multiplicateur de la vitesse du joueur
    public int respawnMode = 0; //Action à effectuer en cas de mort


    void Start()
    {
        if (LP == null)
            LP = GameObject.Find("Main Camera").GetComponent<LevelPlayer>();
        if (JoyStick == null)
            JoyStick = GameObject.Find("SensitiveJoystick");
        /*if (Parents == null)
            Parents = GameObject.Find("Base").transform;*/

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

        FileFormat.XML.RootElement xml = Inventory.xmlDefault;
        int playerSkin = int.Parse(xml.GetItem("SelectedItems").GetItemByAttribute("item", "category", "native/PLAYERS/").Value);
        if (!Inventory.Owned(xml, playerSkin.ToString()))
        {
            xml.GetItem("SelectedItems").GetItemByAttribute("item", "category", "native/PLAYERS/").Value = "0";
            playerSkin = 0;
            Inventory.xmlDefault = xml;
        }
        GetComponent<UImage_Reader>().baseID = "native/PLAYERS/" + playerSkin;
        GetComponent<UImage_Reader>().Load();


        vitesse = 1;

        JoyStick.GetComponent<RectTransform>().position = new Vector2(
            Screen.width / 2 - (JoyStick.GetComponent<RectTransform>().rect.width * JoyStick.transform.parent.GetComponent<Canvas>().scaleFactor) / 2,
            Display.Screen.Resolution.y * 0.2F - (JoyStick.GetComponent<RectTransform>().rect.height * JoyStick.transform.parent.GetComponent<Canvas>().scaleFactor) / 2);

        PositionInitiale = transform.position;
    }

    void Update()
    {
        Sensibility.z = Random.Range(Sensibility.x, Sensibility.y);
        int xa = (int)(CnInputManager.GetAxis("Horizontal") * 50 * Sensibility.z); //x actuel
        int ya = (int)(CnInputManager.GetAxis("Vertical") * 50 * Sensibility.z); //y actuel

        bool t = false;
        if (x != 0 | y != 0)
            t = true;

        if (Ar == new Vector2())
            JoyStick.SetActive(true);
        else JoyStick.SetActive(false);


        if (xa == 0 & ya == 0 & t & Ar == new Vector2() & PeutAvancer) //si le joueur a laché le joystick
        {
            LP.nbLancer = LP.nbLancer + 1;
            Vector3 pos = new Vector3(transform.position.x - x, transform.position.y - y, 0);
            Ar = pos;

            if (respawnMode == 0) PositionInitiale = transform.position;
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
        float hypothenuse = Mathf.Sqrt(Mathf.Pow(adjacent, 2) + Mathf.Pow(oppose, 2));
        float cos = adjacent / hypothenuse;
        double z = (Mathf.Acos(cos) * 180) / Mathf.PI;
        if (transform.position.y < Ar.y)
            z = z - 90;
        else z = z * -1 - 90;
        Quaternion rot = new Quaternion();
        rot.eulerAngles = new Vector3(0, 0, (float)z);
        transform.rotation = rot;

        System.TimeSpan MoveTime = System.TimeSpan.FromSeconds(0.75F / vitesse);
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        Vector2 InitialPos = transform.position;
        float Mouvement = Mathf.Sqrt(Mathf.Pow(Ar.x - InitialPos.x, 2) + Mathf.Pow(Ar.y - InitialPos.y, 2));

        bool LastFrame = true;
        long lastTime = 0;
        stopwatch.Start();
        while ((stopwatch.Elapsed < MoveTime | LastFrame) & PeutAvancer)
        {
            if (vitesse <= 0)
            {
                stopwatch.Stop();
                yield return new WaitWhile(() => (vitesse <= 0));
                stopwatch.Start();
            }
            else
            {
                MoveTime = System.TimeSpan.FromSeconds(0.75F / vitesse);

                long Time = stopwatch.ElapsedMilliseconds;
                if (stopwatch.Elapsed >= MoveTime)
                {
                    LastFrame = false;
                    Time = (long)MoveTime.TotalMilliseconds;
                }

                float maxDistance = ((float)MoveTime.TotalMilliseconds / 2) + 1;
                float moveFrame = 0;
                for (long i = lastTime; i < Time + 1; i++)
                {
                    long vi = i + 1;
                    if (i > MoveTime.TotalMilliseconds / 2F)
                        vi = (long)MoveTime.TotalMilliseconds - i;
                    moveFrame = moveFrame + ((Mouvement / ((maxDistance / vi) / maxDistance)) / (long)MoveTime.TotalMilliseconds / (maxDistance / 2F));
                }
                lastTime = Time + 1;
                transform.Translate(new Vector2(0, moveFrame), Space.Self);

                yield return new WaitForEndOfFrame();
            }
        }
        stopwatch.Stop();
        Ar = new Vector2();
        vitesse = 1;
    }
}
