using Level;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tools;
using UnityEngine;
using UnityEngine.UI;

public class LevelPlayer : MonoBehaviour
{
    public LevelItem level;
    string file;
    public string FromScene;
    public string[] passThroughArgs;
    public string[] component;
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
        nbLancerTxt.text = LangueAPI.StringWithArgument("native", "playerTurn", new string[1] { nbLancer.ToString() }, "[0] Turn");
    }

    private void Start()
    {
        cam = GetComponent<Camera>();
        cam.transform.position = new Vector3(Screen.width / 2, Screen.height / 2, -10);
        cam.GetComponent<Camera>().orthographicSize = Screen.height / 2;

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Online")
        {
            string[] args = GetComponent<BaseControl>().LSC.GetArgs();
            if (args != null)
            {
                if (args.Length > 2)
                {
                    passThroughArgs = args.RemoveAt(0, 2);
                    if (args[1] == "File") FromFile(args[2], args[0]);
                    else if (args[1] == "Data")
                    {
                        FromScene = args[0]; //Define the scene that will be open at the level player's exit
                        PlayLevel(LevelItem.Parse(args[2]));
                    }
                    else FromFile();
                }
                else FromFile();
            }
            else FromFile();
        }
    }
    void FromFile(string _file = null, string _fromScene = null)
    {
        if (!string.IsNullOrEmpty(_file))
        {
            file = _file;
            if (string.IsNullOrEmpty(_fromScene)) FromScene = "Home";
            else FromScene = _fromScene;

            LevelItem item = new LevelItem(Path.GetFileNameWithoutExtension(file));
            item.Data = File.ReadAllLines(file);
            PlayLevel(item);
        }
        else FromFile(Application.persistentDataPath + "/Levels/Official Levels/4.level", "Home");
    }

    public void PlayLevel(LevelItem item)
    {
        level = item;
        component = Editeur.UpdateLevel(item.Data);
        Base.GetChild(3).GetChild(0).GetComponent<Text>().text = item.Name; //Sets the level name
        Parse(); //Spawn blocks
        Discord.Presence(LangueAPI.String("native", "discordPlaying_title"), "", new DiscordClasses.Img("default", LangueAPI.StringWithArgument("native", "discordPlaying_caption", item.Name)), null, -1, 0); //Sets the Discord Infos
    }

    void Parse()
    {
        int a = -1;
        for (int x = 0; x < component.Length; x++)
        {
            if (component[x].Contains("background = ") & a == -1)
            {
                a = x;
                x = component.Length;
            }
        }
        string back = "1; 4B4B4B255";
        if (a != -1)
            back = component[a].Replace("background = ", "");
        string[] Ar = back.Split(new string[1] { "; " }, System.StringSplitOptions.None);
        for (int i = 0; i < ArrierePlan.childCount; i++)
        {
            ArrierePlan.GetChild(i).GetComponent<Image>().color = HexToColor(Ar[1]);
            ArrierePlan.GetChild(i).GetComponent<UImage_Reader>().baseID = "native/BACKGROUNDS/" + Ar[0];
        }

        int d = -1;
        for (int x = 0; x < component.Length; x++)
        {
            if (component[x].Contains("Blocks {") & d == -1)
            {
                d = x + 1;
                x = component.Length;
            }
        }
        int end = -1;
        if (d != -1)
        {
            for (int i = component.Length - 1; i >= d; i--)
            {
                if (component[i].Replace("\r", "") == "}" & end == -1)
                {
                    end = i;
                    i = component.Length;
                }
            }
        }

        int respawnModeLine = -1;
        for (int x = 0; x < component.Length; x++)
        {
            if (component[x].Contains("respawnMode = ") & respawnModeLine == -1)
            {
                respawnModeLine = x;
                x = component.Length;
            }
        }
        int respawnMode = 0;
        if (respawnModeLine != -1)
            respawnMode = int.Parse(component[respawnModeLine].Replace("respawnMode = ", ""));
        while (GetComponent<MainCam>().Player == null) { }
        GetComponent<MainCam>().Player.GetComponent<Player>().respawnMode = respawnMode;

        Transform place = new GameObject("Items").transform;
        for (int i = d; i < end; i++)
            Instance(i, place);
        transform.GetChild(0).gameObject.SetActive(true);

        Base.GetChild(3).gameObject.SetActive(false);

        int m = -1;
        for (int x = 0; x < component.Length; x++)
        {
            if (component[x].Contains("music = ") & m == -1)
            {
                m = x;
                x = component.Length;
            }
        }
        string music = "";
        if (m != -1)
            music = Application.persistentDataPath + "/Musics/" + component[m].Replace("music = ", "");

        if (GameObject.Find("Audio") != null & music != "null")
            GameObject.Find("Audio").GetComponent<menuMusic>().LoadMusic(music);

        Destroy(SummonPlace.gameObject);
        SummonPlace = place;


        Time.timeScale = 1;
        GetComponent<MainCam>().Player.GetComponent<Player>().PeutAvancer = true; //Le niveau est chargé, le joueur peut bouger
    }

    public void Instance(int num, Transform place)
    {
        float id = 0;
        try { id = float.Parse(GetBlocStatus("ID", num)); }
        catch { Debug.LogWarning("The block at the line " + num + " as an invalid id"); return; }
        Vector3 p = new Vector3();
        try { p = GetObjectPos(num); }
        catch { Debug.LogWarning("The block at the line " + num + " as an invalid position"); return; }
        Vector3 pos = new Vector3(p.x, p.y, 0);
        Quaternion rot = new Quaternion();

        if (id >= 1)
        {
            string color = GetBlocStatus("Color", num);
            float colid = 0;
            try { colid = float.Parse(GetBlocStatus("Behavior", num)); }
            catch { Debug.LogWarning("The block at the line " + num + " as an invalid behavior id :\n" + GetBlocStatus("Behavior", num)); return; }
            string rotZ = GetBlocStatus("Rotate", num);
            try { rot.eulerAngles = new Vector3(0, 0, int.Parse(rotZ)); }
            catch { Debug.LogWarning("The block at the line " + num + " as an invalid rotation"); return; }

            GameObject go = null;
            try
            {
                go = Instantiate(Prefabs[(int)id - 1], pos, rot, place);
            }
            catch { Debug.LogWarning("The block at the line " + num + " as an invalid id"); return; }
            go.name = "Objet n° " + num;
            SpriteRenderer SR = go.GetComponent<SpriteRenderer>();
            try { SR.color = HexToColor(color); }
            catch
            {
                SR.color = new Color32(190, 190, 190, 255);
                Debug.LogWarning("The block at the line " + num + " as an invalid color");
            }
            SR.sortingOrder = (int)p.z;
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(File.ReadAllBytes(Sprite_API.Sprite_API.spritesPath("native/BLOCKS/" + id.ToString(".0####") + ".png")));
            SR.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));

            go.transform.localScale = new Vector2(100F / tex.width * 50, 100F / tex.height * 50);

            go.GetComponent<Mur>().colider = colid;
            go.GetComponent<Mur>().blockID = id;
        }
        else //Trigger
        {
            if (id == 0.1F) //Start
            {
                GetComponent<MainCam>().Player.transform.position = pos;
                GetComponent<MainCam>().Player.GetComponent<Player>().PositionInitiale = pos;
            }
            else if (id == 0.2F) //Stop
            {
                GameObject go = null;
                try { go = Instantiate(TriggerPref[0], pos, rot, place); }
                catch { Debug.LogWarning("The block at the line " + num + " as an invalid id"); return; }
                go.name = "Objet n° " + num;
                Texture2D tex = new Texture2D(1, 1);
                if (go.GetComponent<SpriteRenderer>() != null) tex = go.GetComponent<SpriteRenderer>().sprite.texture;
                go.transform.localScale = new Vector2(100F / tex.width * 50, 100F / tex.height * 50);
            }
            else if (id == 0.3F) //Checkpoint
            {
                GameObject go = null;
                try { go = Instantiate(TriggerPref[1], pos, rot, place); }
                catch { Debug.LogWarning("The block at the line " + num + " as an invalid id"); return; }
                go.name = "Objet n° " + num;
                Texture2D tex = new Texture2D(1, 1);
                if (go.GetComponent<SpriteRenderer>() != null) tex = go.GetComponent<SpriteRenderer>().sprite.texture;
                go.transform.localScale = new Vector2(100F / tex.width * 50, 100F / tex.height * 50);
                go.SetActive(GetComponent<MainCam>().Player.GetComponent<Player>().respawnMode == 1);
            }
            else if (id == 0.4F) //Move
            {
                GameObject go = null;
                try { go = Instantiate(TriggerPref[2], pos, rot, place); }
                catch { Debug.LogWarning("The block at the line " + num + " as an invalid id"); return; }
                go.name = "Objet n° " + num;
                Texture2D tex = new Texture2D(100, 100);
                if (go.GetComponent<SpriteRenderer>() != null) tex = go.GetComponent<SpriteRenderer>().sprite.texture;
                go.transform.localScale = new Vector2(100F / tex.width * 50, 100F / tex.height * 50);

                MoveTrigger moveTrigger = go.GetComponent<MoveTrigger>();
                try { moveTrigger.AffectationType = int.Parse(GetBlocStatus("AffectationType", num)); } catch { }
                string[] Blocks = GetBlocStatus("Blocks", num).Split(new string[] { "," }, System.StringSplitOptions.None);
                if (string.IsNullOrEmpty(Blocks[0]) | Blocks[0] == "Null") Blocks = new string[0];
                moveTrigger.Blocks = Blocks;
                try { moveTrigger.Translation = Editor_MoveTrigger.getVector2(GetBlocStatus("Translation", num)); } catch { }
                string translationFrom = GetBlocStatus("TranslationFrom", num);
                try
                {
                    string[] translationFromArray = translationFrom.Substring(1, translationFrom.Length - 2).Split(',');
                    for (int i = 0; i < translationFromArray.Length; i++)
                        moveTrigger.TranslationFromPlayer[i] = bool.Parse(translationFromArray[i]);
                }
                catch { }
                string reset = GetBlocStatus("Reset", num);
                try
                {
                    string[] resetArray = reset.Substring(1, reset.Length - 2).Split(',');
                    for (int i = 0; i < resetArray.Length; i++)
                        moveTrigger.Reset[i] = bool.Parse(resetArray[i]);
                }
                catch { }
                try { moveTrigger.GlobalRotation = bool.Parse(GetBlocStatus("GlobalRotation", num)); } catch { }
                try { moveTrigger.Type = int.Parse(GetBlocStatus("Type", num)); } catch { }
                try { moveTrigger.Speed = float.Parse(GetBlocStatus("Speed", num)); } catch { }
                try { moveTrigger.MultiUsage = bool.Parse(GetBlocStatus("MultiUsage", num)); } catch { }
                try { moveTrigger.Rotation = Editor_MoveTrigger.getVector3(GetBlocStatus("Rotation", num)); } catch { }
            }
        }
    }
    public Vector3 GetObjectPos(int num)
    {
        string a = GetBlocStatus("Position", num);
        string[] b = a.Replace("(", "").Replace(" ", "").Replace(")", "").Replace(".0", "").Split(new string[] { "," }, System.StringSplitOptions.None);
        float[] c = new float[] { float.Parse(b[0]) * 50 + 25, float.Parse(b[1]) * 50 + 25, float.Parse(b[2]) };
        return new Vector3(c[0], c[1], c[2]);
    }
    public static Color HexToColor(string hex)
    {
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        byte a = byte.Parse(hex.Substring(6), System.Globalization.NumberStyles.Number);
        return new Color32(r, g, b, a);
    }
    public string GetBlocStatus(string StatusID, int Bloc)
    {
        if (component.Length > 0 & component.Length > Bloc)
        {
            try
            {
                string[] a = component[Bloc].Split(new string[] { "; " }, System.StringSplitOptions.None);

                if (StatusID == "ID")
                    return a[0];
                else if (StatusID == "Position")
                    return a[1];
                else if (StatusID == "PositionX")
                    return a[1].Split(new string[] { ", " }, System.StringSplitOptions.None)[0].Replace(")", "").Replace("(", "");
                else if (StatusID == "PositionY")
                    return a[1].Split(new string[] { ", " }, System.StringSplitOptions.None)[1].Replace(")", "").Replace("(", "");
                else if (StatusID == "Layer")
                    return a[1].Split(new string[] { ", " }, System.StringSplitOptions.None)[2].Replace(")", "").Replace("(", "");
                else
                {
                    string[] b = component[Bloc].Split(new string[] { "; {" }, System.StringSplitOptions.None)[1].Split(new string[] { "}" }, System.StringSplitOptions.None)[0].Split(new string[] { "; " }, System.StringSplitOptions.None);
                    for (int i = 0; i < b.Length; i++)
                    {
                        string[] param = b[i].Split(new string[] { ":" }, System.StringSplitOptions.None);
                        if (param[0] == StatusID)
                            return param[1];
                    }

                    return "";
                }
            }
            catch { return ""; }
        }
        else return "";
    }

    float oldSpeed = 1;
    public void Pause(bool pause)
    {
        if (pause) Time.timeScale = 0;
        else Time.timeScale = 1;

        Player player = GetComponent<MainCam>().Player.GetComponent<Player>();
        player.enabled = !pause;
        if (pause)
        {
            oldSpeed = player.vitesse;
            player.vitesse = 0;
        }
        else player.vitesse = oldSpeed;
    }

    public void Exit()
    {
        Time.timeScale = 1;
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Online")
        {
            string scene = FromScene;
            string[] args = null;
            if (FromScene == "")
                scene = "Home";
            else if (FromScene.Contains("/"))
            {
                string[] FromSceneDetails = FromScene.Split(new string[] { "/" }, System.StringSplitOptions.None);
                scene = FromSceneDetails[0];
                args = FromSceneDetails.RemoveAt(0);
                GameObject.Find("Audio").GetComponent<menuMusic>().StartDefault();
            }
            else if (FromScene == "Editor")
            {
                scene = FromScene;
                args = passThroughArgs;
                GameObject.Find("Audio").GetComponent<menuMusic>().Stop();
            }
            GetComponent<BaseControl>().LSC.LoadScreen(scene, args);
        }
        else
        {
            GameObject.Find("Network Manager").GetComponent<_NetworkManager>().Disconnect();
        }
    }

    public void Replay()
    {
        Pause(false);
        if (GameObject.Find("Player") != null) GameObject.Find("Player").GetComponent<Player>().PeutAvancer = true;
        else if (GameObject.Find("Player(Clone)") != null) GameObject.Find("Player(Clone)").GetComponent<Player>().PeutAvancer = true;
        nbLancer = 0;
        Base.GetChild(3).gameObject.SetActive(false);
        PlayLevel(level);
    }
}
