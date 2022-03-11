using System;
using System.Collections;
using System.Diagnostics;
using AngryDash.Image.Reader;
using CnControls;
using Tools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AngryDash.Game
{
    public class Player : MonoBehaviour
    {
        public static Player userPlayer;

        //Dépendances
        public LevelPlayer LP;
        public Transform Trace;

        //Joystick
        public GameObject JoyStick; //The joystick
        private Vector2 joystickOffset; //The joystick's offset on screen
        public Vector3 Sensibility; //Min et max de l'aléatoire + Sensibilité actuelle (distance en nombre blocs)
        private Vector2 joystickPos; //pos du joystick

        //Avancer
        public bool PeutAvancer; //Pas de mur
        public Vector2 PositionInitiale; //Dernier point d'arrivé valide
        public bool Touched; //Le joueur est-il déjà géré par un bloc
        private bool Moving; //Le joueur est en mouvement ?

        //Paramètres
        private string selectedTrace = "0"; //Trace Type
        public float vitesse = 1; //Multiplicateur de la vitesse du joueur
        public Level.Player levelSettings; //Autres paramètres défini par le niveau

        //Evenements
        public Action onRespawn;

        private void Start()
        {
            if (LP == null) LP = GameObject.Find("Main Camera").GetComponent<LevelPlayer>();
            if (JoyStick == null) JoyStick = GameObject.Find("SensitiveJoystick");
            if (Trace == null) Trace = GameObject.Find("Traces").transform;

#if EnableMultiplayer
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Online")
            {
                if (GetComponent<NetworkIdentity>().isLocalPlayer)
                {
                    userPlayer = this;
                    GetComponent<SpriteRenderer>().color = new Color32(255, 185, 0, 255);
                    GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                }
                else enabled = false;
            }
            else
#endif
            userPlayer = this;

            var xml = Inventory.xmlDefault;
            GetComponent<UImage_Reader>().SetID("native/PLAYERS/" + Inventory.GetSelected(xml, "native/PLAYERS/")).LoadAsync(); //Player Image
            selectedTrace = Inventory.GetSelected(xml, "native/TRACES/"); //Trace Image

#if UNITY_ANDROID || UNITY_IOS
            var dpi = Screen.dpi;
            if (dpi < 25 | dpi > 1000) dpi = 150;
            var size = (96 / 1080F) / (dpi / Screen.height) * (325F / 1080) * Screen.height;
#else
            float size = 0.2F * Screen.height;
#endif
            JoyStick.GetComponent<RectTransform>().sizeDelta = new Vector2(size, size);
            var rect = JoyStick.GetComponent<RectTransform>().rect;
            joystickOffset = new Vector2((rect.width * JoyStick.transform.parent.GetComponent<Canvas>().scaleFactor) / 2,
                (rect.height * JoyStick.transform.parent.GetComponent<Canvas>().scaleFactor) / 2);

            vitesse = 1;
            PositionInitiale = transform.position;
        }

        private void Update()
        {
            JoyStick.GetComponent<RectTransform>().position = (Vector2)LP.GetComponent<Camera>().WorldToScreenPoint(transform.position) - joystickOffset;
            Sensibility.z = Random.Range(Sensibility.x, Sensibility.y);
            var newJoystickPos = new Vector2(CnInputManager.GetAxis("Horizontal"), CnInputManager.GetAxis("Vertical"));
            JoyStick.SetActive(!Moving);

            if (newJoystickPos == new Vector2(0, 0) & joystickPos != new Vector2(0, 0) & !Moving & PeutAvancer) //si le joueur a laché le joystick
            {
                LP.nbLancer = LP.nbLancer + 1;
                if (levelSettings.respawnMode == 0) PositionInitiale = transform.position;
                levelSettings = LP.level.player.DeepClone();
                StartCoroutine(Navigate(joystickPos * new Vector2(-1, 1)));
            }

            joystickPos = newJoystickPos;
        }

        public IEnumerator Navigate(Vector2 joystickPos)
        {
            Moving = true;

            //Rotation du Player
            var adjacent = joystickPos.x;
            var oppose = joystickPos.y;
            var hypothenuse = Mathf.Sqrt(Mathf.Pow(adjacent, 2) + Mathf.Pow(oppose, 2));
            var cos = adjacent / hypothenuse;
            double z = (Mathf.Acos(cos) * 180) / Mathf.PI;
            if (transform.position.y < transform.position.y - joystickPos.y) z -= 90;
            else z = z * -1 - 90;
            var rot = new Quaternion();
            rot.eulerAngles = new Vector3(0, 0, (float)z);
            transform.rotation = rot;

            var MoveTime = TimeSpan.FromSeconds(0.75F / vitesse);
            var stopwatch = new Stopwatch();
            Vector2 InitialPos = transform.position;

            var traceObj = CreateTrace();
            var endPos = InitialPos;

            var LastFrame = true;
            long lastTime = 0;
            stopwatch.Start();
            while ((stopwatch.Elapsed < MoveTime | LastFrame) & PeutAvancer)
            {
                var mouvement = levelSettings.distance * 50F * joystickPos;
                var Mouvement = Mathf.Sqrt(Mathf.Pow(mouvement.x, 2) + Mathf.Pow(mouvement.y, 2));
                if (vitesse <= 0)
                {
                    stopwatch.Stop();
                    yield return new WaitWhile(() => (vitesse <= 0));
                    stopwatch.Start();
                }
                else
                {
                    MoveTime = TimeSpan.FromSeconds(0.75F / vitesse);

                    var Time = stopwatch.ElapsedMilliseconds;
                    if (stopwatch.Elapsed >= MoveTime)
                    {
                        LastFrame = false;
                        Time = (long)MoveTime.TotalMilliseconds;
                    }

                    var maxDistance = ((float)MoveTime.TotalMilliseconds / 2) + 1;
                    float moveFrame = 0;
                    for (var i = lastTime; i < Time + 1; i++)
                    {
                        var vi = i + 1;
                        if (i > MoveTime.TotalMilliseconds / 2F)
                            vi = (long)MoveTime.TotalMilliseconds - i;
                        moveFrame = moveFrame + ((Mouvement / ((maxDistance / vi) / maxDistance)) / (long)MoveTime.TotalMilliseconds / (maxDistance / 2F));
                    }
                    lastTime = Time + 1;
                    transform.Translate(new Vector2(0, moveFrame), Space.Self);

                    if (rot != transform.rotation) //If the player bounced, start a new trace
                    {
                        TraceEnd(traceObj, endPos); //End the old trace
                        traceObj = CreateTrace(); //Create another
                    }

                    var imgSize = traceObj.GetComponent<UImage_Reader>().FrameSize;
                    traceObj.localScale = new Vector2(100F / imgSize.x * 25, Vector2Extensions.Distance(transform.position, InitialPos) * 100F / imgSize.y);
                    traceObj.position = Vector2Extensions.Center(transform.position, InitialPos);
                    traceObj.rotation = transform.rotation;
                    rot = transform.rotation;
                    endPos = transform.position;

                    yield return new WaitForEndOfFrame();
                }
            }
            TraceEnd(traceObj, endPos);

            stopwatch.Stop();
            levelSettings = LP.level.player.DeepClone();
            vitesse = 1;
            Moving = false;
        }

        private Transform CreateTrace()
        {
            var traceObj = new GameObject("Trace Obj").transform;
            traceObj.parent = Trace;
            traceObj.gameObject.AddComponent<SpriteRenderer>().sortingOrder = 32766;
            traceObj.gameObject.AddComponent<UImage_Reader>().SetID("native/TRACES/" + selectedTrace + "/Moving").LoadAsync();
            return traceObj;
        }

        private void TraceEnd(Transform traceObj, Vector2 endPos)
        {
            var endPointID = "native/TRACES/" + selectedTrace + "/SuccessEnd";
            if ((Vector2)transform.position == PositionInitiale)
            {
                traceObj.GetComponent<UImage_Reader>().SetID("native/TRACES/" + selectedTrace + "/Missed").LoadAsync();
                endPointID = "native/TRACES/" + selectedTrace + "/MissedEnd";
            }
            else traceObj.GetComponent<UImage_Reader>().SetID("native/TRACES/" + selectedTrace + "/Success").LoadAsync();
            var endPoint = new GameObject("Trace End Point");
            endPoint.transform.parent = Trace;
            endPoint.transform.localScale = new Vector2(50 / 64F * 50F, 50 / 64F * 50F);
            endPoint.transform.position = endPos;
            endPoint.AddComponent<SpriteRenderer>().sortingOrder = 32766;
            endPoint.AddComponent<UImage_Reader>().baseID = endPointID;
            StartCoroutine(TraceDespawn(traceObj.GetComponent<SpriteRenderer>(), endPoint.GetComponent<SpriteRenderer>()));
        }

        private IEnumerator TraceDespawn(SpriteRenderer traceObj, SpriteRenderer endPoint)
        {
            var actualStage = LP.nbLancer + 5;
            yield return new WaitWhile(() => LP.nbLancer < actualStage);

            var obj = traceObj.color;
            var end = endPoint.color;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (stopwatch.Elapsed.TotalMilliseconds < 2000)
            {
                var a = (2000 - (int)stopwatch.Elapsed.TotalMilliseconds) / 2000F;
                Apply(a);
                yield return new WaitForEndOfFrame();
            }
            Apply(0);
            stopwatch.Stop();

            void Apply(float a)
            {
                if (obj.a > a) obj.a = a;
                if (end.a > a) end.a = a;

                traceObj.color = obj;
                endPoint.color = end;
            }
        }
    }
}
