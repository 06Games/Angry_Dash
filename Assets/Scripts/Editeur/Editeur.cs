using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Level
{
    [System.Serializable]
    public class Infos : System.IEquatable<Infos>
    {
        public string name;
        public string description;
        public string author;
        public Background background;
        public SongItem music;
        public Versioning version;
        public int respawnMode;

        public Block[] blocks;

        public override string ToString() { return FileFormat.XML.Utils.ClassToXML(this, !ConfigAPI.GetBool("editor.beautify")); }
        public static Infos Parse(string xml) { return FileFormat.XML.Utils.XMLtoClass<Infos>(xml); }

        public bool Equals(Infos other)
        {

            if (ReferenceEquals(other, null)) return false; //If parameter is null, return false.
            if (ReferenceEquals(this, other)) return true; //Optimization for a common success case.
            if (GetType() != other.GetType()) return false; //If run-time types are not exactly the same, return false.
            
            if (name == other.name
                & description == other.description
                & author == other.author
                & background == other.background
                & music == other.music
                & version == other.version
                & respawnMode == other.respawnMode
                & Block.Equals(blocks,other.blocks))
                return true;
            else return false;
        }
        public override bool Equals(object obj) { return Equals(obj as Infos); }
        public static bool operator ==(Infos left, Infos right)
        {
            if (left is null & right is null) return true;
            else if (left is null | right is null) return false;
            else return left.Equals(right);
        }
        public static bool operator !=(Infos left, Infos right) { return !(left == right); }
        public override int GetHashCode() { return base.GetHashCode(); }
        public void CopyTo(out Infos other)
        {
            other = new Infos();
            other.name = name;
            other.description = description;
            other.author = author;
            other.background = background;
            other.music = music;
            other.version = version;
            other.respawnMode = respawnMode;
            other.blocks = new Block[blocks.Length];
            for (int i = 0; i < blocks.Length; i++) blocks[i].CopyTo(out other.blocks[i]);
        }
    }

    [System.Serializable]
    public class Background
    {
        public string category = "native";
        public int id;
        public Color32 color;

        public bool Equals(Background other)
        {
            if (ReferenceEquals(other, null)) return false; //If parameter is null, return false.
            if (ReferenceEquals(this, other)) return true; //Optimization for a common success case.
            if (GetType() != other.GetType()) return false; //If run-time types are not exactly the same, return false.

            if (category == other.category
                & id == other.id
                & color.Equals(other.color))
                return true;
            else return false;
        }
        public override bool Equals(object obj) { return Equals(obj as Background); }
        public static bool operator ==(Background left, Background right)
        {
            if (left is null & right is null) return true;
            else if (left is null | right is null) return false;
            else return left.Equals(right);
        }
        public static bool operator !=(Background left, Background right) { return !(left == right); }
        public override int GetHashCode() { return base.GetHashCode(); }
    }

    [System.Serializable]
    public class Block
    {
        public enum Type { Block, Event } public Type type;
        public string category = "native";
        public float id;
        public Vector3 position;
        public Tools.Dictionary.Serializable<string, string> parameter = new Tools.Dictionary.Serializable<string, string>();

        public bool Equals(Block other)
        {
            if (other is null) return false; //If parameter is null, return false.
            if (ReferenceEquals(this, other)) return true; //Optimization for a common success case.
            if (GetType() != other.GetType()) return false; //If run-time types are not exactly the same, return false.

            if (type == other.type
                & category == other.category
                & id == other.id
                & position == other.position
                & parameter == other.parameter)
                return true;
            else return false;
        }
        public override bool Equals(object obj) { return Equals(obj as Block); }
        public static bool Equals(Block[] left, Block[] right)
        {
            bool match = true;
            if(left is null & right is null) match = true;
            if (left is null | right is null) match = false;
            else if (left.Length != right.Length) match = false;
            else
            {
                for (int i = 0; i < left.Length & i < right.Length; i++)
                {
                    if (!(left[i] is null & right[i] is null))
                    {
                        if (left[i] is null | right[i] is null) match = false;
                        else if (!left[i].Equals(right[i])) match = false;
                    }
                }
            }
            return match;
        }
        public static bool operator ==(Block left, Block right)
        {
            if (left is null & right is null) return true;
            else if (left is null | right is null) return false;
            else return left.Equals(right);
        }
        public static bool operator !=(Block left, Block right) { return !(left == right); }
        public override int GetHashCode() { return base.GetHashCode(); }
        public void CopyTo(out Block other)
        {
            other = new Block();
            other.type = type;
            other.category = category;
            other.id = id;
            other.position = position;
            parameter.CopyTo(out other.parameter);
        }
    }
}

public class Editeur : MonoBehaviour
{
    public LoadingScreenControl LSC;
    public Transform LoadingLevel;
    public Soundboard SoundBoard;
    public string FromScene = "Home";

    public string file;
    public Level.Infos level;

    bool AddBlocking;
    float newblockid;
    public GameObject Prefab;

    public bool SelectBlocking;
    public int[] SelectedBlock;
    public GameObject NoBlocSelectedPanel;
    public GameObject[] Contenu;
    public Scrollbar zoomIndicator;

    Camera cam;
    public int ZoomSensitive = 20;

    bool SelectMode = false;
    public bool bloqueSelect = false;

    public GameObject BulleDeveloppementCat;
    public Sprite[] BulleDeveloppementCatSp;

    public Sprite GrilleSp;

    public bool bloqueEchap;

#if UNITY_STANDALONE || UNITY_EDITOR
    public int CameraMouvementSpeed = 10;
#endif

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
    bool MultiSelect = false;
    Vector2 touchLastPosition;
#endif

