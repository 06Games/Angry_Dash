﻿using System.Collections;
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

    public GameObject BulleDeveloppementCat;
    public Sprite[] BulleDeveloppementCatSp;

    #region UI
    public void CreateFile(string fileName, string directory, string desc)
    {
        string txt = directory + fileName + ".level";
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);
        File.CreateText(txt);
        Selection.SetActive(false);
        gameObject.SetActive(true);

        component = new string[] { "//Description", desc, "", "//Arrière Plan", "1; 4b4b4b255", " ", "//Blocks", "" };
        file = txt;
        transform.GetChild(0).gameObject.SetActive(true);

        transform.GetChild(0).GetChild(2).GetChild(1).GetChild(0).GetChild(1).GetComponent<Background>().ActualiseFond(this);
    }

    public void EditFile(string txt)
    {
        file = txt;
        Selection.SetActive(false);
        gameObject.SetActive(true);

        component = File.ReadAllLines(txt);
        for (int i = 7; i < component.Length; i++)
            Instance(i);
        transform.GetChild(0).gameObject.SetActive(true);

        transform.GetChild(0).GetChild(2).GetChild(1).GetChild(0).GetChild(1).GetComponent<Background>().ActualiseFond(this);
    }

    public void ExitEdit()
    {
        if (GameObject.Find("Audio") != null)
            GameObject.Find("Audio").GetComponent<menuMusic>().Play();
        Selection.SetActive(true);
        Selection.GetComponent<EditorSelect>().NewStart();
        //transform.GetChild(0).gameObject.SetActive(false);
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
    }
    #endregion

    private void Start()
    {
        if(File.Exists(Application.temporaryCachePath + "/play.txt"))
        {
            if(File.ReadAllLines(Application.temporaryCachePath + "/play.txt").Length > 0)
            {
                file = File.ReadAllLines(Application.temporaryCachePath + "/play.txt")[0];
                File.WriteAllText(Application.temporaryCachePath + "/play.txt", "");
                EditFile(file);
            }
            else Selection.SetActive(true);
        }
        else Selection.SetActive(true);

        cam = Selection.GetComponent<EditorSelect>().Cam.GetComponent<Camera>();
        zoomIndicator.gameObject.SetActive(false);

        transform.GetChild(0).GetChild(2).GetChild(1).GetChild(0).GetChild(1).GetComponent<Background>().Charg();
        BulleDeveloppementCat.SetActive(false);
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
            if(SelectedZone == null)
            {
                SelectedZone = Instantiate(SelectedZonePref, transform.GetChild(1));
                SelectedZone.name = "Selected Block";
            }
        }

        //Sauvegarde Automatique
        if (file != "" & component.Length != 0)
        {
            File.WriteAllLines(file, component);
        }

        //Détection de la localisation lors de l'ajout d'un bloc

        if (AddBlocking)
        {
            KeyCode clic = KeyCode.Mouse0;
            if (SelectMode)
                clic = KeyCode.Mouse1;

            if (Input.GetKey(clic))
            {
                if (Input.mousePosition.y > Screen.height / 4)
                {
                    //float ZoomIndice = (cam.orthographicSize / Screen.height) + 0.5F;
                    float PosDoigtX = (Input.mousePosition.x - 25) / 50;
                    float PosDoigtY = (Input.mousePosition.y - 25) / 50;
                    float IndiceChangPosCamX = cam.transform.position.x / (Screen.width / 2);
                    float IndiceChangPosCamY = cam.transform.position.y / (Screen.height / 2);

                    int x = Mathf.RoundToInt(PosDoigtX * IndiceChangPosCamX);
                    int y = Mathf.RoundToInt(PosDoigtY * IndiceChangPosCamY);


                    Vector3 a = new Vector3(x, y, 0);
                    string color = ColorToHex(new Color32(190, 190, 190, 255));
                    float id = newblockid;
                    if (id > 10000)
                        id = (newblockid - 10000F) / 10F;

                    bool p = true;
                    for(int i = 0; i < component.Length; i++)
                    {
                        if (component[i] == id + "; " + a + "; 0; " + color)
                            p = false;
                    }
                    if (p)
                        CreateBloc(x, y, new Color32(190, 190, 190, 255));
                }
            }
        }

        if (SelectBlocking)
        {
            if (Input.mousePosition.y > Screen.height / 4)
            {
                //float ZoomIndice = (cam.orthographicSize / Screen.height) + 0.5F;
                float PosDoigtX = (Input.mousePosition.x - 25) / 50;
                float PosDoigtY = (Input.mousePosition.y - 25) / 50;
                float IndiceChangPosCamX = cam.transform.position.x / (Screen.width / 2);
                float IndiceChangPosCamY = cam.transform.position.y / (Screen.height / 2);

                int x = Mathf.RoundToInt(PosDoigtX * IndiceChangPosCamX);
                int y = Mathf.RoundToInt(PosDoigtY * IndiceChangPosCamY);

                SelectedBlock = GetBloc(x, y);
            }
            SelectBlocking = false;
        }

        NoBlocSelectedPanel.SetActive(!Contenu[3].activeInHierarchy & !Contenu[0].activeInHierarchy & !Contenu[1].activeInHierarchy & !Contenu[4].activeInHierarchy & SelectedBlock == -1);
        if (SelectedBlock != -1)
        {
            SelectedZone.SetActive(true);

            string a = component[SelectedBlock].Split(new string[] { "; " }, System.StringSplitOptions.None)[1];
            string[] b = a.Replace("(", "").Replace(" ", "").Replace(")", "").Split(new string[] { "," }, System.StringSplitOptions.None);
            float[] c = new float[] { float.Parse(b[0]) * 50, float.Parse(b[1]) * 50, float.Parse(b[2]) };
            SelectedZone.transform.position = new Vector3(c[0] + (BlocScale / 2), c[1] + (BlocScale / 2));
            SelectedZone.transform.localScale = new Vector2(Screen.height / (Screen.height/50) + 1, Screen.height / (Screen.height / 50) + 1);
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

        /*#if UNITY_STANDALONE || UNITY_EDITOR
                int MoveX = 0;
                int MoveY = 0;

                int Speed = 5;

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

        #endif*/
    }

    void Zoom(int Z = 0)
    {
        if (Z == 0)
            Z = ZoomSensitive;

        if(cam.orthographicSize > 240)
        {
            cam.orthographicSize = cam.orthographicSize - Z;

            float zoom = Screen.height / cam.orthographicSize;
            cam.transform.position = new Vector3(Screen.width / zoom, Screen.height / zoom, -10);
        }
        StartCoroutine(ZoomIndicator());
    }
    void Dezoom(int Z = 0)
    {
        if (Z == 0)
            Z = ZoomSensitive;

        if (cam.orthographicSize < 1200)
        {
            cam.orthographicSize = cam.orthographicSize + Z;

            float zoom = Screen.height / cam.orthographicSize;
            cam.transform.position = new Vector3(Screen.width / zoom, Screen.height / zoom, -10);
        }
        StartCoroutine(ZoomIndicator());
    }
    IEnumerator ZoomIndicator()
    {
        zoomIndicator.gameObject.SetActive(true);
        zoomIndicator.value = (cam.orthographicSize-240) / 960 * -1 + 1;
        yield return new WaitForSeconds(1F);
        
        bool Ctrl = Input.GetKey(KeyCode.LeftControl) | Input.GetKey(KeyCode.RightControl);
        if (Input.GetAxis("Mouse ScrollWheel") == 0 & !Ctrl)
            zoomIndicator.gameObject.SetActive(false);
    }

    #region GestionBloc
    void CreateBloc(int x, int y, Color32 _Color)
    {
        Vector3 a = new Vector3(x, y, 0);
        string color = ColorToHex(_Color);

        float id = newblockid;
        if(id > 10000)
            id = (newblockid - 10000F) /10F;

        component = component.Union(new string[1] { id.ToString(".0####") + "; " + a + "; 0; " + color }).ToArray();
        //AddBlocking = false;
        Instance(component.Length - 1);
    }
    public void AddBlock(string _id)
    {
        float id = float.Parse(_id);

        if(id == newblockid)
            AddBlocking = !AddBlocking;
        else
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
                tex.LoadImage(File.ReadAllBytes(Application.persistentDataPath + "/Textures/0/" + newblockid.ToString(".0####") + ".png"));
                Contenu[3].transform.GetChild(i).GetChild(0).GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
            }
            else
            {
                Contenu[3].transform.GetChild(i).GetComponent<Image>().color = new Color32(0, 0, 0, 255);
                Texture2D tex = new Texture2D(1, 1);
                tex.LoadImage(File.ReadAllBytes(Application.persistentDataPath + "/Textures/0/" + i.ToString(".0####") + ".png"));
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
            for (int i = 0; i < Directory.GetFiles(path, id+".*", SearchOption.AllDirectories).Length; i++)
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

    public void Instance(int num)
    {
        float id = float.Parse(component[num].Split(new string[] { "; " }, System.StringSplitOptions.None)[0]);
        Vector3 p = GetObjectPos(num);
        Vector3 pos = new Vector3(p.x, p.y, 0);
        Quaternion rot = new Quaternion();
        rot.eulerAngles = new Vector3(0, 0, int.Parse(GetBlocStatus(2, num)));
        GameObject Pref = null;
        if (id < 1)
            Pref = TriggerPrefabs[(int)(id * 10F)-1];
        else Pref = Prefab;
        GameObject go = Instantiate(Pref, pos, rot, transform.GetChild(1));
        go.name = "Objet n° " + num;
        go.transform.localScale = new Vector2(Screen.height / (Screen.height / 50), Screen.height / (Screen.height / 50));
        SpriteRenderer SR = go.GetComponent<SpriteRenderer>();
        
        Texture2D tex = new Texture2D(1, 1);
        tex.LoadImage(File.ReadAllBytes(Application.persistentDataPath + "/Textures/0/" + id.ToString(".0####") + ".png"));
        SR.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
        
        SR.color = HexToColor(GetBlocStatus(3, num));
        SR.sortingOrder = (int)p.z;
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
    public static UnityEngine.Color HexToColor(string hex)
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

    public void ChangBlocStatus(float StatusID, string conponent, int Bloc = -1)
    {
        if (Bloc == -1)
            Bloc = SelectedBlock;

        string[] b = component[Bloc].Split(new string[] { "; " }, System.StringSplitOptions.None);
        string[] Pos = b[1].Split(new string[] { ", " }, System.StringSplitOptions.None);

        string c = "";
        if (StatusID == 0)
            c = conponent + "; " + b[1] + "; " + b[2] + "; " + b[3];
        else if (StatusID == 1)
            c = b[0] + "; " + conponent + "; " + b[2] + "; " + b[3];
        else if(StatusID == 1.1F)
            c = b[0] + "; (" + conponent + ", " + Pos[2] + "; " + b[2] + "; " + b[3];
        else if (StatusID == 1.2F)
            c = b[0] + "; " + Pos[0] + ", " + Pos[1] + ", " + conponent + "); " + b[2] + "; " + b[3];
        else if (StatusID == 2)
            c = b[0] + "; " + b[1] + "; " + conponent + "; " + b[3];
        else if (StatusID == 3)
            c = b[0] + "; " + b[1] + "; " + b[2] + "; " + conponent;

        component[Bloc] = c;
        Destroy(GameObject.Find("Objet n° " + Bloc));
        Instance((Bloc));
    }
    public string GetBlocStatus(float StatusID, int Bloc = -1)
    {
        if (Bloc == -1)
            Bloc = SelectedBlock;

        if (file != "" & component.Length > Bloc)
        {
            string[] a = component[Bloc].Split(new string[] { "; " }, System.StringSplitOptions.None);

            if (StatusID < a.Length & StatusID == (int)StatusID)
                return a[(int)StatusID];
            else if (StatusID < a.Length)
                return a[(int)StatusID].Split(new string[] { ", " }, System.StringSplitOptions.None)[(int)(StatusID * 10 - (int)StatusID * 10)];
            else return "";
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

            //GameObject go = GameObject.Find("Objet n° " + SB);

            //while (transform.GetChild(1).GetChild(SB - 6).gameObject.name != "Objet n° " + SB)
            //{
                GameObject go = transform.GetChild(1).GetChild(SB - 6).gameObject;
                Destroy(go);
            //}

            for (int i = 0; i < NewComponent.Length; i++)
            {
                if (i < SB)
                    NewComponent[i] = component[i];
                else
                {
                    NewComponent[i] = component[i + 1];
                    //GameObject.Find("Objet n° " + (i + 1)).name = "Objet n° " + i;
                }
            }
            component = NewComponent;
            File.WriteAllLines(file, component);
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
        float CamX = cam.transform.position.x;
        float CamY = cam.transform.position.y;

        if(x < 0)
        {
            if (cam.transform.position.x > Screen.width / 2)
                CamX = cam.transform.position.x + x;
        }
        else if(x > 0)
            CamX = cam.transform.position.x + x;

        if (y < 0)
        {
            if (cam.transform.position.y > Screen.height / 2)
                CamY = cam.transform.position.y + y;
        }
        else if (y > 0)
            CamY = cam.transform.position.y + y;

        cam.transform.position = new Vector3(CamX, CamY, -10);
    }

    public void PlayLevel()
    {
        File.WriteAllLines(Application.temporaryCachePath + "/play.txt", new string[2] { file, "Editor" });
       GameObject.Find("LoadingScreen").GetComponent<LoadingScreenControl>().LoadScreen("Player");
    }

    public void SelectModeChang(bool enable) { SelectMode = enable; }
}
