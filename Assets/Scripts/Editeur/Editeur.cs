using Boo.Lang;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Editeur : MonoBehaviour
{

    public GameObject Selection;

    public string file;
    public string[] component;

    public bool AddBlocking;
    public float newblockid;
    public GameObject Prefab;

    public bool SelectBlocking;
    public int[] SelectedBlock;
    public GameObject NoBlocSelectedPanel;
    public GameObject[] Contenu;
    GameObject SelectedZone;
    public GameObject SelectedZonePref;
    public Scrollbar zoomIndicator;

    Camera cam;
    public int ZoomSensitive = 20;

    public bool SelectMode = false;
    public bool bloqueSelect = false;

    public GameObject BulleDeveloppementCat;
    public Sprite[] BulleDeveloppementCatSp;

    public Sprite GrilleSp;
    public Sprite[] GrilleBtn;

    public bool bloqueEchap;

#if UNITY_STANDALONE || UNITY_EDITOR
    public int CameraMouvementSpeed = 10;
#endif

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
    bool MultiSelect = false;
    Vector2 touchLastPosition;
#endif

    #region UI
    public void CreateFile(string fileName, string directory, string desc)
    {
        string txt = directory + fileName + ".level";
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        File.CreateText(txt).Close();
        file = txt;
        Selection.SetActive(false);
        gameObject.SetActive(true);

        System.DateTime createTime = System.DateTime.UtcNow;
        File.SetCreationTimeUtc(file, createTime);
        component = new string[] { "description = " + desc,
            "background = 1; 4b4b4b255",
            "music = ",
            "version = " +Application.version,
            "author = " +ConfigAPI.GetString("Account.Username"),
            "//Please don't touch the publicID",
            "publicID = " + SHA_PublicID(createTime, ConfigAPI.GetString("Account.Username")),
            "respawnMode = 0",
            " ",
            "Blocks {",
            "}"};
        transform.GetChild(0).gameObject.SetActive(true);

        transform.GetChild(0).GetChild(2).GetChild(1).GetChild(0).GetChild(1).GetComponent<Background>().ActualiseFond(this);
        OpenCat(-1);

        Grille(false, true);
        GrilleOnOff(ConfigAPI.GetBool("editor.Grid"), transform.GetChild(0).GetChild(5).GetComponent<Image>());


        string[] dirToPath = file.Split(new string[2] { "/", "\\" }, System.StringSplitOptions.None);
        Discord.Presence(LangueAPI.String("discordEditor_title"), LangueAPI.StringWithArgument("discordEditor_subtitle", dirToPath[dirToPath.Length - 1].Replace(".level", "")), new DiscordClasses.Img("default"));
        cam.GetComponent<BaseControl>().returnScene = false;
    }

    public void EditFile(string txt)
    {
        file = txt;
        Selection.SetActive(false);
        gameObject.SetActive(true);

        component = File.ReadAllLines(txt);

        //if (!CheckPublicID(txt)) Debug.LogError("The Public ID is invalid");
        UpdateLevel(component);

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
        for (int i = component.Length - 1; i >= d; i--)
        {
            if (component[i] == "}" & end == -1)
            {
                end = i;
                i = component.Length;
            }
        }

        for (int i = d; i < end; i++)
            Instance(i);
        transform.GetChild(0).gameObject.SetActive(true);

        transform.GetChild(0).GetChild(2).GetChild(1).GetChild(0).GetChild(1).GetComponent<Background>().ActualiseFond(this);
        OpenCat(-1);

        Grille(false, true);
        GrilleOnOff(ConfigAPI.GetBool("editor.Grid"), transform.GetChild(0).GetChild(5).GetComponent<Image>());


        string[] dirToPath = file.Split(new string[2] { "/", "\\" }, System.StringSplitOptions.None);
        Discord.Presence(LangueAPI.String("discordEditor_title"), LangueAPI.StringWithArgument("discordEditor_subtitle", dirToPath[dirToPath.Length - 1].Replace(".level", "")), new DiscordClasses.Img("default"));
        cam.GetComponent<BaseControl>().returnScene = false;
    }

    public void ExitEdit()
    {
        if (GameObject.Find("Audio") != null)
        {
            GameObject.Find("Audio").GetComponent<menuMusic>().StartDefault();
        }
        Selection.SetActive(true);
        Selection.GetComponent<EditorSelect>().NewStart();
        Destroy(transform.GetChild(1).gameObject);
        GameObject Object = new GameObject("Objects");
        Object.transform.SetParent(transform, false);
        SelectedZone = Instantiate(SelectedZonePref, Object.transform);
        SelectedZone.name = "Selected Block";
        SelectedZone.SetActive(false);
        SelectedBlock = new int[0];
        SelectMode = false;
        component = new string[0];
        transform.GetChild(0).GetChild(2).GetChild(1).GetComponent<CreatorManager>().array = 3;
        newblockid = -1;
        gameObject.SetActive(false);
        Discord.Presence(LangueAPI.String("discordEditor_title"), "", new DiscordClasses.Img("default"));
        cam.GetComponent<BaseControl>().returnScene = true;
    }
    #endregion

    public static string SHA_PublicID(System.DateTime Date, string User)
    {
        string input = Date.ToString("yyyyMMddHHmmss") + "_" + User;
        return Security.Hashing.SHA1(input);
    }
    public static bool CheckPublicID(string txt)
    {
        string[] f = File.ReadAllLines(txt);

        int l = -1;
        for (int x = 0; x < f.Length; x++)
        {
            if (f[x].Contains("publicID = ") & l == -1)
            {
                l = x;
                x = f.Length;
            }
        }
        int u = -1;
        for (int x = 0; x < f.Length; x++)
        {
            if (f[x].Contains("author = ") & u == -1)
            {
                u = x;
                x = f.Length;
            }
        }
        if (l != -1) return f[l].Replace("publicID = ", "") == SHA_PublicID(File.GetCreationTimeUtc(txt), f[u].Replace("author = ", ""));
        else return false;
    }

    private void Start()
    {
        Discord.Presence(LangueAPI.String("discordEditor_title"), "", new DiscordClasses.Img("default"));
        cam = Selection.GetComponent<EditorSelect>().Cam.GetComponent<Camera>();
        zoomIndicator.gameObject.SetActive(false);
        BulleDeveloppementCat.SetActive(false);

        if (File.Exists(Application.temporaryCachePath + "/play.txt"))
        {
            if (File.ReadAllLines(Application.temporaryCachePath + "/play.txt").Length > 0)
            {
                file = File.ReadAllLines(Application.temporaryCachePath + "/play.txt")[0];
                File.WriteAllText(Application.temporaryCachePath + "/play.txt", "");
                EditFile(file);
                Selection.GetComponent<EditorSelect>().SoundBoard.RefreshList();
            }
            else Selection.SetActive(true);
        }
        else Selection.SetActive(true);

        transform.GetChild(0).GetChild(2).GetChild(1).GetChild(0).GetChild(1).GetComponent<Background>().Charg();
    }

    void Update()
    {
        GetComponent<SurClique>().enabled = SelectMode;

        if (SelectedZone == null)
        {
            SelectedZone = GameObject.Find("Selected Block");
            if (SelectedZone == null)
            {
                SelectedZone = Instantiate(SelectedZonePref, transform.GetChild(1));
                SelectedZone.name = "Selected Block";
            }
        }

        Color32 backColor = new Color32(32, 32, 32, 255);
        if (SelectMode)
            backColor = new Color32(70, 70, 70, 255);
        transform.GetChild(0).GetChild(2).GetComponent<Image>().color = backColor;

        //Sauvegarde Automatique
        if (file != "" & component.Length != 0)
        {
            try { File.WriteAllLines(file, component); } catch { }
        }

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
                        Vector2 pos = GetClicPos();

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
                Vector2 pos = GetClicPos();

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
                    float blockId = float.Parse(GetBlocStatus("ID", Selected));
                    float firstId = -1;
                    if (SelectedBlock.Length > 0) firstId = float.Parse(GetBlocStatus("ID", SelectedBlock[0]));

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
                Vector2 pos = GetClicPos();
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
        else SelectedZone.SetActive(false);


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
                
        Vector2 TouchPosition = new Vector2(touchZero.position.x, touchZero.position.y);
                if(touchLastPosition == new Vector2(-50000, -50000))
                    touchLastPosition = TouchPosition;
                Vector2 touchZeroPrevPos = TouchPosition - touchLastPosition; // Find the diference between the last position.
                Deplacer(touchZeroPrevPos.x * Speed, touchZeroPrevPos.y * Speed);
                touchLastPosition = TouchPosition;
            }
        }
        else touchLastPosition = new Vector2(-50000, -50000);
