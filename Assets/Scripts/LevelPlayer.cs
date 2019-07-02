using AngryDash.Image.Reader;
using AngryDash.Language;
using Level;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tools;
using UnityEngine;
using UnityEngine.UI;
using Player = AngryDash.Game.Player;

public class LevelPlayer : MonoBehaviour
{
    public Infos level;
    string file;
    public string FromScene;
    public string[] passThroughArgs;
    Camera cam;

    public GameObject PlayerPrefab;
    public GameObject[] Prefabs;
    [HideInInspector] public Transform SummonPlace;

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
        if (nbLancer > level.victoryConditions.maxThrow & level.victoryConditions.maxThrow > 0) Lost();
        else nbLancerTxt.text = LangueAPI.Get("native", "levelPlayer.throw", "[0] Throw", nbLancer);
    }

    private void Start()
    {
        cam = GetComponent<Camera>();
        cam.transform.position = new Vector3(Screen.width / 2, Screen.height / 2, -10);
        cam.GetComponent<Camera>().orthographicSize = Screen.height / 2;

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Online")
        {
            string[] args = LoadingScreenControl.GetLSC().GetArgs();
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

            Infos lvl = Infos.Parse(LevelUpdater.UpdateLevel(File.ReadAllText(file)));
            if (string.IsNullOrEmpty(lvl.name)) lvl.name = Path.GetFileNameWithoutExtension(file);
            PlayLevel(lvl);
        }
        else FromFile(Application.persistentDataPath + "/Levels/Official Levels/4.level", "Home");
    }

    public void PlayLevel(LevelItem item)
    {
        Infos lvl = Infos.Parse(LevelUpdater.UpdateLevel(item.Data));
        if (string.IsNullOrEmpty(lvl.name)) lvl.name = item.Name;
        PlayLevel(lvl);
    }
    public void PlayLevel(Infos item)
    {
        level = item;
        Parse(); //Spawn blocks
        Discord.Presence(LangueAPI.Get("native", "discordPlaying_title", "Play a level"), "", new DiscordClasses.Img("default", LangueAPI.Get("native", "discordPlaying_caption", "Level : [0]", level.name)), null, -1, 0); //Sets the Discord Infos
    }

    void Parse()
    {
        for (int i = 0; i < ArrierePlan.childCount; i++)
        {
            ArrierePlan.GetChild(i).GetComponent<Image>().color = level.background.color;
            ArrierePlan.GetChild(i).GetComponent<UImage_Reader>().baseID = level.background.category + "/BACKGROUNDS/" + level.background.id;
        }
        Vector2 size = ArrierePlan.GetChild(0).GetComponent<Image>().sprite.Size();
        ArrierePlan.GetComponent<CanvasScaler>().referenceResolution = size;
        float match = 1;
        if (size.y < size.x) match = 0;
        ArrierePlan.GetComponent<CanvasScaler>().matchWidthOrHeight = match;

        while (GetComponent<MainCam>().Player == null) { }
        if (level.player == null) level.player = new Level.Player();
        if (level.victoryConditions == null) level.victoryConditions = new VictoryConditions();
        GetComponent<MainCam>().Player.GetComponent<Player>().levelSettings = level.player.DeepClone();

        Transform place = new GameObject("Items").transform;
        for (int i = 0; i < level.blocks.Length; i++) Instance(i, place);
        transform.GetChild(0).gameObject.SetActive(true);

        Base.GetChild(4).gameObject.SetActive(false);


        string music = "";
        if (level.music != null) music = Application.persistentDataPath + "/Musics/" + level.music.Artist + " - " + level.music.Name;

        if (GameObject.Find("Audio") != null)
        {
            GameObject.Find("Audio").GetComponent<menuMusic>().Stop();
            if (!string.IsNullOrEmpty(music)) GameObject.Find("Audio").GetComponent<menuMusic>().LoadUnpackagedMusic(music);
        }

        if (SummonPlace != null) Destroy(SummonPlace.gameObject);
        SummonPlace = place;


        Time.timeScale = 1;
        GetComponent<MainCam>().Player.GetComponent<Player>().PeutAvancer = true; //Le niveau est chargé, le joueur peut bouger
    }

    public void Instance(int num, Transform place)
    {
        float id = level.blocks[num].id;
        Vector3 p = level.blocks[num].position * 50 + new Vector3(25, 25, 0);
        Vector3 pos = new Vector3(p.x, p.y, 0);
        Quaternion rot = new Quaternion();

        if (id >= 1)
        {
            string color = GetBlocStatus("Color", num);
            float colid = 0;
            float.TryParse(GetBlocStatus("Behavior", num), out colid);
            int rotZ = 0;
            int.TryParse(GetBlocStatus("Rotate", num), out rotZ);
            rot.eulerAngles = new Vector3(0, 0, rotZ);

            GameObject go = null;
            try
            {
                go = Instantiate(Prefabs[(int)id - 1], pos, rot, place);
            }
            catch { Debug.LogWarning("The block at the line " + num + " as an invalid id"); return; }
            go.name = "Objet n° " + num;
            SpriteRenderer SR = go.GetComponent<SpriteRenderer>();
            SR.color = HexToColor(color);
            SR.sortingOrder = (int)level.blocks[num].position.z;
            go.GetComponent<UImage_Reader>().SetID("native/BLOCKS/" + id.ToString(".0####")).Load();
            go.transform.localScale = new Vector2(100F / SR.sprite.texture.width * 50, 100F / SR.sprite.texture.height * 50);

            go.GetComponent<AngryDash.Game.Mur>().colider = colid;
            go.GetComponent<AngryDash.Game.Mur>().blockID = id;
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
                GameObject go = Instantiate(TriggerPref[0], pos, rot, place);
                go.name = "Objet n° " + num;
                UImage_Reader reader = go.GetComponent<UImage_Reader>().Load();
                go.transform.localScale = new Vector2(100, 100) / reader.FrameSize * 50;
            }
            else if (id == 0.3F) //Checkpoint
            {
                GameObject go = Instantiate(TriggerPref[1], pos, rot, place);
                go.name = "Objet n° " + num;
                UImage_Reader reader = go.GetComponent<UImage_Reader>().Load();
                go.transform.localScale = new Vector2(100, 100) / reader.FrameSize * 50;
                go.SetActive(GetComponent<MainCam>().Player.GetComponent<Player>().levelSettings.respawnMode == 1);
            }
            else if (id == 0.4F) //Move
            {
                GameObject go = Instantiate(TriggerPref[2], pos, rot, place);
                go.name = "Objet n° " + num;
                go.transform.localScale = new Vector2(50, 50);

                MoveTrigger moveTrigger = go.GetComponent<MoveTrigger>();
                int.TryParse(GetBlocStatus("AffectationType", num), out moveTrigger.AffectationType);
                string[] Blocks = GetBlocStatus("Blocks", num).Split(new string[] { "," }, System.StringSplitOptions.None);
                if (string.IsNullOrEmpty(Blocks[0]) | Blocks[0] == "Null") Blocks = new string[0];
                moveTrigger.Blocks = Blocks;
                try { moveTrigger.Translation = Editor_MoveTrigger.getVector2(GetBlocStatus("Translation", num)); } catch { }
                string translationFrom = GetBlocStatus("TranslationFrom", num);
                try
                {
                    string[] translationFromArray = translationFrom.Substring(1, translationFrom.Length - 2).Split(',');
                    for (int i = 0; i < translationFromArray.Length; i++)
                        bool.TryParse(translationFromArray[i], out moveTrigger.TranslationFromPlayer[i]);
                }
                catch { }
                string reset = GetBlocStatus("Reset", num);
                try
                {
                    string[] resetArray = reset.Substring(1, reset.Length - 2).Split(',');
                    for (int i = 0; i < resetArray.Length; i++) bool.TryParse(resetArray[i], out moveTrigger.Reset[i]);
                }
                catch { }
                bool.TryParse(GetBlocStatus("GlobalRotation", num), out moveTrigger.GlobalRotation);
                int.TryParse(GetBlocStatus("Type", num), out moveTrigger.Type);
                float.TryParse(GetBlocStatus("Speed", num), out moveTrigger.Speed);
                bool.TryParse(GetBlocStatus("MultiUsage", num), out moveTrigger.MultiUsage);
                try { moveTrigger.Rotation = Editor_MoveTrigger.getVector3(GetBlocStatus("Rotation", num)); } catch { }
            }
        }
    }
    public Vector3 GetObjectPos(int num)
    {
        Vector2 pos = level.blocks[num].position * 50 + new Vector3(25, 25, 0);
        return new Vector3(pos.x, pos.y, level.blocks[num].position.z);
    }
    public static Color HexToColor(string hex)
    {
        if (hex == null) return new Color32(190, 190, 190, 255);
        else if (hex.Length < 6 | hex.Length > 9) return new Color32(190, 190, 190, 255);

        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        byte a = byte.Parse(hex.Substring(6), System.Globalization.NumberStyles.Number);
        return new Color32(r, g, b, a);
    }
    public string GetBlocStatus(string StatusID, int Bloc)
    {
        if (level.blocks.Length > Bloc)
        {
            Block block = level.blocks[Bloc];

            if (StatusID == "ID") return block.id.ToString();
            else if (StatusID == "Position") return block.position.ToString();
            else if (StatusID == "PositionX") return block.position.x.ToString();
            else if (StatusID == "PositionY") return block.position.y.ToString();
            else if (StatusID == "Layer") return block.position.z.ToString();
            else if (block.parameter.ContainsKey(StatusID)) return block.parameter[StatusID];
            else return "";
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
            LoadingScreenControl.GetLSC().LoadScreen(scene, args);
        }
        else
        {
            GameObject.Find("Network Manager").GetComponent<_NetworkManager>().Disconnect();
        }
    }

    public void Replay()
    {
        Time.timeScale = 1;
        GameObject player = GameObject.Find("Player");
        if (player == null) player = GameObject.Find("Player(Clone)");
        if (player != null) Destroy(player);
        GetComponent<MainCam>().Player = Instantiate(PlayerPrefab);
        nbLancer = 0;

        Transform trace = GameObject.Find("Traces").transform;
        for (int i = 0; i < trace.childCount; i++)
            Destroy(trace.GetChild(i).gameObject);

        PlayLevel(level);
    }

    void Lost()
    {
        Pause(true);
        Base.GetChild(6).gameObject.SetActive(true);
    }
}