    #region UI
    public void CreateFile(string path, string desc)
    {
        file = path;
        gameObject.SetActive(true);

        Level.Infos lvl = new Level.Infos()
        {
            name = Path.GetFileNameWithoutExtension(new FileInfo(path).FullName),
            description = desc,
            author = Account.Username,
            version = Versioning.Actual,
            background = new Level.Background() { category = "native", id = 1, color = new Color32(75, 75, 75, 255) },
            music = null,
            respawnMode = 0,
            blocks = new Level.Block[] { }
        };


        File.WriteAllText(file, lvl.ToString());
        EditFile(file);
    }

    public void EditFile(string txt)
    {
        gameObject.SetActive(true);
        StartCoroutine(editFile(txt));
    }
    IEnumerator editFile(string txt)
    {
        if (!File.Exists(txt))
        {
            string[] FromSceneDetails = FromScene.Split(new string[] { "/" }, System.StringSplitOptions.None);
            LSC.LoadScreen(FromSceneDetails[0], FromSceneDetails.RemoveAt(0));
        }
        else
        {
            LvlLoadingActivation(true);
            int actualValue = 0;
            int maxValue = 5;

            file = txt;
            string fileText = File.ReadAllText(file);

            if (level == null)
            {
                string[] FromSceneDetails = FromScene.Split(new string[] { "/" }, System.StringSplitOptions.None);
                LSC.LoadScreen(FromSceneDetails[0], FromSceneDetails.RemoveAt(0));
            }
            else
            {
                actualValue++;
                LvlLoadingStatus(actualValue, maxValue, LangueAPI.String("native", "editorExploreLoadingVersionCheck", "Checking the level version"));
                yield return new WaitForEndOfFrame();
                string updated = UpdateLevel(fileText, file);
                level = Level.Infos.Parse(updated);
                if (updated != fileText) File.WriteAllText(file, updated); //The level was updated, save changes

                actualValue++;
                LvlLoadingStatus(actualValue, maxValue, LangueAPI.String("native", "editorExploreLoadingBlocks", "Placing Blocks"));
                yield return new WaitForEndOfFrame();

                float each = (int)(level.blocks.Length * 0.25F);
                if (level.blocks.Length < 100) each = -1F;
                for (int i = 0; i < level.blocks.Length; i++)
                {
                    if (each > 0 & (int)(i / each) == i / each)
                    {
                        LvlLoadingStatus(actualValue, maxValue, LangueAPI.StringWithArgument("native", "editorExploreLoadingBlocksStatus", new string[] { i.ToString(), level.blocks.Length.ToString() }, "Placing Blocks : [0]/[1]"));
                        yield return new WaitForEndOfFrame();
                    }
                    Instance(i);
                }
                transform.GetChild(0).gameObject.SetActive(true);



                actualValue++;
                LvlLoadingStatus(actualValue, maxValue, LangueAPI.String("native", "editorExploreLoadingBackgrounds", "Caching Backgrounds"));
                yield return new WaitForEndOfFrame();
                transform.GetChild(0).GetChild(2).GetChild(1).GetChild(0).GetChild(1).GetComponent<Background>().ActualiseFond(this); //Caching Backgrounds

                actualValue++;
                LvlLoadingStatus(actualValue, maxValue, LangueAPI.String("native", "editorExploreLoadingMusics", "Refreshing the list of music"));
                yield return new WaitForEndOfFrame();
                SoundBoard.RefreshList(); //Refresh musics list


                actualValue++;
                LvlLoadingStatus(actualValue, maxValue, LangueAPI.String("native", "editorExploreLoadingOpen", "Opening Level"));
                yield return new WaitForEndOfFrame();
                OpenCat(-1);

                Discord.Presence(LangueAPI.String("native", "discordEditor_title"), LangueAPI.StringWithArgument("native", "discordEditor_subtitle", level.name), new DiscordClasses.Img("default"));
                cam.GetComponent<BaseControl>().returnScene = false;

                LvlLoadingActivation(false);
                History.LvlPlayed(file, "E");
                if (GameObject.Find("Audio") != null) GameObject.Find("Audio").GetComponent<menuMusic>().Stop();

                saveMethode = (SaveMethode)ConfigAPI.GetInt("editor.autoSave");
            }
        }
    }

    /// <summary>
    /// Change Loading Status,
    /// return true when finished
    /// </summary>
    /// <param name="actualValue">Actual value</param>
    /// <param name="maxValue">Max value</param>
    /// <param name="text">Status Message</param>
    public bool LvlLoadingStatus(float actualValue, float maxValue, string text)
    {
        LoadingLevel.GetChild(1).GetComponent<Text>().text = text;
        LoadingLevel.GetChild(2).GetComponent<Scrollbar>().size = actualValue / maxValue;
        return true;
    }
    public void LvlLoadingActivation(bool activate)
    {
        UnityThread.executeInUpdate(() => LoadingLevel.gameObject.SetActive(activate));
        LoadingLevel.GetChild(1).GetComponent<Text>().text = "Initialisation";
        LoadingLevel.GetChild(2).GetComponent<Scrollbar>().size = 0;
    }

    public void ExitEdit()
    {
        SaveLevel();
        if (GameObject.Find("Audio") != null) GameObject.Find("Audio").GetComponent<menuMusic>().StartDefault();
        string[] FromSceneDetails = FromScene.Split(new string[] { "/" }, System.StringSplitOptions.None);
        LSC.LoadScreen(FromSceneDetails[0], FromSceneDetails.RemoveAt(0));
    }
    #endregion

