using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MoveTrigger : MonoBehaviour
{

    public int AffectationType = 0;
    public string[] Blocks;
    public Vector2 Translation = new Vector2(0, 0);
    public bool[] TranslationFromPlayer = new bool[2];
    public int Type = 0;
    public float Speed = 1;
    public bool MultiUsage = false;
    public Vector3 Rotation;
    public bool[] Reset = new bool[2];

    bool Used = false;


    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!Used | MultiUsage)
            StartCoroutine(Move());
    }

    IEnumerator Move()
    {
        int FPS = int.Parse(GameObject.Find("Base").transform.GetChild(1).GetComponent<Text>().text.Replace(" FPS", ""));
        float Frame = Speed * FPS;
        if ((int)(Frame / 2) == Frame / 2 & Type == 1) Frame = Frame + 1; //Support Frame pair si le type est Fluid
        for (int i = 0; i < Frame; i++)
        {
            GameObject[] go = null;
            if (AffectationType == 0)
            {
                go = new GameObject[Blocks.Length];
                for (int b = 0; b < Blocks.Length; b++)
                    go[b] = GameObject.Find("Objet n° " + Blocks[b]);
            }
            else if (AffectationType == 1)
            {
                go = new GameObject[] { GameObject.Find("Main Camera") };
                go[0].GetComponent<MainCam>().OnPlayer = false;
            }

            for (int b = 0; b < go.Length; b++)
            {
                Vector2 moveVector = new Vector2();
                if (Type == 0) moveVector = Translation / Frame;
                else if (Type == 1)
                {
                    float v = i + 1;
                    if (i > Frame / 2)
                        v = Frame - i;
                    moveVector = (Translation / (((int)(Frame / 2) + 1) / v)) / ((int)(Frame / 2) + 1);
                }

                Vector3 rotateVector = new Vector3();
                if (Type == 0) rotateVector = Rotation / Frame;
                else if (Type == 1)
                {
                    float v = i + 1;
                    if (i > Frame / 2)
                        v = Frame - i;
                    rotateVector = (Rotation / (((int)(Frame / 2) + 1) / v)) / ((int)(Frame / 2) + 1);
                }

                if (go != null & Frame >= 1)
                {
                    Vector3 pos = go[b].transform.position;
                    if (Reset[0] & AffectationType == 0)
                        try { pos = GameObject.Find("Main Camera").GetComponent<LevelPlayer>().GetObjectPos(int.Parse(Blocks[b])); } catch { }
                    else if (Reset[0]) GameObject.Find("Main Camera").GetComponent<MainCam>().OnPlayer = true;
                    else
                    {
                        for (int m = 0; m < 2; m++)
                        {
                            if (TranslationFromPlayer[m])
                                pos[m] = GameObject.Find("Main Camera").GetComponent<MainCam>().Player.transform.position[m] + (Translation[m] * 50);
                            else pos[m] = pos[m] + moveVector[m] * 50;
                        }
                    }
                    go[b].transform.position = pos;

                    Quaternion quaternion = new Quaternion();
                    if (Reset[1] & AffectationType == 0)
                        try { quaternion.eulerAngles = Editor_MoveTrigger.getVector3(GameObject.Find("Main Camera").GetComponent<LevelPlayer>().GetBlocStatus("Rotation", int.Parse(Blocks[b]))); } catch { }
                    else if (Reset[1]) GameObject.Find("Main Camera").GetComponent<MainCam>().OnPlayer = true;
                    else quaternion.eulerAngles = go[b].transform.rotation.eulerAngles + rotateVector;
                    go[b].transform.rotation = quaternion;
                }
                if (go[b].GetComponent<Mur>() != null) go[b].GetComponent<Mur>().Move = moveVector;
            }

            yield return new WaitForSeconds(1F / FPS);
        }

        Used = true;
    }
}
