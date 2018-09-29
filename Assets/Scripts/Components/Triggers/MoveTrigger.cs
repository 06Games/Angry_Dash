using System.Collections;
using UnityEngine;

public class MoveTrigger : MonoBehaviour
{

    public string[] Blocks;
    public Vector2 Range = new Vector2(0, 0);
    public float Speed = 1;
    public int Type = 0;
    public bool MultiUsage = false;
    bool Used = false;


    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!Used | MultiUsage)
            StartCoroutine(Move());
    }

    IEnumerator Move()
    {
        for (int i = 0; i < Speed; i++)
        {
            for (int b = 0; b < Blocks.Length; b++)
            {
                GameObject go = GameObject.Find("Objet n° " + Blocks[b]);

                Vector2 moveVector = new Vector2();
                if (Type == 0) moveVector = Range / Speed;
                else if (Type == 1)
                {
                    float v = i + 1;
                    if (i > Speed / 2)
                        v = Speed - i;
                    moveVector = (Range / Speed) * (v / ((Speed/2)+1)) * 2;
                }

                if (go != null & Speed >= 1) go.transform.Translate(moveVector * 50);
                if (go.GetComponent<Mur>() != null) go.GetComponent<Mur>().Move = moveVector;
            }

            yield return new WaitForEndOfFrame();
        }

        Used = true;
    }
}