    private void Start()
    {
        Discord.Presence(LangueAPI.String("native", "discordEditor_title"), "", new DiscordClasses.Img("default"));
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        cam.transform.position = new Vector3(Screen.width / 2, Screen.height / 2, -10);
        cam.GetComponent<Camera>().orthographicSize = Screen.height / 2;

        GrilleOnOff(ConfigAPI.GetBool("editor.Grid"), transform.GetChild(0).GetChild(5).GetComponent<UImage_Reader>());

        zoomIndicator.gameObject.SetActive(false);
        BulleDeveloppementCat.SetActive(false);

        string[] args = cam.GetComponent<BaseControl>().LSC.GetArgs();
        if (args != null)
        {
            if (args.Length > 2)
            {
                if (args[0] != "Player")
                {
                    FromScene = args[0];
                    if (args[1] == "Edit")
                    {
                        file = args[2];
                        EditFile(file);
                    }
                    else if (args[1] == "Create" & args.Length > 3)
                        CreateFile(args[2], args[3]);
                }
            }
            else LSC.LoadScreen(FromScene);
        }
        else
        {
#if UNITY_EDITOR
            EditFile(Application.persistentDataPath + "/Levels/Official Levels/4.level");
#else
            LSC.LoadScreen(FromScene);
#endif
        }

        transform.GetChild(0).GetChild(6).gameObject.SetActive(ConfigAPI.GetBool("editor.showCoordinates"));
    }

    void Update()
    {
        GetComponent<SurClique>().enabled = SelectMode;
#if UNITY_EDITOR || UNITY_STANDALONE
        transform.GetChild(0).GetChild(6).GetComponent<Text>().text =
        GetWorldPosition(Input.mousePosition, false).ToString("0.0");
#else
        transform.GetChild(0).GetChild(6).GetComponent<Text>().text = 
        GetWorldPosition(Display.Screen.Resolution / 2, false).ToString("0.0");
#endif

        //Détection de la localisation lors de l'ajout d'un bloc
        if (AddBlocking & !bloqueSelect)
        {
            if (Input.GetKey(KeyCode.Mouse0) & !SelectMode)
            {
                if (Input.mousePosition.y > Screen.height / 4)
                {
                    bool isInTop = Input.mousePosition.y > Screen.height - (Screen.height / 10);
                    bool isInRightTop = Input.mousePosition.x > Screen.width - (Screen.width / 6);

                    if (!(isInTop & isInRightTop))
                    {
                        Vector2 pos = GetWorldPosition(Input.mousePosition);

                        float id = newblockid;
                        if (id > 10000)
                            id = (newblockid - 10000F) / 10F;

                        CreateBloc((int)pos.x, (int)pos.y, new Color32(190, 190, 190, 255));
                    }
                }
            }
        }

        if (SelectBlocking & !bloqueSelect)
        {
            if (Input.mousePosition.y > Screen.height / 4)
            {
                Vector2 pos = GetWorldPosition(Input.mousePosition);

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
                bool SelectCtrl = MultiSelect;
#else
                bool SelectCtrl = Input.GetKey(KeyCode.LeftControl) | Input.GetKey(KeyCode.RightControl);
#endif

                int[] SelectedBlocks = SelectedBlock;

                int Selected = GetBloc((int)pos.x, (int)pos.y);

                if (Selected == -1 & !SelectCtrl) SelectedBlock = new int[0];
                else if (Selected != -1 & SelectCtrl)
                {
                    float blockId = level.blocks[Selected].id;
                    float firstId = -1;
                    if (SelectedBlock.Length > 0) firstId = level.blocks[SelectedBlock[0]].id;

                    bool pass = false;
                    if (blockId >= 1 & firstId >= 1) pass = true;
                    else if (blockId < 1 & blockId > 0 & blockId == firstId) pass = true;
                    else if (firstId == -1) pass = true;

                    if (pass) SelectedBlock = SelectedBlock.Union(new int[] { Selected }).ToArray();
                }
                else if (!SelectCtrl) SelectedBlock = new int[] { Selected };

                for (int i = 0; i < SelectedBlocks.Length; i++)
                {
                    Transform obj = transform.GetChild(1).Find("Objet n° " + SelectedBlocks[i]);
                    if (obj != null) obj.transform.GetChild(0).gameObject.SetActive(false);
                }

                SelectBlocking = false;
            }
        }


#if UNITY_STANDALONE || UNITY_EDITOR
        if (Input.GetKey(KeyCode.Mouse1) & !bloqueSelect & SelectMode)
        {
#else
        SimpleGesture.OnLongTap(() => {
#endif
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            if (!bloqueSelect & SelectMode)
            {
                bool SelectCtrl = MultiSelect;
#else
            bool SelectCtrl = Input.GetKey(KeyCode.LeftControl) | Input.GetKey(KeyCode.RightControl);
#endif
            if (Input.mousePosition.y > Screen.height / 4)
            {
                Vector2 pos = GetWorldPosition(Input.mousePosition);
                int Selected = GetBloc((int)pos.x, (int)pos.y);
                if (Selected != -1 & SelectCtrl)
                {
                    List<int> list = new List<int>(SelectedBlock);
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (SelectedBlock[i] == Selected)
                        {
                            list.RemoveAt(i);
                            Transform obj = transform.GetChild(1).Find("Objet n° " + SelectedBlock[i]);
                            SelectedBlock = list.ToArray();
                            if (obj != null) obj.transform.GetChild(0).gameObject.SetActive(false);
                        }
                    }
                    SelectedBlock = list.ToArray();
                }
            }
        }
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        });
#endif
        if (NoBlocSelectedPanel.activeInHierarchy & !(!Contenu[3].activeInHierarchy & !Contenu[0].activeInHierarchy & !Contenu[1].activeInHierarchy & !Contenu[4].activeInHierarchy & SelectedBlock.Length == 0)) Contenu[2].GetComponent<Edit>().EnterToEdit();
        NoBlocSelectedPanel.SetActive(!Contenu[3].activeInHierarchy & !Contenu[0].activeInHierarchy & !Contenu[1].activeInHierarchy & !Contenu[4].activeInHierarchy & SelectedBlock.Length == 0);
        if (SelectedBlock.Length > 0)
        {
            for (int i = 0; i < SelectedBlock.Length; i++)
            {
                Transform obj = transform.GetChild(1).Find("Objet n° " + SelectedBlock[i]);
                if (obj != null) obj.transform.GetChild(0).gameObject.SetActive(true);
            }
        }


#if UNITY_STANDALONE || UNITY_EDITOR
        bool Ctrl = Input.GetKey(KeyCode.LeftControl) | Input.GetKey(KeyCode.RightControl);
        if (Input.GetAxis("Mouse ScrollWheel") > 0 & Ctrl)
            Zoom();
        else if (Input.GetAxis("Mouse ScrollWheel") < 0 & Ctrl)
            Dezoom();
#elif UNITY_ANDROID || UNITY_IOS
        // If there are two touches on the device...
        if (Input.touchCount == 2)
        {
            // Store both touches.
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Find the position in the previous frame of each touch.
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Find the magnitude of the vector (the distance) between the touches in each frame.
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Find the difference in the distances between each frame.
            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            // ... change the orthographic size based on the change in distance between the touches.
            if (deltaMagnitudeDiff < 0)
                Zoom(5);
            else if(deltaMagnitudeDiff > 0)
                Dezoom(5);
        }
#endif

#if UNITY_STANDALONE || UNITY_EDITOR
        int MoveX = 0;
        int MoveY = 0;

        int Speed = CameraMouvementSpeed;
        if (Input.GetKey(KeyCode.LeftShift) | Input.GetKey(KeyCode.RightShift))
            Speed = CameraMouvementSpeed * 2;

        if (Input.GetKey(KeyCode.RightArrow))
            MoveX = 1;
        else if (Input.GetKey(KeyCode.LeftArrow))
            MoveX = -1;

        if (Input.GetKey(KeyCode.UpArrow))
            MoveY = 1;
        else if (Input.GetKey(KeyCode.DownArrow))
            MoveY = -1;

        Deplacer(MoveX * Speed, MoveY * Speed);
#elif UNITY_ANDROID || UNITY_IOS
        bool isSimple = !AddBlocking & (Input.touchCount == 1 | Input.touchCount == 2);
        bool isAdvence = AddBlocking & (Input.touchCount == 2 | Input.touchCount == 3);
        if (isAdvence | isSimple)
        {
            Touch touchZero = Input.GetTouch(0);

            bool isInTop = touchZero.position.y > Screen.height - (Screen.height / 10);
            bool isInRightTop = touchZero.position.x > Screen.width - (Screen.width / 6);
            if (touchZero.position.y > Screen.height / 4 & !(isInTop & isInRightTop))
            {
                float Speed = 1F;
                if ((Input.touchCount == 2 & isSimple) | (Input.touchCount == 3 & isAdvence))
                    Speed = Speed * 2;
                
                Vector2 TouchPosition =  touchZero.position;
                if(touchLastPosition == new Vector2(-50000, -50000))
                    touchLastPosition = TouchPosition;
                Vector2 touchZeroPrevPos = touchLastPosition - TouchPosition; // Find the diference between the last position.
                Deplacer(touchZeroPrevPos.x * Speed, touchZeroPrevPos.y * Speed);
                touchLastPosition = TouchPosition;
            }
        }
        else touchLastPosition = new Vector2(-50000, -50000);
#endif
    }

