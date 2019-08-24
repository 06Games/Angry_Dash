using AngryDash.Image.Reader;
using AngryDash.Language;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tools;
using UnityEngine;
using UnityEngine.UI;

public class Editeur : MonoBehaviour
{
    Camera cam;
    public LoadingScreenControl LSC;
    public Transform LoadingLevel;
    public Background backgroundManager;
    string FromScene = "Home";

    [HideInInspector] public string file;
    public Level.Infos level;

    float newblockid = -1;
    [HideInInspector] public int selectedLayer = -2;
    public GameObject Prefab;
    public GameObject BulleDeveloppementCat;

    [HideInInspector] public bool canInteract = true; //Can the user place/select blocks?
    bool SelectMode = false; //Is the user in a menu where he has to select blocks ?
    public int[] SelectedBlock; //The selected blocks
    public GameObject NoBlocSelectedPanel;
    public MenuManager Toolbox;

    public Scrollbar zoomIndicator;
    public int ZoomSensitive = 20;

    [HideInInspector] public bool bloqueEchap;

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
            player = new Level.Player() { respawnMode = 0 },
            victoryConditions = new Level.VictoryConditions(),
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
                LvlLoadingStatus(actualValue, maxValue, LangueAPI.Get("native", "editor.loading.versionCheck", "Checking the level version"));
                yield return new WaitForEndOfFrame();
                string updated = Level.LevelUpdater.UpdateLevel(fileText, file);
                level = Level.Infos.Parse(updated);
                if (updated != fileText) File.WriteAllText(file, updated); //The level was updated, save changes

                actualValue++;
                LvlLoadingStatus(actualValue, maxValue, LangueAPI.Get("native", "editor.loading.blocks", "Placing Blocks"));
                yield return new WaitForEndOfFrame();

                float each = (int)(level.blocks.Length * 0.25F);
                if (level.blocks.Length < 100) each = -1F;
                for (int i = 0; i < level.blocks.Length; i++)
                {
                    if (each > 0 & (int)(i / each) == i / each)
                    {
                        LvlLoadingStatus(actualValue, maxValue, LangueAPI.Get("native", "editor.loading.blocksStatus", "Placing Blocks : [0]/[1]", i, level.blocks.Length));
                        yield return new WaitForEndOfFrame();
                    }
                    Instance(i);
                }
                transform.GetChild(0).gameObject.SetActive(true);


                actualValue++;
                LvlLoadingStatus(actualValue, maxValue, LangueAPI.Get("native", "editor.loading.backgrounds", "Caching Backgrounds"));
                yield return new WaitForEndOfFrame();
                backgroundManager.ActualiseFond(this); //Caching Backgrounds


                actualValue++;
                LvlLoadingStatus(actualValue, maxValue, LangueAPI.Get("native", "editor.loading.opening", "Opening Level"));
                yield return new WaitForEndOfFrame();
                if (level.player == null) level.player = new Level.Player();
                if (level.victoryConditions == null) level.victoryConditions = new Level.VictoryConditions();
                OpenCat(-1);

                Discord.Presence(LangueAPI.Get("native", "discordEditor_title", "In the editor"), LangueAPI.Get("native", "discordEditor_subtitle", "Editing [0]", level.name), new DiscordClasses.Img("default"));
                cam.GetComponent<BaseControl>().returnScene = false;

                LvlLoadingActivation(false);
                History.LvlPlayed(file, "E");
                if (GameObject.Find("Audio") != null) GameObject.Find("Audio").GetComponent<menuMusic>().Stop();

                saveMethode = (SaveMethode)ConfigAPI.GetInt("editor.autoSave");
                if (ConfigAPI.GetBool("editor.hideToolbox"))
                {
                    Toolbox.transform.parent.position *= new Vector2(1, -1);
                    Toolbox.transform.parent.GetChild(4).gameObject.SetActive(SystemInfo.deviceType != DeviceType.Desktop);
                }
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
        Discord.Presence(LangueAPI.Get("native", "discordEditor_title", "In the editor"), "", new DiscordClasses.Img("default"));
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        cam.transform.position = new Vector3(Screen.width / 2, Screen.height / 2, -10);
        cam.GetComponent<Camera>().orthographicSize = Screen.height / 2;

