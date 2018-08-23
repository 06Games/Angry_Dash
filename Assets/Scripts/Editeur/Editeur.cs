using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Editeur : MonoBehaviour
{

    public GameObject Selection;
    //public int ortho;

    public string file;
    public string[] component;

    public bool AddBlocking;
    public float newblockid;
    public GameObject Prefab;

    public bool SelectBlocking;
    public int SelectedBlock;
    public GameObject NoBlocSelectedPanel;
    public GameObject[] Contenu;
    GameObject SelectedZone;
    public GameObject SelectedZonePref;
    public Scrollbar zoomIndicator;

    Camera cam;
    public int ZoomSensitive = 20;

    public GameObject[] TriggerPrefabs;
    public bool SelectMode = false;
    public bool bloqueSelect = false;

    public GameObject BulleDeveloppementCat;
    public Sprite[] BulleDeveloppementCatSp;

    public Sprite GrilleSp;
    public Sprite[] GrilleBtn;

    public bool bloqueEchap;

#if UNITY_IOS || UNITY_ANDROID && !UNITY_EDITOR
    public int CameraMouvementSpeed = 1;
#else
    public int CameraMouvementSpeed = 10;
#endif

    #region UI
    public void CreateFile(string fileName, string directory, string desc)
    {
        string txt = directory + fileName + ".level";
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        File.CreateText(txt).Close();
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
            " ",
            "Blocks {",
            "}"};
        file = txt;
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

        if (!CheckPublicID(txt)) Debug.LogError("The Public ID is invalid");

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
        SelectedBlock = -1;
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
                l = x;
        }
        int u = -1;
        for (int x = 0; x < f.Length; x++)
        {
            if (f[x].Contains("author = ") & u == -1)
                u = x;
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
        SurClique.QuelClique SelectClique = SurClique.QuelClique.Droit;
        if (SelectMode)
            SelectClique = SurClique.QuelClique.Gauche;
        GetComponent<SurClique>().SurQuelClique = SelectClique;

        int BlocScale = Screen.height / (Screen.height / 50);

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
            KeyCode clic = KeyCode.Mouse0;
            if (SelectMode)
                clic = KeyCode.Mouse1;

            if (Input.GetKey(clic))
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
                SelectedBlock = GetBloc((int)pos.x, (int)pos.y);
            }
            SelectBlocking = false;
        }

        NoBlocSelectedPanel.SetActive(!Contenu[3].activeInHierarchy & !Contenu[0].activeInHierarchy & !Contenu[1].activeInHierarchy & !Contenu[4].activeInHierarchy & SelectedBlock == -1);
        if (SelectedBlock != -1)
        {
            SelectedZone.SetActive(true);

            try
            {
                string a = component[SelectedBlock].Split(new string[] { "; " }, System.StringSplitOptions.None)[1];
                string[] b = a.Replace("(", "").Replace(" ", "").Replace(")", "").Split(new string[] { "," }, System.StringSplitOptions.None);
                float[] c = new float[] { float.Parse(b[0]) * 50, float.Parse(b[1]) * 50, float.Parse(b[2]) };
                SelectedZone.transform.position = new Vector3(c[0] + (BlocScale / 2), c[1] + (BlocScale / 2));
                SelectedZone.transform.localScale = new Vector2(Screen.height / (Screen.height / 50) + 1, Screen.height / (Screen.height / 50) + 1);
            }
            catch { }
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

        if (Input.GetKey(KeyCode.RightArrow) & Ctrl)
            MoveX = 1;
        else if (Input.GetKey(KeyCode.LeftArrow) & Ctrl)
            MoveX = -1;

        if (Input.GetKey(KeyCode.UpArrow) & Ctrl)
            MoveY = 1;
        else if (Input.GetKey(KeyCode.DownArrow) & Ctrl)
            MoveY = -1;

        Deplacer(MoveX * Speed, MoveY * Speed);
#elif UNITY_ANDROID || UNITY_IOS
        bool isSimple = !AddBlocking & (Input.touchCount == 1 | Input.touchCount == 2);
        bool isAdvence = AddBlocking & (Input.touchCount == 2 | Input.touchCount == 3);
        if (isAdvence)
        {
            // Store both touches.
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            float screenCenterX = Screen.width * 0.5f;
            float screenCenterY = Screen.height * 0.5f;

            int MoveX = 0;
            int MoveY = 0;

            int Speed = CameraMouvementSpeed;
            if (Input.touchCount == 3)
                Speed = CameraMouvementSpeed * 2;

            if (touchZero.position.x > screenCenterX & touchOne.position.x > screenCenterX)
                MoveX = 1;
            else if (touchZero.position.x < screenCenterX & touchOne.position.x < screenCenterX)
                MoveX = -1;

            if (touchZero.position.y > screenCenterY & touchOne.position.y > screenCenterY)
                MoveY = 1;
            else if (touchZero.position.y < screenCenterY & touchOne.position.y < screenCenterY)
                MoveY = -1;

            Deplacer(MoveX * Speed, MoveY * Speed);
        }
        else if (isSimple)
        {
            Touch touchZero = Input.GetTouch(0);

            bool isInTop = touchZero.position.y > Screen.height - (Screen.height / 10);
            bool isInRightTop = touchZero.position.x > Screen.width - (Screen.width / 6);
            if (touchZero.position.y > Screen.height / 4 & !(isInTop & isInRightTop))
            {
                float screenCenterX = Screen.width * 0.5f;
                float screenCenterY = Screen.height * 0.5f;

                int MoveX = 0;
                int MoveY = 0;

                int Speed = CameraMouvementSpeed;
                if (Input.touchCount == 3)
                    Speed = CameraMouvementSpeed * 2;

                if (touchZero.position.x > screenCenterX)
                    MoveX = 1;
                else if (touchZero.position.x < screenCenterX)
                    MoveX = -1;

                if (touchZero.position.y > screenCenterY)
                    MoveY = 1;
                else if (touchZero.position.y < screenCenterY)
                    MoveY = -1;

                Deplacer(MoveX * Speed, MoveY * Speed);
            }
        }
#endif
    }

    Vector2 GetClicPos()
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
#if UNITY_ANDROID || UNITY_IOS
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
            if (component[i].Contains("}") & end == -1)
                end = i;
        }
        string[] newComponent = new string[component.Length + 1];
        for (int i = 0; i < newComponent.Length; i++)
        {
            if (i < end)
                newComponent[i] = component[i];
            else if (i == end)
                newComponent[i] = id.ToString("0.0####") + "; " + a + "; 0; " + color + "; 0";
            else newComponent[i] = component[i - 1];
        }

        bool t = true;
        for (int i = 0; i < component.Length; i++)
        {
            if (component[i] == newComponent[end])
                t = false;
        }
        if (t)
        {
            component = newComponent;
            Instance(end);
        }