    public enum SaveMethode
    {
        Everytime,
        EveryChange,
        EveryMinute,
        EveryFiveMinutes
    }
    private SaveMethode _saveMethode;
    public SaveMethode saveMethode
    {
        get { return _saveMethode; }
        set { _saveMethode = value; if (!AutoSaveLevelEnabled) StartCoroutine(AutoSaveLevel()); }
    }
    bool AutoSaveLevelEnabled = false;
    IEnumerator AutoSaveLevel()
    {
        AutoSaveLevelEnabled = true;

        if (_saveMethode == SaveMethode.Everytime) yield return new WaitForFixedUpdate();
        else if (_saveMethode == SaveMethode.EveryChange)
        {
            Level.Infos oldComponent = new Level.Infos();
            level.CopyTo(out oldComponent);
            while (level.Equals(oldComponent))
                yield return new WaitForEndOfFrame();
        }
        else if (_saveMethode == SaveMethode.EveryMinute) yield return new WaitForSeconds(60);
        else if (_saveMethode == SaveMethode.EveryFiveMinutes) yield return new WaitForSeconds(60 * 5);
        else throw new System.NotImplementedException("¯\\_(ツ)_/¯");

        SaveLevel();
        AutoSaveLevelEnabled = false;
        StartCoroutine(AutoSaveLevel());
    }
    public void SaveLevel()
    {
        Logging.Log("Start saving...", LogType.Log);
        if (file != "" & level != null)
            File.WriteAllText(file, level.ToString());
        Logging.Log("Saving completed !", LogType.Log);
    }
    public Vector2 GetWorldPosition(Vector2 pos, bool round = true) { return GetWorldPosition(pos, round, cam.transform.position); }
    public Vector2 GetWorldPosition(Vector2 pos, bool round, Vector2 camPos)
    {
        float zoom = cam.orthographicSize / Screen.height * 2;
        Vector2 Cam0 = new Vector2(camPos.x - (cam.pixelWidth * zoom / 2),
            camPos.y - (cam.pixelHeight * zoom / 2));
        float x = (pos.x * zoom + Cam0.x - 25) / 50F;
        float y = (pos.y * zoom + Cam0.y - 25) / 50F;

        if (round) return new Vector2(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
        else return new Vector2(x, y);
    }

    public void ChangeMultiSelect(Image btn)
    {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        MultiSelect = !MultiSelect;
        if(MultiSelect) btn.color = new Color32(75, 75, 75, 255);
        else btn.color = new Color32(125, 125, 125, 255);
#endif
    }

    void Zoom(int Z = 0)
    {
        if (Z == 0)
            Z = ZoomSensitive;

        if (cam.orthographicSize > 240)
            cam.orthographicSize = cam.orthographicSize - Z;
        StartCoroutine(ZoomIndicator());
        Grille(true);
    }
    void Dezoom(int Z = 0)
    {
        if (Z == 0)
            Z = ZoomSensitive;

        if (cam.orthographicSize < 1200)
            cam.orthographicSize = cam.orthographicSize + Z;
        StartCoroutine(ZoomIndicator());
        Grille(true);
    }
    IEnumerator ZoomIndicator()
    {
        zoomIndicator.gameObject.SetActive(true);
        zoomIndicator.value = (cam.orthographicSize - 240) / 960 * -1 + 1;
        yield return new WaitForSeconds(1F);

        bool Ctrl = Input.GetKey(KeyCode.LeftControl) | Input.GetKey(KeyCode.RightControl);
        if (Input.GetAxis("Mouse ScrollWheel") == 0 & !Ctrl)
            zoomIndicator.gameObject.SetActive(false);
    }
    public void GrilleOnOff(UImage_Reader Img)
    {
        Transform _Grille = transform.GetChild(1).GetChild(1);
        GrilleOnOff(!_Grille.GetComponent<SpriteRenderer>().enabled, Img);
    }
    void GrilleOnOff(bool on, UImage_Reader Img)
    {
        ConfigAPI.SetBool("editor.Grid", on);
        if (on) //Grid ON
        {
            Img.SetID("native/GUI/editor/gridOn").Load();
            Grille(true, false);
        }
        else //Grid OFF
        {
            Img.SetID("native/GUI/editor/gridOff").Load();
            Grille(false, true);
        }
    }
    void Grille(bool needRespawn, bool del = false)
    {
        if (!ConfigAPI.GetBool("editor.Grid") & needRespawn) return; //Can't create a grid if the user doesn't want it ...
        Transform _Grille = transform.GetChild(1).GetChild(1);

        if (needRespawn)
        {
            _Grille.GetComponent<SpriteRenderer>().enabled = true;
            _Grille.localScale = new Vector2(100, 100) / GrilleSp.Size() * 50; //One grid block size

            //Grid Size
            _Grille.GetComponent<SpriteRenderer>().size = GetWorldPosition(Display.Screen.Resolution, false,
                Display.Screen.Resolution * (cam.orthographicSize / Display.Screen.Resolution.y)) + new Vector2(3, 3);
        }


        if (del) _Grille.GetComponent<SpriteRenderer>().enabled = false;
        else //Place correctly the grid
        {
            Vector2 bottomLeft = (GetWorldPosition(new Vector2(0, 0), false) * 50F) + new Vector2(25, 25); //Bottom Left Corner
            Vector2 center = _Grille.GetComponent<SpriteRenderer>().size * _Grille.localScale * 0.5F; //Grid Center
            _Grille.localPosition = (bottomLeft.Round(50) + center) - new Vector2(50, 50);
        }
    }

    #region GestionBloc
    void CreateBloc(int x, int y, Color32 _Color)
    {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        if (Input.touchCount == 1)
        {
#endif
        if (newblockid <= 0) return;
        Vector3 a = new Vector3(x, y, 0);

        float _id = newblockid;
        Level.Block.Type _type = Level.Block.Type.Block;
        if (newblockid > 10000) { _id = (newblockid - 10000F) / 10F; _type = Level.Block.Type.Event; }

        Level.Block newBlock = new Level.Block
        {
            type = _type,
            category = "native",
            id = _id,
            position = a
        };

        if (!level.blocks.Contains(newBlock))
        {
            level.blocks = level.blocks.Union(new Level.Block[] { newBlock }).ToArray();
            if (level.blocks.Length > 0) Instance(level.blocks.Length - 1);
        }

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        }
#endif
    }
    public void AddBlock(string _id)
    {
        float id = float.Parse(_id);

        if (id != newblockid)
        {
            newblockid = id;
            AddBlocking = true;
        }
        else if (id > 10000)
        {
            newblockid = -1;
            AddBlocking = false;
        }

        BulleDeveloppementCat.SetActive(false);
        if (id <= 10000)
        {
            for (int i = 1; i < Contenu[3].transform.childCount - 2; i++)
            {
                if (i == (int)newblockid & AddBlocking)
                    Contenu[3].transform.GetChild(i).GetComponent<Image>().color = new Color32(70, 70, 70, 255);
                else Contenu[3].transform.GetChild(i).GetComponent<Image>().color = new Color32(0, 0, 0, 255);
                if (i == (int)newblockid) Contenu[3].transform.GetChild(i).GetChild(0).GetComponent<UImage_Reader>().SetID("native/BLOCKS/" + newblockid.ToString("0.0####")).Load();
            }
        }
        else
        {
            for (int i = 1; i < Contenu[1].transform.childCount - 1; i++)
            {
                int v = (int)(newblockid - 10000F);
                if (i == v & AddBlocking) Contenu[1].transform.GetChild(i).GetComponent<Image>().color = new Color32(70, 70, 70, 255);
                else Contenu[1].transform.GetChild(i).GetComponent<Image>().color = new Color32(45, 45, 45, 255);
            }
        }
    }
    public void OpenCat(int id)
    {
        if (id == (int)newblockid)
        {
            newblockid = -1;
            AddBlocking = false;
            AddBlock(newblockid.ToString());
        }
        else if (id >= 0)
        {
            Vector2 pos = Contenu[3].transform.GetChild(id).localPosition;
            pos.x = pos.x + 20;
            if (id / 2F != id / 2)
            {
                pos.y = pos.y - 80;
                Image im = BulleDeveloppementCat.GetComponent<Image>();
                im.sprite = BulleDeveloppementCatSp[0];
                BulleDeveloppementCat.transform.GetChild(0).GetComponent<Image>().sprite = BulleDeveloppementCatSp[3];
                BulleDeveloppementCat.transform.GetChild(0).GetChild(0).localPosition = new Vector3(0, -8.75F, 0);
            }
            else
            {
                pos.y = pos.y + 80;
                Image im = BulleDeveloppementCat.GetComponent<Image>();
                im.sprite = BulleDeveloppementCatSp[1];
                BulleDeveloppementCat.transform.GetChild(0).GetComponent<Image>().sprite = BulleDeveloppementCatSp[2];
                BulleDeveloppementCat.transform.GetChild(0).GetChild(0).localPosition = new Vector3(0, 8.75F, 0);
            }

            if (!BulleDeveloppementCat.activeInHierarchy | BulleDeveloppementCat.transform.localPosition != (Vector3)pos)
            {
                for (int i = 1; i < BulleDeveloppementCat.transform.childCount; i++)
                    Destroy(BulleDeveloppementCat.transform.GetChild(i).gameObject);
                BulleDeveloppementCat.transform.GetChild(0).gameObject.SetActive(false);

                string path = Application.persistentDataPath + "/Ressources/default/textures/native/BLOCKS/";
                for (int i = 0; i < Directory.GetFiles(path, id + ".*", SearchOption.AllDirectories).Length; i++)
                {
                    GameObject newRef = Instantiate(BulleDeveloppementCat.transform.GetChild(0).gameObject, BulleDeveloppementCat.transform);
                    newRef.SetActive(true);
                    newRef.name = i.ToString();
                    newRef.transform.localPosition = new Vector3(i * 80, 0, 0);
                    newRef.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => AddBlock(id.ToString() + "." + newRef.name));

                    newRef.transform.GetChild(0).GetComponent<UImage_Reader>().SetID("native/BLOCKS/" + id + "." + i).Load();
                }
                BulleDeveloppementCat.SetActive(true);
                BulleDeveloppementCat.transform.localPosition = pos;
            }
            else
            {
                for (int i = 1; i < BulleDeveloppementCat.transform.childCount; i++)
                    Destroy(BulleDeveloppementCat.transform.GetChild(i).gameObject);

                BulleDeveloppementCat.SetActive(false);
            }
        }
        else
        {
            for (int i = 1; i < BulleDeveloppementCat.transform.childCount; i++)
                Destroy(BulleDeveloppementCat.transform.GetChild(i).gameObject);
            BulleDeveloppementCat.SetActive(false);

            for (int i = 1; i < Contenu[3].transform.childCount - 2; i++)
                Contenu[3].transform.GetChild(i).GetComponent<Image>().color = new Color32(0, 0, 0, 255);
        }
    }

