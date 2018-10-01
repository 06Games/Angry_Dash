using System.Collections;
using UnityEngine;

public class MoveTrigger : MonoBehaviour
{

    public string[] Blocks;
    public Vector2 Translation = new Vector2(0, 0);
    public int Type = 0;
    public float Speed = 1;
    public bool MultiUsage = false;
    public Vector3 Rotation;

    bool Used = false;


    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!Used | MultiUsage)
            StartCoroutine(Move());
    }

    IEnumerator Move()
    {
        if ((int)(Speed / 2) == Speed / 2 & Type == 1) Speed = Speed + 1; //Support Speed pair si le type est Fluid
        for (int i = 0; i < Speed; i++)
        {
            for (int b = 0; b < Blocks.Length; b++)
            {
                GameObject go = GameObject.Find("Objet n° " + Blocks[b]);

                Vector2 moveVector = new Vector2();
                if (Type == 0) moveVector = Translation / Speed;
                else if (Type == 1)
                {
                    float v = i + 1;
                    if (i > Speed / 2)
                        v = Speed - i;
                    moveVector = (Translation / (((int)(Speed / 2) + 1) / v)) / ((int)(Speed / 2) + 1);
                }

                Vector3 rotateVector = new Vector3();
                if (Type == 0) rotateVector = Rotation / Speed;
                else if (Type == 1)
                {
                    float v = i + 1;
                    if (i > Speed / 2)
                        v = Speed - i;
                    rotateVector = (Rotation / (((int)(Speed/2)+1) /v)) / ((int)(Speed / 2) + 1);
                }

                if (go != null & Speed >= 1)
                {
                    go.transform.Translate(moveVector * 50, Space.World);
                    go.transform.Rotate(rotateVector, Space.Self);
                }
                if (go.GetComponent<Mur>() != null) go.GetComponent<Mur>().Move = moveVector;
            }

            yield return new WaitForEndOfFrame();
        }

        Used = true;
    }
}
