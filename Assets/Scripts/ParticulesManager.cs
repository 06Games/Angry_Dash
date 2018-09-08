using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ParticulesManager : MonoBehaviour
{

    public float spawnSpeed;
    public GameObject Prefab;
    public Sprite[] Sp;
    public int[] sizeRange;
    public int[] Rotate = new int[2] { 0, 360 };
    public int[] RotateSpeed = new int[2] { 25, 50 };
    public int[] FallSpeed = new int[2] { 5, 8 };
    public Vector2 FallDirector = new Vector2(0, -1);
    public Vector2 SpawnZoneMultiplier = new Vector2(1, 1);

    [Range(0, 360)]public double oldH = 0;

    private void Start()
    {
        /* //En cas d'erreur de Pading
        PreviewLabs.PlayerPrefs.DeleteAll();
        PreviewLabs.PlayerPrefs.Flush();
        */

        NewStart(false);
    }

    public void NewStart(bool b)
    {
        if (!gameObject.activeInHierarchy & b)
            gameObject.SetActive(true);

        if (gameObject.activeInHierarchy)
        {
            for (int i = 0; i < 5; i++)
                Particule();

            StartCoroutine(SpawnLoop());
        }
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            Particule();
            yield return new WaitForSeconds(spawnSpeed);
        }
    }

    void Particule()
    {
        if (string.IsNullOrEmpty(ConfigAPI.GetString("particule.mode")) | ConfigAPI.GetInt("particule.mode") != 0)
        {
            System.Random rnd = new System.Random();

            Vector2 pos = new Vector2(rnd.Next(0, Screen.width * (int)SpawnZoneMultiplier.x), (Screen.height * (int)SpawnZoneMultiplier.y) + sizeRange[1]);
            GameObject go = Instantiate(Prefab, pos, new Quaternion(), transform);
            float scale = rnd.Next(sizeRange[0], sizeRange[1]);
            go.transform.localScale = new Vector2(scale, scale);
            Quaternion rot = new Quaternion();
            rot.eulerAngles = new Vector3(0, 0, rnd.Next(Rotate[0], Rotate[1]));
            go.transform.rotation = rot;

            int SpNb = rnd.Next(0, Sp.Length);
            if (SpNb == Sp.Length)
                SpNb = SpNb - 1;
            go.GetComponent<Image>().sprite = Sp[SpNb];

            Particules pa = go.GetComponent<Particules>();
            int sens = rnd.Next(0, 1);
            if (sens == 0)
                sens = -1;
            pa.RotateSpeed = rnd.Next(RotateSpeed[0], RotateSpeed[1]) * sens;
            pa.FallSpeed = rnd.Next(FallSpeed[0], FallSpeed[1]);
            pa.FallDirection = FallDirector;
            pa.Static = true;

            if (string.IsNullOrEmpty(ConfigAPI.GetString("particule.mode"))) pa.Mode = 1;
            else pa.Mode = ConfigAPI.GetInt("particule.mode");
            if (pa.Mode == 0) Destroy(go); //Au cas où il y aurai un bug
            if (pa.Mode == 2)
            {
                HsvColor hsv = HSVUtil.ConvertRgbToHsv(pa.GetComponent<Image>().color);
                
                double h = oldH + (rnd.Next(0, 6) * 30);
                if (h <= 0) h = h * -1;
                if (h >= 360) h = h - (360 * (int)(h / 360));

                hsv.H = h;
                oldH = hsv.H;
                pa.GetComponent<Image>().color = HSVUtil.ConvertHsvToRgb(hsv, 255);
            }
        }
    }
}