    public void Instance(int num, bool keep = false)
    {
        float id = level.blocks[num].id;
        Vector3 p = level.blocks[num].position * 50 + new Vector3(25, 25, 0);
        Vector3 pos = new Vector3(p.x, p.y, 0);
        Quaternion rot = new Quaternion();
        try { rot.eulerAngles = new Vector3(0, 0, int.Parse(GetBlocStatus("Rotate", num))); }
        catch { rot.eulerAngles = new Vector3(0, 0, 0); }
        GameObject Pref = Prefab;

        Transform g = transform.GetChild(1);
        bool b = true;
        for (int i = 0; i < g.childCount; i++)
        {
            if (g.GetChild(i).gameObject.name == "Objet n° " + num)
                b = false;
        }
        if (b | keep)
        {
            GameObject go = null;
            if (keep)
            {
                go = GameObject.Find("Objet n° " + num);
                go.transform.position = pos;
                go.transform.rotation = rot;
            }
            else go = Instantiate(Pref, pos, rot, transform.GetChild(1));

            go.name = "Objet n° " + num;

            UImage_Reader UImage = go.GetComponent<UImage_Reader>();
            if (id < 1) UImage.SetID(level.blocks[num].category + "/GUI/editor/events/" + id.ToString("0.0####")).Load();
            else UImage.SetID(level.blocks[num].category + "/BLOCKS/" + id.ToString("0.0####")).Load();

            SpriteRenderer SR = go.GetComponent<SpriteRenderer>();
            go.transform.localScale = new Vector2(100, 100) / UImage.FrameSize * 50;
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Texture2D SelectedZoneSize = go.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite.texture;
                go.transform.GetChild(i).localScale = new Vector2(UImage.FrameSize.x / SelectedZoneSize.width, UImage.FrameSize.y / SelectedZoneSize.height);
            }

            SR.color = HexToColor(GetBlocStatus("Color", num));
            SR.sortingOrder = (int)level.blocks[num].position.z;
        }
    }
    public Vector3 GetObjectPos(int num)
    {
        Vector2 pos = level.blocks[num].position * 50 + new Vector3(25, 25, 0);
        return new Vector3(pos.x, pos.y, level.blocks[num].position.z);
    }
    public static string ColorToHex(Color32 color)
    {
        string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2") + color.a.ToString();
        return hex;
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
    #endregion

    public void SelectBloc()
    {
        SelectBlocking = true;
    }
    public int GetBloc(int x, int y)
    {
        int a = -1;
        for (int i = 0; i < level.blocks.Length; i++)
        {
            if ((Vector2)level.blocks[i].position == new Vector2(x, y)) a = i;
        }
        return a;
    }

    /// <summary>
    /// Chang Block Parameter
    /// </summary>
    /// <param name="StatusID">Parameter ID</param>
    /// <param name="_component">Parameter Value</param>
    /// <param name="Bloc">Blocks conserned</param>
    public void ChangBlocStatus(string StatusID, string _component, int[] Bloc)
    { ChangBlocStatus(new string[] { StatusID }, new string[] { _component }, Bloc); }
    /// <summary>
    /// Chang Blocks Parameter
    /// </summary>
    /// <param name="StatusID">Parameters IDs</param>
    /// <param name="_component">Parameters Values</param>
    /// <param name="Bloc">Blocks conserned</param>
    public void ChangBlocStatus(string[] StatusID, string[] _component, int[] Bloc)
    {
        if (Bloc != null)
        {
            for (int s = 0; s < StatusID.Length; s++)
            {
                for (int i = 0; i < Bloc.Length; i++)
                {
                    if (Bloc[i] != -1)
                    {
                        Level.Block block = level.blocks[Bloc[i]];

                        if (StatusID[s] == "ID") float.TryParse(_component[s], out block.id);
                        else if (StatusID[s] == "Position") Vector3Extensions.TryParse(_component[s], out block.position);
                        else if (StatusID[s] == "PositionX") float.TryParse(_component[s], out block.position.x);
                        else if (StatusID[s] == "PositionY") float.TryParse(_component[s], out block.position.y);
                        else if (StatusID[s] == "Layer") float.TryParse(_component[s], out block.position.z);
                        else if (block.parameter.ContainsKey(StatusID[s])) block.parameter[StatusID[s]] = _component[s];
                        else block.parameter.Add(StatusID[s], _component[s]);

                        Instance(Bloc[i], true);
                    }
                }
            }
        }
    }


    /// <summary>
    /// Get Block Parameter
    /// </summary>
    /// <param name="StatusID">Parameter ID</param>
    /// <param name="Bloc">Block conserned</param>
    /// <returns></returns>
    public string GetBlocStatus(string StatusID, int Bloc)
    {
        if (file != "" & level.blocks.Length > Bloc)
        {
            Level.Block block = level.blocks[Bloc];

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

    public void DeleteSelectedBloc(bool fromUpdateScript)
    {
        for (int v = 0; v < SelectedBlock.Length; v++)
        {
            if (SelectedBlock[v] != -1)
            {
                int SB = SelectedBlock[v];
                SelectedBlock[v] = -1;

                Transform obj = transform.GetChild(1).Find("Objet n° " + SB);
                if (obj != null)
                    Destroy(obj.gameObject);

                Level.Block[] NewComponent = new Level.Block[level.blocks.Length - 1];
                for (int i = 0; i < NewComponent.Length; i++)
                {
                    if (i < SB)
                    {
                        string[] Blocks = GetBlocStatus("Blocks", i).Split(new string[] { "," }, System.StringSplitOptions.None);
                        if (string.IsNullOrEmpty(Blocks[0]) | Blocks[0] == "Null") Blocks = new string[0];
                        if (Blocks.Length > 0)
                        {
                            for (int bloc = 0; bloc < Blocks.Length; bloc++)
                            {
                                List<string> list = new List<string>(Blocks);
                                if (int.Parse(Blocks[bloc]) == SB) list.RemoveAt(bloc);
                                else if (int.Parse(Blocks[bloc]) > SB) list[bloc] = (int.Parse(list[bloc]) - 1).ToString();
                                Blocks = list.ToArray();
                            }
                            string blocks = "Null";
                            if (Blocks.Length > 0) blocks = Blocks[0];
                            for (int b = 1; b < Blocks.Length; b++)
                                blocks = blocks + "," + Blocks[b];
                            ChangBlocStatus("Blocks", blocks, new int[] { i });
                        }

                        NewComponent[i] = level.blocks[i];
                    }
                    else
                    {
                        string[] Blocks = GetBlocStatus("Blocks", i + 1).Split(new string[] { "," }, System.StringSplitOptions.None);
                        if (string.IsNullOrEmpty(Blocks[0]) | Blocks[0] == "Null") Blocks = new string[0];
                        if (Blocks.Length > 0)
                        {
                            for (int bloc = 0; bloc < Blocks.Length; bloc++)
                            {
                                List<string> list = new List<string>(Blocks);
                                if (int.Parse(Blocks[bloc]) == SB) list.RemoveAt(bloc);
                                else if (int.Parse(Blocks[bloc]) > SB) list[bloc] = (int.Parse(list[bloc]) - 1).ToString();
                                Blocks = list.ToArray();
                            }
                            string blocks = "Null";
                            if (Blocks.Length > 0) blocks = Blocks[0];
                            for (int b = 1; b < Blocks.Length; b++)
                                blocks = blocks + "," + Blocks[b];
                            ChangBlocStatus("Blocks", blocks, new int[] { i + 1 });
                        }

                        Transform objet = transform.GetChild(1).Find("Objet n° " + (i + 1));
                        if (objet != null)
                            objet.name = "Objet n° " + i;

                        NewComponent[i] = level.blocks[i + 1];
                    }
                }
                level.blocks = NewComponent;
                SelectedBlock[v] = -1;
            }
            else if (!fromUpdateScript)
            {
                NoBlocSelectedPanel.SetActive(true);
                StartCoroutine(WaitForDesableNoBlocSelectedPanel(2F));
            }
        }
    }
    public IEnumerator WaitForDesableNoBlocSelectedPanel(float sec)
    {
        yield return new WaitForSeconds(sec);
        NoBlocSelectedPanel.SetActive(false);
    }

    public void Deplacer(float x, float y)
    {
        if (x != 0 | y != 0)
        {
            float CamX = cam.transform.position.x;
            float CamY = cam.transform.position.y;

            if (x < 0) CamX = cam.transform.position.x + x;
            else if (x > 0) CamX = cam.transform.position.x + x;

            if (y < 0) CamY = cam.transform.position.y + y;
            else if (y > 0) CamY = cam.transform.position.y + y;

            cam.transform.position = new Vector3(CamX, CamY, -10);
            Grille(false);
        }
    }

    public void PlayLevel()
    {
        string[] args = new string[] { "Editor", "File", file };
        string[] passThrough = new string[] { "Player", "Edit", file };
        GameObject.Find("LoadingScreen").GetComponent<LoadingScreenControl>().LoadScreen("Player", args.Concat(passThrough).ToArray(), true);
    }

    public void SelectModeChang(bool enable) { SelectMode = enable; }
    public void BloqueActions(bool on) { bloqueSelect = on; }
    public void OnEchap() { if (!string.IsNullOrEmpty(file) & !bloqueEchap) { ExitEdit(); cam.GetComponent<BaseControl>().returnScene = true; } }

    #region UpdateLevel
    /// <summary>
    /// Update a level to the newest version
    /// Warning : The level will be incompatible with older versions
    /// </summary>
    /// <param name="path">Path to the file</param>
    public static void UpdateLevel(FileInfo path)
    {
        string fileLines = File.ReadAllText(path.FullName);
        File.WriteAllText(path.FullName, UpdateLevel(fileLines, path.FullName));
    }

    /// <summary>
    /// Update a level to the newest version
    /// Warning : The level will be incompatible with older versions
    /// </summary>
    /// <param name="fileLines">File content</param>
    public static string UpdateLevel(string fileLines, string path = "")
    {
        if (FileFormat.XML.Utils.IsValid(fileLines)) //0.3 and upper
        {
            return fileLines;
        }
        else //0.2 - 0.2.2
        {
            string[] newFileLines = fileLines.Split(new string[] { "\n" }, System.StringSplitOptions.None);
            int v = -1;
            for (int x = 0; x < newFileLines.Length; x++)
            {
                if (newFileLines[x].Contains("version = ") & v == -1)
                {
                    v = x;
                    x = newFileLines.Length;
                }
            }
            Versioning version = new Versioning(0.2F);
            if (v != -1) version = new Versioning(newFileLines[v].Replace("version = ", ""));
            else
            {
                v = 0;
                newFileLines = new string[] { "version = 0.2" }.Union(newFileLines).ToArray();
            }

            //Upgrade to 0.2.1
            if (version.CompareTo(new Versioning("0.2"), Versioning.SortConditions.OlderOrEqual))
            {
                int d = -1;
                for (int x = 0; x < newFileLines.Length; x++)
                {
                    if (newFileLines[x].Contains("Blocks {") & d == -1)
                    {
                        d = x + 1;
                        x = newFileLines.Length;
                    }
                }
                if (d != -1)
                {
                    for (int i = d; i < newFileLines.Length; i++)
                    {
                        if (newFileLines[i] == "}") i = newFileLines.Length;
                        else
                        {
                            string[] parm = newFileLines[i].Split(new string[] { "; " }, System.StringSplitOptions.None);
                            try
                            {
                                if (float.Parse(parm[0]) >= 1) newFileLines[i] = parm[0] + "; " + parm[1] + "; {Rotate:" + parm[2] + "; Color:" + parm[3] + "; Behavior:" + parm[4] + "}";
                                else newFileLines[i] = parm[0] + "; " + parm[1] + "; {}";
                            }
                            catch { }
                        }
                    }
                }

                //Set new version number
                newFileLines[v] = "version = 0.2.1";
                version = new Versioning("0.2.1");
            }
            if(version.CompareTo(new Versioning("0.3"), Versioning.SortConditions.Older)) //Upgrade from 0.2.1 - 0.2.2 to 0.3
            {
                string Name = "";
                if (!string.IsNullOrEmpty(path))
                {
                    Name = Path.GetFileNameWithoutExtension(path);

                    //Drastic changes to the format, backup the level in case...
                    string backupFolder = Application.persistentDataPath + "/Levels/Edited Levels/Backups/";
                    if (!Directory.Exists(backupFolder)) Directory.CreateDirectory(backupFolder);
                    File.WriteAllText(backupFolder + Name + ".level", fileLines);
                }
                Level.Infos updated = new Level.Infos();
                updated.name = Name;
                updated.blocks = new Level.Block[0];

                bool blockArea = false;
                foreach (string line in newFileLines)
                {
                    if (line.Contains("description = ")) updated.description = line.Replace("description = ", "").Replace("\r", "");
                    else if (line.Contains("background = "))
                    {
                        string[] data = line.Replace("background = ", "").Replace("\r", "").Split(new string[] { "; " }, System.StringSplitOptions.None);
                        if (data.Length >= 2)
                        {
                            int ID = 1; int.TryParse(data[0], out ID);
                            updated.background = new Level.Background()
                            {
                                category = "native",
                                id = ID,
                                color = HexToColor(data[1])
                            };
                        }
                    }
                    else if (line.Contains("music = "))
                    {
                        string[] data = line.Replace("music = ", "").Replace("\r", "").Split(new string[] { " - " }, System.StringSplitOptions.None);
                        if(data.Length >= 2) updated.music = new Level.SongItem() { Artist = data[0], Name = data[1] };
                    }
                    else if (line.Contains("author = ")) updated.author = line.Replace("author = ", "").Replace("\r", "");
                    else if (line.Contains("respawnMode = ")) int.TryParse(line.Replace("respawnMode = ", "").Replace("\r", ""), out updated.respawnMode);
                    else if (line.Contains("Blocks {")) blockArea = true;
                    else if (blockArea)
                    {
                        if (line == "}") blockArea = false;
                        else if (!string.IsNullOrEmpty(line))
                        {
                            string[] data = line.Replace("\r", "").Split(new string[] { "; " }, System.StringSplitOptions.None);
                            if (data.Length >= 2)
                            {
                                float ID = 1; float.TryParse(data[0], out ID);
                                Level.Block.Type type = Level.Block.Type.Block;
                                if (ID < 1) type = Level.Block.Type.Event;

                                Tools.Dictionary.Serializable<string, string> parameters = new Tools.Dictionary.Serializable<string, string>();
                                try
                                {
                                    string[] pData = line.Split(new string[] { "; {" }, System.StringSplitOptions.None)[1].Split(new string[] { "}" }, System.StringSplitOptions.None)[0].Split(new string[] { "; " }, System.StringSplitOptions.None);
                                    foreach (string p in pData)
                                    {
                                        string[] param = p.Split(new string[] { ":" }, System.StringSplitOptions.None);
                                        if (param.Length == 2) parameters.Add(param[0], param[1]);
                                    }
                                }
                                catch { }

                                var array = new Level.Block[]{new Level.Block()
                                {
                                    type = type,
                                    category = "native",
                                    id = ID,
                                    position = Vector3Extensions.Parse(data[1]),
                                    parameter = parameters
                                }};
                                updated.blocks = updated.blocks.Concat(array).ToArray();
                            }
                        }
                    }
                }
                updated.version = new Versioning("0.3");
                return updated.ToString();
            }
            else return string.Join("\n", newFileLines);
        }
    }
    #endregion
}