#if UNITY_ANDROID || UNITY_IOS
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
        try { id = float.Parse(component[num].Split(new string[] { "; " }, System.StringSplitOptions.None)[0]); }
        catch { Debug.LogWarning("The block at the line " + num + " as an invalid id"); return; }
        Vector3 p = new Vector3();
        try { p = GetObjectPos(num); }
        catch { Debug.LogWarning("The block at the line " + num + " as an invalid position"); return; }
        Vector3 pos = new Vector3(p.x, p.y, 0);
        Quaternion rot = new Quaternion();
        try { rot.eulerAngles = new Vector3(0, 0, int.Parse(GetBlocStatus(2, num))); }
        catch { Debug.LogWarning("The block at the line " + num + " as an invalid rotation"); return; }
        GameObject Pref = null;

        try
        {
            if (id < 1)
                Pref = TriggerPrefabs[(int)(id * 10F) - 1];
            else Pref = Prefab;
        }
        catch { Debug.LogWarning("The block at the line " + num + " as an invalid id"); return; }

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

            try { SR.color = HexToColor(GetBlocStatus(3, num)); }
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
        string a = component[num].Split(new string[] { "; " }, System.StringSplitOptions.None)[1];
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
    int GetBloc(int x, int y)
    {
        int a = -1;
        for (int i = 0; i < component.Length; i++)
        {
            if (component[i].Contains("(" + x + ".0, " + y + ".0, "))
                a = i;
        }
        return a;
    }

    public void ChangBlocStatus(float StatusID, string _component, int Bloc = -1)
    {
        if (Bloc == -1)
            Bloc = SelectedBlock;

        if (Bloc != -1)
        {
            string[] b = component[Bloc].Split(new string[] { "; " }, System.StringSplitOptions.None);

            string[] Pos = b[1].Split(new string[] { ", " }, System.StringSplitOptions.None);
            if (StatusID == 1.1F)
                _component = "(" + _component + ", " + Pos[2];
            else if (StatusID == 1.2F)
                _component = Pos[0] + ", " + Pos[1] + ", " + _component + ")";

            string c = "";
            for (int i = 0; i < b.Length; i++)
            {
                if (i == (int)StatusID)
                    c = c + _component;
                else c = c + b[i];

                if (i < b.Length - 1)
                    c = c + "; ";
            }

            component[Bloc] = c;
            Instance(Bloc, true);
        }
    }
    public string GetBlocStatus(float StatusID, int Bloc = -1)
    {
        if (Bloc == -1)
            Bloc = SelectedBlock;

        if (file != "" & component.Length > Bloc)
        {
            try
            {
                string[] a = component[Bloc].Split(new string[] { "; " }, System.StringSplitOptions.None);

                if (StatusID < a.Length & StatusID == (int)StatusID)
                    return a[(int)StatusID];
                else if (StatusID < a.Length)
                    return a[(int)StatusID].Split(new string[] { ", " }, System.StringSplitOptions.None)[(int)(StatusID * 10 - (int)StatusID * 10)].Replace(")", "").Replace("(", "");
                else return "";
            }
            catch { return ""; }
        }
        else return "";
    }

    public void DeleteSelectedBloc(bool fromUpdateScript)
    {
        if (SelectedBlock != -1)
        {
            int SB = SelectedBlock;
            SelectedBlock = -1;
            string[] NewComponent = new string[component.Length - 1];

            Transform obj = transform.GetChild(1).Find("Objet n° " + SB);
            if (obj != null)
                Destroy(obj.gameObject);

            for (int i = 0; i < NewComponent.Length; i++)
            {
                if (i < SB)
                    NewComponent[i] = component[i];
                else
                {
                    NewComponent[i] = component[i + 1];

                    Transform objet = transform.GetChild(1).Find("Objet n° " + (i + 1));
                    if (objet != null)
                        objet.name = "Objet n° " + i;
                }
            }
            component = NewComponent;
            File.WriteAllLines(file, component);
            SelectedBlock = -1;
        }
        else if (!fromUpdateScript)
        {
            NoBlocSelectedPanel.SetActive(true);
            StartCoroutine(WaitForDesableNoBlocSelectedPanel(2F));
        }
    }
    public IEnumerator WaitForDesableNoBlocSelectedPanel(float sec)
    {
        yield return new WaitForSeconds(sec);
        NoBlocSelectedPanel.SetActive(false);
    }

    public void Deplacer(int x, int y)
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
}