        GrilleOnOff(ConfigAPI.GetBool("editor.Grid"), transform.GetChild(0).GetChild(5).GetComponent<UImage_Reader>());

        zoomIndicator.gameObject.SetActive(false);
        BulleDeveloppementCat.SetActive(false);

        string[] args = LoadingScreenControl.GetLSC().GetArgs();
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
        transform.GetChild(0).GetChild(2).GetChild(3).GetChild(1).GetComponent<InputField>().text = LangueAPI.Get("native", "editor.layer.all", "All");
    }

    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        transform.GetChild(0).GetChild(6).GetComponent<Text>().text =
        GetWorldPosition(Input.mousePosition, false).ToString("0.0");
#else
        transform.GetChild(0).GetChild(6).GetComponent<Text>().text =
        GetWorldPosition(Display.Screen.Resolution / 2, false).ToString("0.0");
#endif

        //Détection de la localisation lors de l'ajout d'un bloc
        if (newblockid >= 0 & !SelectMode & canInteract)
        {
            if (Input.GetKey(KeyCode.Mouse0) && !IsHoverGUI())
            {
                Vector2 pos = GetWorldPosition(Input.mousePosition);
                CreateBlock((int)pos.x, (int)pos.y, new Color32(190, 190, 190, 255));
            }
        }

        if (SelectMode & canInteract)
        {
            if (Input.GetKey(KeyCode.Mouse0) && !IsHoverGUI())
            {
                Vector2 pos = GetWorldPosition(Input.mousePosition);

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
                bool SelectCtrl = MultiSelect;
#else
                bool SelectCtrl = Input.GetKey(KeyCode.LeftControl) | Input.GetKey(KeyCode.RightControl);
#endif

                int[] previouslySelected = SelectedBlock;

                int Selected = GetBloc((int)pos.x, (int)pos.y);
                if(Selected >= 0)
                {
                    if (level.blocks[Selected].id == 0) SelectCtrl = false;

                    if (SelectCtrl)
                    {
                        float blockId = level.blocks[Selected].id;
                        float firstId = -1;
                        if (SelectedBlock.Length > 0) firstId = level.blocks[SelectedBlock[0]].id;

                        bool sameFamily = false;
                        if (blockId >= 1 & firstId >= 1) sameFamily = true;
                        else if (blockId < 1 & blockId > 0 & blockId == firstId) sameFamily = true;
                        else if (firstId == -1) sameFamily = true;

                        if (sameFamily) SelectedBlock = SelectedBlock.Union(new int[] { Selected }).ToArray();
                    }
                    else SelectedBlock = new int[] { Selected };
                }
                else if(!SelectCtrl) SelectedBlock = new int[0];

                //Removes the selection marker from deselected blocks
                foreach (int block in previouslySelected.Except(SelectedBlock))
                {
                    Transform obj = transform.GetChild(1).Find("Objet n° " + block);
                    if (obj != null) obj.transform.GetChild(0).gameObject.SetActive(false);
                }
                //Adds the selection marker of the newly selected blocks
                foreach (int block in SelectedBlock.Except(previouslySelected))
                {
                    Transform obj = transform.GetChild(1).Find("Objet n° " + block);
                    if (obj != null) obj.transform.GetChild(0).gameObject.SetActive(true);
                }
            }

#if UNITY_STANDALONE || UNITY_EDITOR
            if (Input.GetKey(KeyCode.Mouse1))
            {
#else
            SimpleGesture.OnLongTap(() => {
#endif
                if (!IsHoverGUI())
                {
                    Vector2 pos = GetWorldPosition(Input.mousePosition);
                    int Selected = GetBloc((int)pos.x, (int)pos.y);
                    if (Selected != -1)
                    {
                        List<int> list = new List<int>(SelectedBlock);
                        list.Remove(Selected);
                        SelectedBlock = list.ToArray();

                        Transform obj = transform.GetChild(1).Find("Objet n° " + Selected);
                        if (obj != null) obj.transform.GetChild(0).gameObject.SetActive(false);
                    }
                }
#if UNITY_STANDALONE || UNITY_EDITOR
            }
#else
            });
#endif
        }
        NoBlocSelectedPanel.SetActive(SelectMode & SelectedBlock.Length == 0);

