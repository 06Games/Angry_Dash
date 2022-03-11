using System;
using System.Collections;
using System.Globalization;
using System.IO;
using AngryDash.Game.Events;
using AngryDash.Image;
using AngryDash.Image.Reader;
using AngryDash.Language;
using Discord;
using FileFormat;
using Level;
using Tools;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Event = AngryDash.Game.Events.Event;

namespace AngryDash.Game
{
    public class LevelPlayer : MonoBehaviour
    {
        public Infos level;
        private string file;
        public string FromScene;
        public string[] passThroughArgs;
        private Camera cam;

        public GameObject PlayerPrefab;
        public GameObject[] Prefabs;
        [HideInInspector] public Transform SummonPlace;

        public Transform ArrierePlan;

        private Vector3 EndPoint;
        public GameObject EventPrefab;
        public GameObject[] TriggerPref;
        public Transform Base;

        public int nbLancer;
        public Text nbLancerTxt;

        private void Update()
        {
            if (level.victoryConditions == null) nbLancerTxt.text = "";
            else if (nbLancer > level.victoryConditions.maxThrow & level.victoryConditions.maxThrow > 0) Lost(Base);
            else nbLancerTxt.text = LangueAPI.Get("native", nbLancer <= 1 ? "levelPlayer.throw" : "levelPlayer.throws", nbLancer <= 1 ? "[0] throw" : "[0] throws", nbLancer);
        }

