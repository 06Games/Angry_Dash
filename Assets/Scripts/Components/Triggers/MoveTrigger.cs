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
                if (go != null & Speed >= 1) go.transform.Translate((Range / Speed) * 50);
                if (go.GetComponent<Mur>() != null) go.GetComponent<Mur>().Move = Range / Speed;
            }

            yield return new WaitForEndOfFrame();
        }

        Used = true;
    }
}