#if UNITY_STANDALONE || UNITY_EDITOR
        bool Ctrl = Input.GetKey(KeyCode.LeftControl) | Input.GetKey(KeyCode.RightControl);
        if (Input.GetAxis("Mouse ScrollWheel") > 0 & Ctrl) Zoom();
        else if (Input.GetAxis("Mouse ScrollWheel") < 0 & Ctrl) Dezoom();
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
        if (Input.GetKey(KeyCode.LeftShift) | Input.GetKey(KeyCode.RightShift)) Speed = CameraMouvementSpeed * 2;

        if (Input.GetKey(KeyCode.RightArrow)) MoveX = 1;
        else if (Input.GetKey(KeyCode.LeftArrow)) MoveX = -1;

        if (Input.GetKey(KeyCode.UpArrow)) MoveY = 1;
        else if (Input.GetKey(KeyCode.DownArrow)) MoveY = -1;

        Deplacer(MoveX * Speed, MoveY * Speed);
#elif UNITY_ANDROID || UNITY_IOS
        bool isSimple = newblockid < 0 & (Input.touchCount == 1 | Input.touchCount == 2);
        bool isAdvence = newblockid >= 0 & (Input.touchCount == 2 | Input.touchCount == 3);
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

#if UNITY_STANDALONE || UNITY_EDITOR
        if (ConfigAPI.GetBool("editor.hideToolbox"))
        {
            Transform toolbox = Toolbox.transform.parent;
            float toolboxY = toolbox.GetComponent<RectTransform>().sizeDelta.y * transform.GetChild(0).GetComponent<Canvas>().scaleFactor;
            if (toolbox.position.y < 0) //The toolbox is hidden
            {
                if (Input.mousePosition.y <= 25) //The mouse is on the bottom of the screen
                    StartCoroutine(HideToolbox(toolbox, toolboxY, false));
            }
            else //The toolbox is displayed
            {
                if (Input.mousePosition.y > toolboxY) //The mouse isn't on the toolbox
                    StartCoroutine(HideToolbox(toolbox, toolboxY, true));
            }
        }
