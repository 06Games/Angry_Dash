using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LevelPlayer : MonoBehaviour {


    string file;
    string FromScene;
    string[] component;
    int BlocSize = 24;
    Camera cam;

    public GameObject[] Prefabs;
    public Transform SummonPlace;

    public Sprite[] ArrierePlanS;
    public Transform ArrierePlan;

    Vector3 EndPoint;
    public GameObject[] TriggerPref;
    public Transform Base;

    public int nbLancer;
    public Text nbLancerTxt;

    public string[] SongName;
    public string[] SongPath;

    private void Update()
    {
        nbLancerTxt.text = LangueAPI.StringWithArgument("playerTurn", new string[1] { nbLancer.ToString() });
    }

    private void Start()
    {
        NewStart();
    }
    void NewStart()
    {
        BlocSize = Screen.height / 50;

        cam = GetComponent<Camera>();

        cam.transform.position = new Vector3(Screen.width / 2, Screen.height / 2, -10);
        cam.GetComponent<Camera>().orthographicSize = Screen.height / 2;

        if (File.Exists(Application.temporaryCachePath + "/play.txt"))
        {
            if (File.ReadAllLines(Application.temporaryCachePath + "/play.txt").Length > 1)
            {
                file = File.ReadAllLines(Application.temporaryCachePath + "/play.txt")[0];

                if (File.ReadAllLines(Application.temporaryCachePath + "/play.txt").Length > 1)
                    FromScene = File.ReadAllLines(Application.temporaryCachePath + "/play.txt")[1];

                File.WriteAllText(Application.temporaryCachePath + "/play.txt", "");

                string scene = FromScene;
                if (FromScene == "")
                    scene = "Home";
                if (file == "")
                    GetComponent<BaseControl>().LSC.LoadScreen(scene);

                component = File.ReadAllLines(file);

                if (file.Contains(Application.temporaryCachePath))
                    File.Delete(file);

                int a = -1;
                for (int x = 0; x < component.Length; x++)
                {
                    if (component[x].Contains("background = ") & a == -1)
                        a = x;
                }
                string back = "1; 4B4B4B255";
                if (a != -1)
                    back = component[a].Replace("background = ", "");
                string[] Ar = back.Split(new string[1] { "; " }, System.StringSplitOptions.None);
                for (int i = 0; i < ArrierePlan.childCount; i++)
                {
                    Image Im = ArrierePlan.GetChild(i).GetComponent<Image>();
                    Im.sprite = ArrierePlanS[int.Parse(Ar[0])];
                    Im.color = HexToColor(Ar[1]);
                }

                int d = -1;
                for (int x = 0; x < component.Length; x++)
                {
                    if (component[x].Contains("Blocks {") & d == -1)
                        d = x + 1;
                }
                int end = -1;
                for (int i = d; i < component.Length; i++)
                {
                    if (component[i].Contains("}") & end == -1)
                        end = i;
                }

                for (int i = d; i < end; i++)
                    Instance(i);
                transform.GetChild(0).gameObject.SetActive(true);

                Base.GetChild(3).gameObject.SetActive(false);
                string[] fileDir = file.Split(new string[1] { "/" }, System.StringSplitOptions.None);
                Base.GetChild(3).GetChild(0).GetComponent<Text>().text = fileDir[fileDir.Length - 1].Replace(".level", "");
                
                int m = -1;
                for (int x = 0; x < component.Length; x++)
                {
                    if (component[x].Contains("music = ") & m == -1)
                        m = x;
                }
                string music = "";
                if (m != -1)
                    music = Application.persistentDataPath + "/Musics/" + component[m].Replace("music = ", "");
                
                if (GameObject.Find("Audio") != null & music != "null")
                    GameObject.Find("Audio").GetComponent<menuMusic>().LoadMusic(music);
            }
            else
            {
                File.WriteAllLines(Application.temporaryCachePath + "/play.txt", new string[2] { Application.persistentDataPath + "/Level/Solo/Level 1.level", "Home" });
                NewStart();
            }
        }
        else
        {
            File.WriteAllLines(Application.temporaryCachePath + "/play.txt", new string[2] { Application.persistentDataPath + "/Level/Solo/Level 1.level", "Home" });
            NewStart();
        }
    }

    public void Instance(int num)
    {
        float id = float.Parse(component[num].Split(new string[] { "; " }, System.StringSplitOptions.None)[0]);
        string rotZ = component[num].Split(new string[] { "; " }, System.StringSplitOptions.None)[2];
        string color = component[num].Split(new string[] { "; " }, System.StringSplitOptions.None)[3];
        float colid = float.Parse(component[num].Split(new string[] { "; " }, System.StringSplitOptions.None)[4]);
        Vector3 p = GetObjectPos(num);
        Vector3 pos = new Vector3(p.x, p.y, 0);
        Quaternion rot = new Quaternion();
        rot.eulerAngles = new Vector3(0, 0, int.Parse(rotZ));

        if (id >= 1)
        {
            GameObject go = Instantiate(Prefabs[(int)id-1], pos, rot, SummonPlace);
            go.name = "Objet n° " + num;
            go.transform.localScale = new Vector2(Screen.height / BlocSize, Screen.height / BlocSize);
            SpriteRenderer SR = go.GetComponent<SpriteRenderer>();
            SR.color = HexToColor(color);
            SR.sortingOrder = (int)p.z;
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(File.ReadAllBytes(Application.persistentDataPath + "/Textures/0/" + id.ToString(".0####") + ".png"));
            SR.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
            go.GetComponent<Mur>().colider = colid;
        }
        else
        {
            if (id == 0.1F)
                GameObject.Find("Player").transform.position = pos;
            else if (id == 0.2F)
            {
                GameObject go = Instantiate(TriggerPref[0], pos, rot, SummonPlace);
                go.name = "Trigger n° " + num;
                go.transform.localScale = new Vector2(Screen.height / BlocSize, Screen.height / BlocSize);
            }
        }
    }
    public Vector3 GetObjectPos(int num)
    {
        string a = component[num].Split(new string[] { "; " }, System.StringSplitOptions.None)[1];
        string[] b = a.Replace("(", "").Replace(" ", "").Replace(")", "").Replace(".0", "").Split(new string[] { "," }, System.StringSplitOptions.None);
        float[] c = new float[] { float.Parse(b[0]) * 50 + 25, float.Parse(b[1]) * 50 + 25, float.Parse(b[2]) };
        return new Vector3(c[0], c[1], c[2]);
    }
    public static UnityEngine.Color HexToColor(string hex)
    {
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        byte a = byte.Parse(hex.Substring(6), System.Globalization.NumberStyles.Number);
        return new Color32(r, g, b, a);
    }

    public void Exit()
    {
        string scene = FromScene;
        if (FromScene == "")
            scene = "Home";
        else if (FromScene == "Online")
            scene = "Editor";

        if (scene == "Editor" & FromScene == "Editor")
        {
            File.WriteAllLines(Application.temporaryCachePath + "/play.txt", new string[2] { file, "" });
            GameObject.Find("Audio").GetComponent<menuMusic>().Stop();
        }
        else GameObject.Find("Audio").GetComponent<menuMusic>().StartDefault();
        GetComponent<BaseControl>().LSC.LoadScreen(scene);
    }

    public void Replay()
    {
        GameObject.Find("Player").GetComponent<Player>().PeutAvancer = true;
        Base.GetChild(3).gameObject.SetActive(false);
        File.WriteAllLines(Application.temporaryCachePath + "/play.txt", new string[2] { file, FromScene });
        NewStart();
    }
}