        private void Start()
        {
            cam = GetComponent<Camera>();
            cam.transform.position = new Vector3(Screen.width / 2, Screen.height / 2, -10);
            cam.GetComponent<Camera>().orthographicSize = Screen.height / 2;

            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Online")
            {
                var args = SceneManager.args;
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
        }

        private void FromFile(string _file = null, string _fromScene = null)
        {
            if (!string.IsNullOrEmpty(_file))
            {
                file = _file;
                if (string.IsNullOrEmpty(_fromScene)) FromScene = "Home";
                else FromScene = _fromScene;

                var lvl = Infos.Parse(LevelUpdater.UpdateLevel(File.ReadAllText(file)));
                if (string.IsNullOrEmpty(lvl.name)) lvl.name = Path.GetFileNameWithoutExtension(file);
                PlayLevel(lvl);
            }
#if UNITY_EDITOR
            else FromFile(Application.persistentDataPath + "/Levels/Official Levels/4.level", "Home/Play/Official Levels");
#endif
        }

        public void PlayLevel(LevelItem item)
        {
            var lvl = Infos.Parse(LevelUpdater.UpdateLevel(item.Data));
            if (string.IsNullOrEmpty(lvl.name)) lvl.name = item.Name;
            PlayLevel(lvl);
        }
        public void PlayLevel(Infos item)
        {
            level = item;
            StartCoroutine(Parse()); //Spawn blocks
            _06Games.Account.Discord.NewActivity(new Activity
            {
                State = LangueAPI.Get("native", "discordPlaying_title", "Play a level"),
                Assets = new ActivityAssets { LargeImage = "default", LargeText = LangueAPI.Get("native", "discordPlaying_caption", "Level : [0]", level.name) },
                Timestamps = new ActivityTimestamps { Start = _06Games.Account.Discord.CalculateStartTime(DateTime.Now) }
            });
        }

        private IEnumerator Parse()
        {
            if (string.IsNullOrWhiteSpace(level.rpURL)) StartCoroutine(ParseCoroutine());
            else
            {
                var go = Base.Find("RP");

                var title = new[] { "levelPlayer.resourcePack.recommend", "This level recommends a resource pack, do you want to install it?" };
                if (level.rpRequired) title = new[] { "levelPlayer.resourcePack.required", "This level require a resource pack, do you want to install it?" };
                go.GetChild(0).GetComponent<Text>().text = LangueAPI.Get("native", title[0], title[1]);

                if (Uri.TryCreate(level.rpURL, UriKind.Absolute, out var uri))
                {
                    if (uri.Host == "localhost" | uri.Host == "06games.ddns.net") go.GetChild(1).gameObject.SetActive(false);
                    else go.GetChild(1).GetComponent<Text>().text = LangueAPI.Get("native", "levelPlayer.resourcePack.warn", "Warning, this resource pack will be downloaded from an external server (<i>[0]</i>), not belonging to 06Games.\nDownload it only if you had full confidence in the host.", uri.Host);


                    var request = UnityWebRequest.Head(uri);
                    request.timeout = 15;
                    yield return request.SendWebRequest();
                    var error = !string.IsNullOrWhiteSpace(request.error);
                    long size = 0;

                    if (!error)
                    {
                        long.TryParse(request.GetResponseHeader("Content-Length"), out size);
                        go.GetChild(2).GetChild(0).GetComponent<Text>().text = LangueAPI.Get("native", "levelPlayer.resourcePack.install", "Install ([0] KB)", Mathf.RoundToInt(size / 1000F));

                        var no = new[] { "levelPlayer.resourcePack.no", "No Thanks" };
                        if (level.rpRequired) no = new[] { "levelPlayer.resourcePack.quit", "Quit" };
                        go.GetChild(3).GetChild(0).GetComponent<Text>().text = LangueAPI.Get("native", no[0], no[1]);
                    }

                    var file = new FileInfo(Application.persistentDataPath + "/Levels/Resource Packs/" + level.rpURL.TrimStart("http://").TrimStart("https://").TrimStart("file://").Replace("\\", "/").Replace("/", "_"));
                    if (file.Exists && (file.Length == size | error))
                    {
                        var dirPath = Application.temporaryCachePath + "/downloadedRP/";
                        if (!Directory.Exists(dirPath)) ZIP.Decompress(file.FullName, dirPath);
                        Sprite_API.forceRP = dirPath;
                        StartCoroutine(ParseCoroutine());
                    }
                    else go.gameObject.SetActive(true);
                }
                else StartCoroutine(ParseCoroutine());
            }
        }
        public void DownloadRP() { StartCoroutine(DownloadRPCoroutine()); }

        private IEnumerator DownloadRPCoroutine()
        {
            var go = Base.Find("RP");

            var request = UnityWebRequest.Get(level.rpURL);
            var async = request.SendWebRequest();
            async.completed += a =>
            {
                var zipPath = Application.persistentDataPath + "/Levels/Resource Packs/" + level.rpURL.TrimStart("http://").TrimStart("https://").TrimStart("file://").Replace("\\", "/").Replace("/", "_");
                var dirPath = Application.temporaryCachePath + "/downloadedRP/";

                File.WriteAllBytes(zipPath, request.downloadHandler.data);
                ZIP.Decompress(zipPath, dirPath);
                Sprite_API.forceRP = dirPath;

                StartCoroutine(ParseCoroutine());
                go.gameObject.SetActive(false);
            };

            var slider = go.GetChild(2).GetChild(1).GetComponent<Slider>();
            while (!async.isDone)
            {
                yield return new WaitForEndOfFrame();
                slider.value = async.progress;
            }
        }
        public void NoRP()
        {
            if (level.rpRequired) Exit();
            else StartCoroutine(ParseCoroutine());
        }

        private IEnumerator ParseCoroutine()
        {
            for (var i = 0; i < ArrierePlan.childCount; i++)
            {
                ArrierePlan.GetChild(i).GetComponent<UnityEngine.UI.Image>().color = level.background.color;
                ArrierePlan.GetChild(i).GetComponent<UImage_Reader>().SetID(level.background.category + "/BACKGROUNDS/" + level.background.id).Load();
            }
            var size = ArrierePlan.GetChild(0).GetComponent<UnityEngine.UI.Image>().sprite.Size();
            ArrierePlan.GetComponent<CanvasScaler>().referenceResolution = size;
            float match = 1;
            if (size.y < size.x) match = 0;
            ArrierePlan.GetComponent<CanvasScaler>().matchWidthOrHeight = match;

            yield return new WaitWhile(() => Player.userPlayer == null);
            if (level.player == null) level.player = new Level.Player();
            if (level.victoryConditions == null) level.victoryConditions = new VictoryConditions();
            Player.userPlayer.levelSettings = level.player.DeepClone();

            var place = new GameObject("Items").transform;
            for (var i = 0; i < level.blocks.Length; i++) Instance(i, place);
            transform.GetChild(0).gameObject.SetActive(true);

            Base.GetChild(4).gameObject.SetActive(false);


            var music = "";
            if (level.music != null) music = Application.persistentDataPath + "/Musics/" + level.music.Artist + " - " + level.music.Name;

            if (GameObject.Find("Audio") != null)
            {
                GameObject.Find("Audio").GetComponent<MenuMusic>().Stop();
                if (!string.IsNullOrEmpty(music)) GameObject.Find("Audio").GetComponent<MenuMusic>().LoadUnpackagedMusic(music);
            }

            if (SummonPlace != null) Destroy(SummonPlace.gameObject);
            SummonPlace = place;


            Time.timeScale = 1;
            Player.userPlayer.PeutAvancer = true; //Le niveau est chargé, le joueur peut bouger
        }

        public void Instance(int num, Transform place)
        {
            var id = level.blocks[num].id;
            var p = level.blocks[num].position * 50 + new Vector3(25, 25, 0);
            var pos = new Vector3(p.x, p.y, 0);
            var rot = new Quaternion();

            if (id >= 1)
            {
                var color = GetBlocStatus("Color", num);
                float.TryParse(GetBlocStatus("Behavior", num), out var colid);
                int.TryParse(GetBlocStatus("Rotate", num), out var rotZ);
                rot.eulerAngles = new Vector3(0, 0, rotZ);

                GameObject go;
                try { go = Instantiate(Prefabs[(int)id - 1], pos, rot, place); }
                catch { Debug.LogWarning("The block at the line " + num + " as an invalid id"); return; }
                go.name = "Objet n° " + num;
                var SR = go.GetComponent<SpriteRenderer>();
                SR.color = HexToColor(color);
                SR.sortingOrder = (int)level.blocks[num].position.z;
                var size = go.GetComponent<UImage_Reader>().SetID("native/BLOCKS/" + id.ToString(".0####")).Load().FrameSize;
                if (size != Vector2.zero) go.transform.localScale = new Vector2(100, 100) / size * 50;
                else go.transform.localScale = new Vector2(50, 50);

                if ((int)id == 1)
                {
                    var colliders = go.GetComponents<BoxCollider2D>();
                    if (colliders.Length > 0) colliders[0].size = size / new Vector2(100, 100);
                    if (colliders.Length > 1) colliders[1].size = size / new Vector2(100, 100) + new Vector2(0.1F, 0.1F);
                }
                else
                {
                    var colliders = go.GetComponents<PolygonCollider2D>();
                    foreach (var collider in colliders)
                    {
                        var points = collider.points;
                        for (var i = 0; i < points.Length; i++) points[i] *= size / new Vector2(100, 100);
                        collider.points = points;
                    }
                }

                go.GetComponent<Mur>().colider = colid;
                go.GetComponent<Mur>().blockID = id;
            }
            else if (id == 0) //Event
            {
                var go = Instantiate(EventPrefab, pos, rot, place);
                go.name = "Objet n° " + num;
                go.transform.localScale = new Vector2(50, 50);
                go.GetComponent<Event>().script = GetBlocStatus("Script", num);
            }
            else //Compatibility Mode
            {
                if (id == 0.1F) //Start
                {
                    Player.userPlayer.transform.position = pos;
                    Player.userPlayer.PositionInitiale = pos;
                }
                else if (id == 0.2F) //Stop
                {
                    var go = Instantiate(TriggerPref[0], pos, rot, place);
                    go.name = "Objet n° " + num;
                    var size = go.GetComponent<UImage_Reader>().Load().FrameSize;
                    if (size != Vector2.zero) go.transform.localScale = new Vector2(100, 100) / size * 50;
                    else go.transform.localScale = new Vector2(50, 50);
                }
                else if (id == 0.3F) //Checkpoint
                {
                    var go = Instantiate(TriggerPref[1], pos, rot, place);
                    go.name = "Objet n° " + num;
                    var size = go.GetComponent<UImage_Reader>().Load().FrameSize;
                    if (size != Vector2.zero) go.transform.localScale = new Vector2(100, 100) / size * 50;
                    else go.transform.localScale = new Vector2(50, 50);
                    go.SetActive(Player.userPlayer.levelSettings.respawnMode == 1);
                }
                else if (id == 0.4F) //Move
                {
                    var go = Instantiate(TriggerPref[2], pos, rot, place);
                    go.name = "Objet n° " + num;
                    go.transform.localScale = new Vector2(50, 50);

                    var moveTrigger = go.GetComponent<MoveTrigger>();
                    int.TryParse(GetBlocStatus("AffectationType", num), out moveTrigger.AffectationType);
                    int.TryParse(GetBlocStatus("Group", num), out moveTrigger.Group);
                    try { moveTrigger.Translation = Editor_MoveTrigger.getVector2(GetBlocStatus("Translation", num)); } catch { }
                    var translationFrom = GetBlocStatus("TranslationFrom", num);
                    try
                    {
                        var translationFromArray = translationFrom.Substring(1, translationFrom.Length - 2).Split(',');
                        for (var i = 0; i < translationFromArray.Length; i++)
                            bool.TryParse(translationFromArray[i], out moveTrigger.TranslationFromPlayer[i]);
                    }
                    catch { }
                    var reset = GetBlocStatus("Reset", num);
                    try
                    {
                        var resetArray = reset.Substring(1, reset.Length - 2).Split(',');
                        for (var i = 0; i < resetArray.Length; i++) bool.TryParse(resetArray[i], out moveTrigger.Reset[i]);
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
            if (hex.Length < 6 | hex.Length > 9) return new Color32(190, 190, 190, 255);

            var r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            var g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            var b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
            var a = byte.Parse(hex.Substring(6), NumberStyles.Number);
            return new Color32(r, g, b, a);
        }
        public string GetBlocStatus(string StatusID, int Bloc)
        {
            if (level.blocks.Length > Bloc)
            {
                var block = level.blocks[Bloc];

                if (StatusID == "ID") return block.id.ToString();
                if (StatusID == "Position") return block.position.ToString();
                if (StatusID == "PositionX") return block.position.x.ToString();
                if (StatusID == "PositionY") return block.position.y.ToString();
                if (StatusID == "Layer") return block.position.z.ToString();
                if (block.parameter.ContainsKey(StatusID)) return block.parameter[StatusID];
                return "";
            }

            return "";
        }

        private static float oldSpeed = 1;
        public static void Pause(bool pause)
        {
            if (pause) Time.timeScale = 0;
            else Time.timeScale = 1;

            var player = Player.userPlayer;
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
                var scene = FromScene;
                string[] args = null;
                if (FromScene == "") scene = "Home";
                else if (FromScene.Contains("/"))
                {
                    var FromSceneDetails = FromScene.Split(new[] { "/" }, StringSplitOptions.None);
                    scene = FromSceneDetails[0];
                    args = FromSceneDetails.RemoveAt(0);
                    GameObject.Find("Audio").GetComponent<MenuMusic>().StartDefault();
                }
                else if (FromScene == "Editor")
                {
                    scene = FromScene;
                    args = passThroughArgs;
                    GameObject.Find("Audio").GetComponent<MenuMusic>().Stop();
                }

                if (scene != "Editor")
                {
                    var path = Application.temporaryCachePath + "/downloadedRP/";
                    if (Directory.Exists(path)) Directory.Delete(path, true);
                    Sprite_API.forceRP = null;
                }
                SceneManager.LoadScene(scene, args);
            }
#if EnableMultiplayer
            else
            {
                GameObject.Find("Network Manager").GetComponent<_NetworkManager>().Disconnect();
            }
#endif
        }

        public void Replay()
        {
            Time.timeScale = 1;
            SceneManager.ReloadScene();
        }

        public static void Lost(Transform Base)
        {
            Pause(true);
            Base.GetChild(6).gameObject.SetActive(true);
        }
    }
}