#endif
    }

    /// <summary> Is the mouse hover GUI ? </summary>
    public bool IsHoverGUI()
    {
        foreach (Transform go in transform.GetChild(0))
        {
            if (go.GetComponent<RectTransform>().IsOver(Input.mousePosition) & go.gameObject.activeInHierarchy) return true;
        }
        foreach (Transform go in transform.GetChild(0).GetChild(2))
        {
            if (go.GetComponent<RectTransform>().IsOver(Input.mousePosition) & go.gameObject.activeInHierarchy) return true;
        }
        return false;
    }

    /// <summary> Switch on/off the Toolbox </summary>
    public void HideOrShowToolbox()
    {
        Transform toolbox = Toolbox.transform.parent;
        float toolboxY = toolbox.GetComponent<RectTransform>().sizeDelta.y * transform.GetChild(0).GetComponent<Canvas>().scaleFactor;
        bool hide = toolbox.position.y >= 0;
        StartCoroutine(HideToolbox(toolbox, toolboxY, hide));
        Toolbox.transform.parent.GetChild(4).GetComponent<UImage_Reader>().SetID(hide ? "native/GUI/editor/toolboxShow" : "native/GUI/editor/toolboxHide").Load();
    }

    /// <summary> Toolbox hide/show animation </summary>
    /// <param name="toolbox">The toolbox transform</param>
    /// <param name="Y">The height of the toolbox</param>
    /// <param name="hide">Should the toolbox to be hidden ?</param>
    IEnumerator HideToolbox(Transform toolbox, float Y, bool hide)
    {
        Vector2 oldPos = toolbox.position;
        yield return new WaitForEndOfFrame();
        if (oldPos == (Vector2)toolbox.position)
        {
            float pos = hide ? Y * -0.5F : Y * 0.5F;
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            bool LastFrame = true;
            System.TimeSpan totalTime = System.TimeSpan.FromMilliseconds(250);
            while (sw.Elapsed < totalTime | LastFrame)
            {
                float Time = sw.ElapsedMilliseconds;
                if (sw.Elapsed >= totalTime)
                {
                    LastFrame = false;
                    Time = (long)totalTime.TotalMilliseconds;
                }
                float totalDist = pos < 0 ? pos * -2 : pos * 2;
                float doneDist = hide ? (toolbox.position.y - totalDist / 2F) * -1 : toolbox.position.y + totalDist / 2F;
                float wantedDist = totalDist / (float)totalTime.TotalMilliseconds * Time;
                float mvt = wantedDist - doneDist;
                toolbox.position += hide ? new Vector3(0, -mvt) : new Vector3(0, mvt);
                yield return new WaitForEndOfFrame();
            }
            sw.Stop();
        }
    }

    /// <summary> The periode of time between each save </summary>
    public enum SaveMethode
    {
        Everytime,
        EveryChange,
        EveryMinute,
        EveryFiveMinutes
    }
    private SaveMethode _saveMethode;
    /// <summary> The periode of time between each save </summary>
    public SaveMethode saveMethode
    {
        get { return _saveMethode; }
        set { _saveMethode = value; if (!AutoSaveLevelEnabled) StartCoroutine(AutoSaveLevel()); }
    }
    bool AutoSaveLevelEnabled = false;
    /// <summary> Save the level each time the periode is done </summary>
    IEnumerator AutoSaveLevel()
    {
        AutoSaveLevelEnabled = true;

        if (_saveMethode == SaveMethode.Everytime) yield return new WaitForFixedUpdate();
        else if (_saveMethode == SaveMethode.EveryChange)
        {
            level.CopyTo(out Level.Infos oldComponent);
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
    /// <summary> Save the Level </summary>
    public void SaveLevel()
    {
        Logging.Log("Start saving...", LogType.Log);
        if (file != "" & level != null)
            File.WriteAllText(file, level.ToString());
        Logging.Log("Saving completed !", LogType.Log);
    }

    /// <summary> Get the position (in block number) in the world of a position on the screen </summary>
    /// <param name="pos">The position on the screen</param>
    /// <param name="round">The position should be rounded to the nearest block?</param>
    public Vector2 GetWorldPosition(Vector2 pos, bool round = true) { return GetWorldPosition(pos, round, cam.transform.position); }
    /// <summary> Get the position (in block number) in the world of a position on the screen </summary>
    /// <param name="pos">The position on the screen</param>
    /// <param name="round">The position should be rounded to the nearest block?</param>
    /// <param name="camPos">The position of the camera in the world</param>
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

    /// <summary> Switch on/off the Multiselection (only for Mobile)</summary>
    /// <param name="btn">The button</param>
    public void ChangeMultiSelect(Image btn)
    {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        MultiSelect = !MultiSelect;
        if(MultiSelect) btn.color = new Color32(75, 75, 75, 255);
        else btn.color = new Color32(125, 125, 125, 255);
#endif
    }

    /// <summary> Deselect all selected blocks </summary>
    public void DeselectAll()
    {
        foreach (int i in SelectedBlock)
        {
            Transform obj = transform.GetChild(1).Find("Objet n° " + i);
            if (obj != null) obj.transform.GetChild(0).gameObject.SetActive(false);
        }
        SelectedBlock = new int[0];
    }

    /// <summary> Zoom in </summary>
    /// <param name="Z">the sensibility</param>
    void Zoom(int Z = 0)
    {
        if (Z == 0) Z = ZoomSensitive;

        if (cam.orthographicSize > 240) cam.orthographicSize = cam.orthographicSize - Z;
        StartCoroutine(ZoomIndicator());
        Grille(true);
    }
    /// <summary> Zoom out </summary>
    /// <param name="Z">the sensibility</param>
    void Dezoom(int Z = 0)
    {
        if (Z == 0) Z = ZoomSensitive;

        if (cam.orthographicSize < 1200) cam.orthographicSize = cam.orthographicSize + Z;
        StartCoroutine(ZoomIndicator());
        Grille(true);
    }
    /// <summary> Show the Zoom Indicator during 1s </summary>
    IEnumerator ZoomIndicator()
    {
        zoomIndicator.gameObject.SetActive(true);
        zoomIndicator.value = (cam.orthographicSize - 240) / 960 * -1 + 1;
        yield return new WaitForSeconds(1F);

        bool Ctrl = Input.GetKey(KeyCode.LeftControl) | Input.GetKey(KeyCode.RightControl);
        if (Input.GetAxis("Mouse ScrollWheel") == 0 & !Ctrl)
            zoomIndicator.gameObject.SetActive(false);
    }

    /// <summary> Switch on/off the Grid </summary>
    /// <param name="Img">The button's image component</param>
    public void GrilleOnOff(UImage_Reader Img)
    {
        Transform _Grille = transform.GetChild(1).GetChild(1);
        GrilleOnOff(!_Grille.GetComponent<SpriteRenderer>().enabled, Img);
    }
    /// <summary> Switch on/off the Grid </summary>
    /// <param name="on">Switch on ?</param>
    /// <param name="Img">The button's image component</param>
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
    /// <summary> Manage the Grid </summary>
    /// <param name="needRespawn">The Grid needs to be respawn ?</param>
    /// <param name="del"></param>
    void Grille(bool needRespawn, bool del = false)
    {
        if (!ConfigAPI.GetBool("editor.Grid") & needRespawn) return; //Can't create a grid if the user doesn't want it ...
        Transform _Grille = transform.GetChild(1).GetChild(1);

        if (needRespawn)
        {
            _Grille.GetComponent<UImage_Reader>().Load();
            _Grille.GetComponent<SpriteRenderer>().enabled = true;
            _Grille.localScale = new Vector2(100, 100) / _Grille.GetComponent<UImage_Reader>().FrameSize * 50; //One grid block size

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

    /// <summary> Add a number to the number of the selected layer </summary>
    /// <param name="operation">The number to add</param>
    public void ChangeDisplayedLayer(int operation)
    {
        InputField input = transform.GetChild(0).GetChild(2).GetChild(3).GetChild(1).GetComponent<InputField>();
        if (int.TryParse(input.text, out int l))
        {
            l = l + operation;
            if (l > -2) input.text = l.ToString();
            else input.text = LangueAPI.Get("native", "editor.layer.all", "All");
            ChangeDisplayedLayer(input);
        }
        else if (operation > 0)
        {
            input.text = "-1";
            ChangeDisplayedLayer(input);
        }
    }
    /// <summary> Selects the layer whose number has been entered </summary>
    /// <param name="input">The input field</param>
    public void ChangeDisplayedLayer(InputField input)
    {
        if (!int.TryParse(input.text, out int l) | l < -2) input.text = LangueAPI.Get("native", "editor.layer.all", "All");
        else if (l > 999) input.text = "999";
        ChangeDisplayedLayer(input.text);
    }
    /// <summary> Selects the layer entered </summary>
    /// <param name="layer">The layer</param>
    public void ChangeDisplayedLayer(string layer)
    {
        Transform trans = transform.GetChild(0).GetChild(2).GetChild(3);

        if (!int.TryParse(layer, out int l) | l < -2) l = -2;
        else if (l > 999) l = 999;

        trans.GetChild(0).GetComponent<Button>().interactable = l > -2;
        trans.GetChild(2).GetComponent<Button>().interactable = l < 999;

        selectedLayer = l;
        float opacity = 0.2F;
        if (!string.IsNullOrEmpty(ConfigAPI.GetString("editor.layerOpacity"))) opacity = ConfigAPI.GetFloat("editor.layerOpacity");
        for (int i = 2; i < transform.GetChild(1).childCount; i++)
        {
            SpriteRenderer renderer = transform.GetChild(1).GetChild(i).GetComponent<SpriteRenderer>();
            Color color = ColorExtensions.ParseHex(GetBlocStatus("Color", i - 2));
            if (renderer.sortingOrder != l & l != -2) color.a = color.a * opacity;
            renderer.color = color;
        }
    }

    #region GestionBloc
    /// <summary> Create a block </summary>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
    /// <param name="_Color">Color of the block</param>
    void CreateBlock(int x, int y, Color32 _Color)
    {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        if (Input.touchCount == 1)
        {
#endif
        if (newblockid < 0) return;
        int l = selectedLayer;
        if (l == -2) l = 0;
        Vector3 a = new Vector3(x, y, l);

        float _id = newblockid;
        Level.Block.Type _type = Level.Block.Type.Block;
        if (newblockid > 10000) { _id = (newblockid - 10000F) / 10F; _type = Level.Block.Type.Event; } //Compatibility Mode
        else if (newblockid == 0) _type = Level.Block.Type.Event;

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

    /// <summary>Selects a block</summary>
    /// <param name="_id">ID of the block</param>
    public void SelectBlock(Level.Block.Type type, string blockID = "")
    {
        BulleDeveloppementCat.SetActive(false);
        Transform Content = Toolbox.GO[3].GetComponent<ScrollRect>().content;

        if (!float.TryParse(blockID, out newblockid)) Debug.LogError("Unkown id: " + blockID);
        string rootID = type == Level.Block.Type.Event ? "native/GUI/editor/build/events/" : "native/BLOCKS/";
        Content.GetChild((int)newblockid).GetChild(0).GetComponent<UImage_Reader>().SetID(rootID + newblockid.ToString("0.0####")).Load();

        for (int i = 0; i < Content.childCount; i++)
        {
            string id = i == (int)newblockid ? "native/GUI/editor/build/buttonSelected" : "native/GUI/editor/build/button";
            Content.GetChild(i).GetComponent<UImage_Reader>().SetID(id).Load();
        }
    }

    /// <summary>Opens a selection bubble for the category</summary>
    /// <param name="id">The id of the category</param>
    public void OpenCat(int id)
    {
        if (id == (int)newblockid) //Deselect
        {
            newblockid = -1;
            if (id >= 0) Toolbox.GO[3].GetComponent<ScrollRect>().content.GetChild(id).GetComponent<UImage_Reader>().SetID("native/GUI/editor/build/button").Load();
        }
        else //Select
        {
            string path = Application.persistentDataPath + "/Ressources/default/textures/"; //The path to the default RP
            string[] files = new string[0];

            //Where to look for blocks ?
            Dictionary<Level.Block.Type, string[]> rootIDs = new Dictionary<Level.Block.Type, string[]>(){
                { Level.Block.Type.Event, new string[] { "native/GUI/editor/build/events/" } },
                { Level.Block.Type.Block, new string[] { "native/BLOCKS/" } }
            };

            Level.Block.Type blockType = id < 1 ? Level.Block.Type.Event : Level.Block.Type.Block;
            foreach (string rootID in rootIDs[blockType])
            {
                string[] result = Directory.GetFiles(path + rootID, id + "*", SearchOption.AllDirectories);
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = result[i].Substring(path.Length); //Remove the path part
                    int index = result[i].IndexOf(" ");
                    if (index > 0) result[i] = result[i].Substring(0, index);
                }
                files = files.Union(result).ToArray();
            }

            if (files.Length == 1) SelectBlock(blockType, Path.GetFileName(files[0])); //Select the block
            else if (files.Length > 1) //Show the bubble
            {
                Vector2 pos = Toolbox.GO[3].GetComponent<ScrollRect>().content.GetChild(id).localPosition;

                BulleDeveloppementCat.GetComponent<UImage_Reader>().Load();
                Vector2 sizeObject = new Vector2(100, 80);
                Vector2 sizeImage = BulleDeveloppementCat.GetComponent<Image>().sprite.Size();

                Vector2 sizeRelative = sizeObject / sizeImage;
                if (sizeRelative.x < sizeRelative.y) sizeRelative.y = sizeRelative.x;
                else if (sizeRelative.x > sizeRelative.y) sizeRelative.x = sizeRelative.y;

                BulleDeveloppementCat.transform.localScale = sizeRelative;
                BulleDeveloppementCat.GetComponent<RectTransform>().sizeDelta = sizeObject / sizeRelative;

                Vector4 border = BulleDeveloppementCat.GetComponent<Image>().sprite.border;
                BulleDeveloppementCat.GetComponent<HorizontalLayoutGroup>().padding = new RectOffset((int)border.z, (int)border.z, (int)border.w, (int)border.y);

                pos.x = pos.x + 20;
                BulleDeveloppementCat.transform.GetChild(0).localScale = new Vector2(1, 1) / sizeRelative;
                if (id / 2F == id / 2)
                {
                    pos.y = pos.y - 80;
                    BulleDeveloppementCat.transform.rotation = QuaternionExtensions.SetEuler(0, 0, 0);
                    BulleDeveloppementCat.transform.GetChild(0).localPosition = new Vector3(0, -8.75F, 0);
                }
                else
                {
                    pos.y = pos.y + 80;
                    BulleDeveloppementCat.transform.rotation = QuaternionExtensions.SetEuler(180, 0, 0);
                    BulleDeveloppementCat.transform.GetChild(0).localPosition = new Vector3(0, 8.75F, 0);
                }

                if (!BulleDeveloppementCat.activeInHierarchy)
                {
                    for (int i = 1; i < BulleDeveloppementCat.transform.childCount; i++) Destroy(BulleDeveloppementCat.transform.GetChild(i).gameObject);
                    BulleDeveloppementCat.transform.GetChild(0).gameObject.SetActive(false);

                    BulleDeveloppementCat.GetComponent<RectTransform>().sizeDelta = (sizeObject / sizeRelative) * new Vector2(files.Length, 1);
                    pos.x = pos.x + (sizeObject.x * (files.Length - 1) / 2F);

                    Toolbox.GO[3].GetComponent<Button>().onClick.RemoveAllListeners();
                    Toolbox.GO[3].GetComponent<Button>().onClick.AddListener(() =>
                    {
                        for (int i = 1; i < BulleDeveloppementCat.transform.childCount; i++) Destroy(BulleDeveloppementCat.transform.GetChild(i).gameObject);
                        BulleDeveloppementCat.SetActive(false);
                    });

                    for (int i = 0; i < files.Length; i++)
                    {
                        GameObject newRef = Instantiate(BulleDeveloppementCat.transform.GetChild(0).gameObject, BulleDeveloppementCat.transform);
                        newRef.SetActive(true);
                        newRef.name = Path.GetFileName(files[i]);
                        newRef.transform.localPosition = new Vector3(i * 80, 0, 0);
                        newRef.transform.rotation = QuaternionExtensions.SetEuler(BulleDeveloppementCat.transform.rotation.eulerAngles.x, 0, 0);
                        newRef.transform.GetComponent<Button>().onClick.AddListener(() => SelectBlock(blockType, newRef.name));

                        newRef.transform.GetComponent<UImage_Reader>().SetID(files[i]).Load();
                    }
                    BulleDeveloppementCat.SetActive(true);
                    BulleDeveloppementCat.GetComponent<RectTransform>().anchoredPosition = pos;
                }
                else
                {
                    for (int i = 1; i < BulleDeveloppementCat.transform.childCount; i++) Destroy(BulleDeveloppementCat.transform.GetChild(i).gameObject);
                    BulleDeveloppementCat.SetActive(false);
                }
            }
        }
    }

    /// <summary>Instanciate a block</summary>
    /// <param name="num">The index of the block</param>
    /// <param name="keep">Just refresh the block or respawn it ?</param>
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
            GameObject go;
            if (keep)
            {
                go = GameObject.Find("Objet n° " + num);
                go.transform.position = pos;
                go.transform.rotation = rot;
            }
            else go = Instantiate(Pref, pos, rot, transform.GetChild(1));

            go.name = "Objet n° " + num;

            UImage_Reader UImage = go.GetComponent<UImage_Reader>();
            UImage.SetID(level.blocks[num].category + (id < 1 ? "/GUI/editor/build/events/" : "/BLOCKS/") + id.ToString("0.0####")).Load();

            Vector2 frameSize = UImage.FrameSize;
            if (frameSize == Vector2.zero) frameSize = new Vector2(100, 100);
            go.transform.localScale = new Vector2(100, 100) / frameSize * 50;
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Texture2D SelectedZoneSize = go.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite.texture;
                go.transform.GetChild(i).localScale = new Vector2(frameSize.x / SelectedZoneSize.width, frameSize.y / SelectedZoneSize.height);
            }

            SpriteRenderer SR = go.GetComponent<SpriteRenderer>();
            SR.color = ColorExtensions.ParseHex(GetBlocStatus("Color", num));
            SR.sortingOrder = (int)level.blocks[num].position.z;
        }
    }

    /// <summary>The coordinates of a block</summary>
    /// <param name="num">The index of the block</param>
    public Vector3 GetObjectPos(int num)
    {
        Vector2 pos = level.blocks[num].position * 50 + new Vector3(25, 25, 0);
        return new Vector3(pos.x, pos.y, level.blocks[num].position.z);
    }
    #endregion

    /// <summary>Seach a block at the coordinates</summary>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
    /// <returns>The index of the block (-1 if null)</returns>
    public int GetBloc(int x, int y)
    {
        for (int i = 0; i < level.blocks.Length; i++)
        {
            if ((Vector2)level.blocks[i].position == new Vector2(x, y)
                & (level.blocks[i].position.z == selectedLayer | selectedLayer == -2))
                return i;
        }
        return -1;
    }

    /// <summary>Change Block Parameter</summary>
    /// <param name="StatusID">Parameter ID</param>
    /// <param name="_component">Parameter Value</param>
    /// <param name="Bloc">Blocks conserned</param>
    public void ChangBlocStatus(string StatusID, string _component, int[] Bloc) { ChangBlocStatus(new string[] { StatusID }, new string[] { _component }, Bloc); }
    /// <summary>Change Blocks Parameter</summary>
    /// <param name="StatusID">Parameters IDs</param>
    /// <param name="_component">Parameters Values</param>
    /// <param name="Bloc">Blocks conserned</param>
    public void ChangBlocStatus(string[] StatusID, string[] _component, int[] Bloc)
    {
        if (Bloc == null) return;
        else if (Bloc.Length == 0) return;
        else
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


    /// <summary>Get Block Parameter</summary>
    /// <param name="StatusID">Parameter ID</param>
    /// <param name="Bloc">Block conserned</param>
    /// <returns></returns>
    public string GetBlocStatus(string StatusID, int Bloc)
    {
        if (file != "" & level.blocks.Length > Bloc & Bloc >= 0)
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

    /// <summary>Delete the selected block</summary>
    /// <param name="fromUpdateScript">Shouldn't activate the GUI ?</param>
    public void DeleteSelectedBloc(bool fromUpdateScript)
    {
        SelectedBlock = SelectedBlock.OrderBy(i => i).ToArray();
        for (int v = 0; v < SelectedBlock.Length; v++)
        {
            if (SelectedBlock[v] != -1)
            {
                int SB = SelectedBlock[v] - v;
                SelectedBlock[v] = -1;

                Transform obj = transform.GetChild(1);
                if (obj.childCount > SB + 2 + v)
                {
                    obj = obj.GetChild(SB + 2 + v);
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
                    StartCoroutine(WaitForDisableNoBlocSelectedPanel(2F));
                }
            }
        }
    }
    public IEnumerator WaitForDisableNoBlocSelectedPanel(float sec)
    {
        yield return new WaitForSeconds(sec);
        NoBlocSelectedPanel.SetActive(false);
    }

    /// <summary>Move the camera</summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void Deplacer(float x, float y)
    {
        if (x != 0 | y != 0)
        {
            float CamX = cam.transform.position.x;
            float CamY = cam.transform.position.y;

            if (x != 0) CamX = cam.transform.position.x + x;
            if (y != 0) CamY = cam.transform.position.y + y;

            cam.transform.position = new Vector3(CamX, CamY, -10);
            Grille(false);
        }
    }

    /// <summary>Play the level</summary>
    public void PlayLevel()
    {
        SaveLevel();
        string[] args = new string[] { "Editor", "File", file };
        string[] passThrough = new string[] { "Player", "Edit", file };
        GameObject.Find("LoadingScreen").GetComponent<LoadingScreenControl>().LoadScreen("Player", args.Concat(passThrough).ToArray(), true);
    }

    //Inspector only
    public void SelectModeChang(bool enable) { SelectMode = enable; }
    public void BloqueActions(bool on) { canInteract = !on; }
    public void OnEchap() { if (!string.IsNullOrEmpty(file) & !bloqueEchap) { ExitEdit(); cam.GetComponent<BaseControl>().returnScene = true; } }
}
