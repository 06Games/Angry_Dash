using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParticulesManagerWithoutRandom : MonoBehaviour {

    public Sprite _Particule;
    public GameObject Prefab;
    public int Size;
    public int FallSpeed;
    public float SpawnSpeed;
    public Vector2 FallDirector = new Vector2(0, -1);
    public Vector2 SpawnZoneMultiplier = new Vector2(1, 1);
    public Color32 color = new Color32(255, 255, 255, 255);

    private void Start()
    {
        total = (Size / FallSpeed);
        Size = Screen.height / 5;
        StartSpawn();
    }


    public void StartSpawn ()
    {
        if(transform.childCount == 0)
        {
            Size = Screen.height / 5;
            total = (Size / FallSpeed);
            e = total;
            float xNbPart = (Screen.width * SpawnZoneMultiplier.x) / Size;
            int yNbPart = ((Screen.height * (int)SpawnZoneMultiplier.y) / Size);

            for (int i = yNbPart; i > 0; i = i-1)
            {
                for (int width = 0; width <= xNbPart; width++)
                {
                    int part = Screen.height / Size;
                    float decalage = (yNbPart - i) * Size + Size/2;
                    Vector2 pos = new Vector2((width * Size) - decalage, Screen.height - ((yNbPart-i) * Size) - Size/2);
                    GameObject go = Instantiate(Prefab, pos, new Quaternion(), transform);
                    float scale = Size;
                    go.transform.localScale = new Vector2(scale, scale);
                    Quaternion rot = new Quaternion();
                    rot.eulerAngles = new Vector3(0, 0, 0);
                    go.transform.rotation = rot;
                    go.GetComponent<Image>().sprite = _Particule;
                    go.GetComponent<Image>().color = color;

                    Particules pa = go.GetComponent<Particules>();
                    pa.RotateSpeed = 0;
                    pa.FallSpeed = FallSpeed;
                    pa.FallDirection = FallDirector;
                    pa.Static = false;
                }
            }
        }
        gameObject.SetActive(true);
    }

    public int total = 0;
    public int e = 0;
    void Update()
    {
        if (e > total)
        {
            float xNbPart = (Screen.width * SpawnZoneMultiplier.x) / Size;
            for (int width = 0; width <= xNbPart; width++)
            {
                Vector2 pos = new Vector2(width*Size + (Size/2), Screen.height + Size/2);
                GameObject go = Instantiate(Prefab, pos, new Quaternion(), transform);
                float scale = Size;
                go.transform.localScale = new Vector2(scale, scale);
                Quaternion rot = new Quaternion();
                rot.eulerAngles = new Vector3(0, 0, 0);
                go.transform.rotation = rot;
                go.GetComponent<Image>().sprite = _Particule;
                go.GetComponent<Image>().color = color;

                Particules pa = go.GetComponent<Particules>();
                pa.RotateSpeed = 0;
                pa.FallSpeed = FallSpeed;
                pa.FallDirection = FallDirector;
                pa.Static = false;

                if (total == (Size / FallSpeed) / 2)
                    total = (Size / FallSpeed);
            }
            e = 0;
        }
        else e = e + 1;
    }
}