#endif
    }

    public Vector2 GetClicPos()
    {
        float zoomRelatively = (Screen.height / 2) / cam.orthographicSize;
        float zoom = Screen.height / cam.orthographicSize;

        float camMoveX = cam.transform.position.x - (Screen.width / zoom);
        float camMoveY = cam.transform.position.y - cam.orthographicSize;

        float PosDoigtX = (camMoveX - (25 * zoomRelatively)) / 50;
        float PosDoigtY = (camMoveY - (25 * zoomRelatively)) / 50;

        float PosDoigtNoCamPosX = (Input.mousePosition.x - (25 * zoomRelatively)) / 50;
        float PosDoigtNoCamPosY = (Input.mousePosition.y - (25 * zoomRelatively)) / 50;

        float IndiceChangPosCamX = (Screen.width / zoom) / (Screen.width / 2);
        float IndiceChangPosCamY = cam.orthographicSize / (Screen.height / 2);

        int x = Mathf.RoundToInt(PosDoigtNoCamPosX * IndiceChangPosCamX) + (int)PosDoigtX;
        int y = Mathf.RoundToInt(PosDoigtNoCamPosY * IndiceChangPosCamY) + (int)PosDoigtY;
        return new Vector2(x, y);
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
    public void GrilleOnOff(Image Img)
    {
        Transform BD = GameObject.Find("BackgroundDiv").transform;
        Transform _Grille = BD.GetChild(BD.childCount - 1);
        GrilleOnOff(!(_Grille.childCount > 0), Img);
    }
    void GrilleOnOff(bool on, Image Img)
    {
        ConfigAPI.SetBool("editor.Grid", on);
        if (on) //Grid ON
        {
            Img.sprite = GrilleBtn[0];
            Grille(true, false);
        }
        else //Grid OFF
        {
            Img.sprite = GrilleBtn[1];
            Grille(false, true);
        }
    }
    void Grille(bool needRespawn, bool del = false)
    {
        if (!ConfigAPI.GetBool("editor.Grid") & needRespawn) return;
        Transform BD = GameObject.Find("BackgroundDiv").transform;
        Transform _Grille = BD.GetChild(BD.childCount - 1);

        Vector3 GrillePos = new Vector2((int)(cam.transform.position.x / 50) * 50, (int)(cam.transform.position.y / 50) * 50);
        _Grille.localPosition = new Vector2((cam.transform.position.x - GrillePos.x) * -1, (cam.transform.position.y - GrillePos.y) * -1);

        if (needRespawn | del)
        {
            for (int i = 0; i < _Grille.childCount; i++)
                Destroy(_Grille.GetChild(i).gameObject);
            if (del) return;

            float zoomRelatively = (Screen.height / 2) / cam.orthographicSize;
            float zoom = Screen.height / cam.orthographicSize;

            float PosDoigtNoCamPosX = (Screen.width - (25 * zoomRelatively)) / 50;
            float PosDoigtNoCamPosY = (Screen.height - (25 * zoomRelatively)) / 50;

            float IndiceChangPosCamX = (Screen.width / zoom) / (Screen.width / 2);
            float IndiceChangPosCamY = cam.orthographicSize / (Screen.height / 2);

            Vector2 GrilleCarre = new Vector2(Mathf.RoundToInt(PosDoigtNoCamPosX * IndiceChangPosCamX) + 2,
                Mathf.RoundToInt(PosDoigtNoCamPosY * IndiceChangPosCamY) + 2);
            int GrilleCarreNb = (int)GrilleCarre.x * (int)GrilleCarre.y;
            for (int i = 0; i < GrilleCarreNb; i++)
            {
                int y = (int)(i / GrilleCarre.x);
                int x = i - ((int)GrilleCarre.x * y);
                GameObject go = Instantiate(Prefab, new Vector2((x - 1) * 50 + 25, (y - 1) * 50 + 25), new Quaternion(), _Grille);
                go.GetComponent<SpriteRenderer>().sprite = GrilleSp;
                go.transform.localScale = new Vector2(50, 50); //ToDo : ScaleWithZoom
            }
        }
    }

    #region GestionBloc
    void CreateBloc(int x, int y, Color32 _Color)
    {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        if (Input.touchCount == 1)
        {
#endif
        if (newblockid <= 0)
            return;

        Vector3 a = new Vector3(x, y, 0);
        string color = ColorToHex(_Color);

        float id = newblockid;
        if (id > 10000)
            id = (newblockid - 10000F) / 10F;

        int start = -1;
        for (int i = 0; i < component.Length; i++)
        {
            if (component[i].Contains("Blocks {") & start == -1)
                start = i + 1;
        }
        int end = -1;
        for (int i = start; i < component.Length; i++)
        {
            if (component[i] == "}" & end == -1)
                end = i;
        }
        string[] newComponent = new string[component.Length + 1];
        for (int i = 0; i < newComponent.Length; i++)
        {
            if (i < end)
                newComponent[i] = component[i];
            else if (i == end)
            {
                if (id < 1 & id > 0) newComponent[i] = id.ToString("0.0####") + "; " + a + "; {}";
                else if (id >= 1) newComponent[i] = id.ToString("0.0####") + "; " + a + "; {Rotate:0; Color:" + color + "; Behavior:0}";
            }
            else newComponent[i] = component[i - 1];
        }

        bool t = true;
        for (int i = 0; i < component.Length; i++)
        {
            string blockI = "";
            string[] I = component[i].Split(new string[] { "; " }, System.StringSplitOptions.None);
            for (int v = 1; v < I.Length; v++)
            {
                if (v > 1) blockI = blockI + "; " + I[v];
                else blockI = I[v];
            }
            string blockNew = "";
            string[] New = newComponent[end].Split(new string[] { "; " }, System.StringSplitOptions.None);
            for (int v = 1; v < New.Length; v++)
            {
                if (v > 1) blockNew = blockNew + "; " + New[v];
                else blockNew = New[v];
            }

            if (blockI == blockNew)
                t = false;
        }
        if (t)
        {
            component = newComponent;
            Instance(end);
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

        BulleDeveloppementCat.SetActive(false);
        for (int i = 1; i < Contenu[3].transform.childCount - 2; i++)
        {
            if (i == (int)newblockid & AddBlocking)
            {
                Contenu[3].transform.GetChild(i).GetComponent<Image>().color = new Color32(70, 70, 70, 255);
                Texture2D tex = new Texture2D(1, 1);
                tex.LoadImage(File.ReadAllBytes(Application.persistentDataPath + "/Textures/0/" + newblockid.ToString("0.0####") + ".png"));
                Contenu[3].transform.GetChild(i).GetChild(0).GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
            }
            else
            {
                Contenu[3].transform.GetChild(i).GetComponent<Image>().color = new Color32(0, 0, 0, 255);
                Texture2D tex = new Texture2D(1, 1);
                tex.LoadImage(File.ReadAllBytes(Application.persistentDataPath + "/Textures/0/" + i.ToString("0.0####") + ".png"));
                Contenu[3].transform.GetChild(i).GetChild(0).GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
            }
        }
        for (int i = 1; i < Contenu[1].transform.childCount - 2; i++)
        {
            int v = (int)(newblockid - 10000F);
            if (i == v & AddBlocking)
                Contenu[1].transform.GetChild(i).GetComponent<Image>().color = new Color32(70, 70, 70, 255);
            else Contenu[1].transform.GetChild(i).GetComponent<Image>().color = new Color32(45, 45, 45, 255);
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

                string path = Application.persistentDataPath + "/Textures/0/";
                for (int i = 0; i < Directory.GetFiles(path, id + ".*", SearchOption.AllDirectories).Length; i++)
                {
                    GameObject newRef = Instantiate(BulleDeveloppementCat.transform.GetChild(0).gameObject, BulleDeveloppementCat.transform);
                    newRef.SetActive(true);
                    newRef.name = i.ToString();
                    newRef.transform.localPosition = new Vector3(i * 80, 0, 0);
                    newRef.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => AddBlock(id.ToString() + "." + newRef.name));

                    Texture2D tex = new Texture2D(1, 1);
                    tex.LoadImage(File.ReadAllBytes(Application.persistentDataPath + "/Textures/0/" + id + "." + i + ".png"));
                    newRef.transform.GetChild(0).GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
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
        float id = 0;
        try { id = float.Parse(GetBlocStatus("ID", num)); }
        catch { Debug.LogWarning("The block at the line " + num + " as an invalid id"); return; }
        Vector3 p = new Vector3();
        try { p = GetObjectPos(num); }
        catch { Debug.LogWarning("The block at the line " + num + " as an invalid position"); return; }
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
            SpriteRenderer SR = go.GetComponent<SpriteRenderer>();

            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(File.ReadAllBytes(Application.persistentDataPath + "/Textures/0/" + id.ToString("0.0####") + ".png"));
            SR.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));

            go.transform.localScale = new Vector2(100F / tex.width * 50, 100F / tex.height * 50);
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Texture2D SelectedZoneSize = go.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite.texture;
                go.transform.GetChild(i).localScale = new Vector2(tex.width / SelectedZoneSize.width, tex.height / SelectedZoneSize.height);
            }

            try { SR.color = HexToColor(GetBlocStatus("Color", num)); }
            catch
            {
                SR.color = new Color32(190, 190, 190, 255);
                Debug.LogWarning("The block at the line " + num + " as an invalid color");
            }
            SR.sortingOrder = (int)p.z;
        }
    }
    public Vector3 GetObjectPos(int num)
    {
        string a = GetBlocStatus("Position", num);
        string[] b = a.Replace("(", "").Replace(" ", "").Replace(")", "").Replace(".0", "").Split(new string[] { "," }, System.StringSplitOptions.None);
        float[] c = new float[] { float.Parse(b[0]) * 50 + 25, float.Parse(b[1]) * 50 + 25, float.Parse(b[2]) };
        return new Vector3(c[0], c[1], c[2]);
    }
    public static string ColorToHex(Color32 color)
    {
        string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2") + color.a.ToString();
        return hex;
    }
    public static Color HexToColor(string hex)
    {
        if (hex.Length < 6 | hex.Length > 9) return new Color32(190, 190, 190, 255);

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
        for (int i = 0; i < component.Length; i++)
        {
            if (component[i].Contains("(" + x + ".0, " + y + ".0, "))
                a = i;
        }
        return a;
    }


    [System.Obsolete("Don't include the block id is no longer supported, use ChangBlocStatus(string StatusID, string _component, int[] Bloc) instead")]
    public void ChangBlocStatus(float StatusID, string _component) { ChangBlocStatus(StatusID, _component, SelectedBlock); }

    [System.Obsolete("Don't include an blocks id array is no longer supported, use ChangBlocStatus(string StatusID, string _component, int[] Bloc) instead")]
    public void ChangBlocStatus(float StatusID, string _component, int Bloc = -1)
    {
        if (Bloc == -1) ChangBlocStatus(StatusID, _component, SelectedBlock);
        else ChangBlocStatus(StatusID, _component, new int[] { Bloc });
    }
    [System.Obsolete("Don't give a string id is no longer supported, use ChangBlocStatus(string StatusID, string _component, int[] Bloc) instead")]
    public void ChangBlocStatus(float StatusID, string _component, int[] Bloc = null)
    {
        string id = "";
        if (StatusID == 0) id = "ID";
        else if (StatusID == 1 | StatusID == 1.1) id = "Position";
        else if (StatusID == 1.11) id = "PositionX";
        else if (StatusID == 1.12) id = "PositionY";
        else if (StatusID == 1.2) id = "Layer";
        else if (StatusID == 2) id = "Rotate";
        else if (StatusID == 3) id = "Color";
        else if (StatusID == 4) id = "Behavior";
        ChangBlocStatus(id, _component, Bloc);
    }
    public void ChangBlocStatus(string StatusID, string _component, int[] Bloc)
    {
        if (Bloc != null)
        {
            for (int i = 0; i < Bloc.Length; i++)
            {
                if (Bloc[i] != -1)
                {
                    string[] a = component[Bloc[i]].Split(new string[] { "; " }, System.StringSplitOptions.None);
                    string[] Pos = a[1].Split(new string[] { ", " }, System.StringSplitOptions.None);
                    string[] b = component[Bloc[i]].Split(new string[] { "; {" }, System.StringSplitOptions.None)[1].Split(new string[] { "}" }, System.StringSplitOptions.None)[0].Split(new string[] { "; " }, System.StringSplitOptions.None);

                    string _Modified = "";

                    if (StatusID == "ID")
                        a[0] = _component;
                    else if (StatusID == "Position")
                        a[1] = "(" + _component + ", " + Pos[2];
                    else if (StatusID == "PositionX")
                        a[1] = "(" + _component + ", " + Pos[1] + ", " + Pos[2] + ")";
                    else if (StatusID == "PositionY")
                        a[1] = Pos[0] + ", " + _component + ", " + Pos[2] + ")";
                    else if (StatusID == "Layer")
                        a[1] = Pos[0] + ", " + Pos[1] + ", " + _component + ")";
                    else
                    {
                        int pNb = -1;
                        for (int p = 0; p < b.Length; p++)
                        {
                            string[] param = b[p].Split(new string[] { ":" }, System.StringSplitOptions.None);
                            if (param[0] == StatusID)
                                pNb = p;
                        }
                        if (pNb == -1)
                        {
                            if (b.Length > 0)
                                if (!string.IsNullOrEmpty(b[0])) b = b.Union(new string[] { StatusID + ":" + _component }).ToArray();
                                else b = new string[] { StatusID + ":" + _component };
                            else b = new string[] { StatusID + ":" + _component };
                        }
                        else
                        {
                            string[] param = b[pNb].Split(new string[] { ":" }, System.StringSplitOptions.None);
                            b[pNb] = param[0] + ":" + _component;
                        }
                    }

                    _Modified = a[0] + "; " + a[1];
                    for (int iB = 0; iB < b.Length; iB++)
                    {
                        if (iB == 0) _Modified = _Modified + "; {" + b[iB];
                        else if (iB < b.Length - 1) _Modified = _Modified + "; " + b[iB];
                        else _Modified = _Modified + "; " + b[iB] + "}";
                    }


                    component[Bloc[i]] = _Modified;
                    Instance(Bloc[i], true);
                }
            }
        }
    }

    [System.Obsolete("Don't include the block id is no longer supported, use GetBlocStatus(string StatusID, int Bloc) instead")]
    public string GetBlocStatus(float StatusID)
    {
        Debug.LogError("Don't include the block id is no longer supported, use GetBlocStatus(string StatusID, int Bloc) instead");
        return GetBlocStatus(StatusID, SelectedBlock[0]);
    }
    [System.Obsolete("Don't give a string id is no longer supported, use GetBlocStatus(string StatusID, int Bloc) instead")]
    public string GetBlocStatus(float StatusID, int Bloc)
    {
        string id = "";
        if (StatusID == 0) id = "ID";
        else if (StatusID == 1) id = "Position";
        else if (StatusID == 1.1) id = "PositionX";
        else if (StatusID == 1.2) id = "PositionY";
        else if (StatusID == 1.3) id = "Layer";
        else if (StatusID == 2) id = "Rotate";
        else if (StatusID == 3) id = "Color";
        else if (StatusID == 4) id = "Behavior";
        return GetBlocStatus(id, Bloc);
    }
    public string GetBlocStatus(string StatusID, int Bloc)
    {
        if (file != "" & component.Length > Bloc)
        {
            try
            {
                string[] a = component[Bloc].Split(new string[] { "; " }, System.StringSplitOptions.None);

                if (StatusID == "UID")
                    return null;
                else if (StatusID == "ID")
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

    public void DeleteSelectedBloc(bool fromUpdateScript)
    {
        for (int v = 0; v < SelectedBlock.Length; v++)
        {
            if (SelectedBlock[v] != -1)
            {
                int SB = SelectedBlock[v];
                SelectedBlock[v] = -1;
                string[] NewComponent = new string[component.Length - 1];

                Transform obj = transform.GetChild(1).Find("Objet n° " + SB);
                if (obj != null)
                    Destroy(obj.gameObject);

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

                        NewComponent[i] = component[i];
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

                        NewComponent[i] = component[i + 1];
                    }
                }
                component = NewComponent;
                File.WriteAllLines(file, component);
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

            if (x < 0)
            {
                if (cam.transform.position.x > Screen.width / 2)
                    CamX = cam.transform.position.x + x;
            }
            else if (x > 0)
                CamX = cam.transform.position.x + x;

            if (y < 0)
            {
                if (cam.transform.position.y > Screen.height / 2)
                    CamY = cam.transform.position.y + y;
            }
            else if (y > 0)
                CamY = cam.transform.position.y + y;

            cam.transform.position = new Vector3(CamX, CamY, -10);
            Grille(false);
        }
    }

    public void PlayLevel()
    {
        File.WriteAllLines(Application.temporaryCachePath + "/play.txt", new string[2] { file, "Editor" });
        GameObject.Find("LoadingScreen").GetComponent<LoadingScreenControl>().LoadScreen("Player");
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
    public static void UpdateLevel(string path)
    {
        string[] fileLines = File.ReadAllLines(path);
        File.WriteAllLines(path, UpdateLevel(fileLines));
    }

    /// <summary>
    /// Update a level to the newest version
    /// Warning : The level will be incompatible with older versions
    /// </summary>
    /// <param name="fileLines">File content line by line</param>
    public static string[] UpdateLevel(string[] fileLines)
    {
        string[] newFileLines = fileLines;

        int v = -1;
        for (int x = 0; x < newFileLines.Length; x++)
        {
            if (newFileLines[x].Contains("version = ") & v == -1)
            {
                v = x;
                x = newFileLines.Length;
            }
        }
        string version = "0.2";
        if (v != -1) version = newFileLines[v].Replace("version = ", "");

        if (version == "0.2")
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
                        if (float.Parse(parm[0]) >= 1) newFileLines[i] = parm[0] + "; " + parm[1] + "; {Rotate:" + parm[2] + "; Color:" + parm[3] + "; Behavior:" + parm[4] + "}";
                        else newFileLines[i] = parm[0] + "; " + parm[1] + "; {}";
                    }
                }
            }

            newFileLines[v] = "version = 0.2.1"; //Set new version number
        }

        return newFileLines;
    }
    #endregion
}
