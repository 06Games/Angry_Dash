using System.Collections;
using UnityEngine;

public class MoveTrigger : MonoBehaviour {
    
    public string[] Blocks;
    public Vector2 Range;
    public float Speed;
    public int Type;

    void OnTriggerEnter2D(Collider2D collision)
    {
        StartCoroutine(Move());
    }
    
    IEnumerator Move()
    {
        float FPS = ConfigAPI.GetInt("FPS.maxValue");
        if (FPS == -1) FPS = 9999;

        int Frame = 60;
        for (int i = 0; i < Frame; i++)
        {
            for(int b = 0; b < Blocks.Length; b++)
            {
                GameObject go = GameObject.Find("Objet n° " + Blocks[b]);
                if (go != null) go.transform.Translate((Range / Frame) * 50);
            }

            float speed = (1 / Speed) / Frame;
            yield return new WaitForSeconds(speed);
        }
    }
}
