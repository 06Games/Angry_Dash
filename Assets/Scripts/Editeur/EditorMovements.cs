using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorMovements : MonoBehaviour
{
    public Editeur Editor;
    Transform Content;

    void Start()
    {
        Content = transform.GetChild(0).GetComponent<ScrollRect>().content;

#if UNITY_STANDALONE || UNITY_EDITOR
        transform.GetChild(1).gameObject.SetActive(false);
#else
        transform.GetChild(1).gameObject.SetActive(true);
#endif
    }

    public void DownUp(float mvt) { Translate(new Vector2(0, mvt)); }
    public void LeftRight(float mvt) { Translate(new Vector2(mvt, 0)); }
    public void Translate(Vector2 mvt){
        int[] blocks = Editor.SelectedBlock;
        for (int i = 0; i < blocks.Length; i++)
        {
            GameObject go = GameObject.Find("Objet n° " + blocks[i]);
            if (go != null)
            {
                go.transform.Translate(mvt * 50F);
                Editor.ChangBlocStatus("Position", ((Vector2)(go.transform.position - new Vector3(25,25)) / 50F).ToString(), new int[] { blocks[i] });
            }
        }
    }

    public void RotateZ(float mvt)
    {
        int[] blocks = Editor.SelectedBlock;
        for (int i = 0; i < blocks.Length; i++)
        {
            GameObject go = GameObject.Find("Objet n° " + blocks[i]);
            if (go != null)
            {
                go.transform.Rotate(new Vector3(0, 0, mvt));
                Editor.ChangBlocStatus("Rotate", go.transform.rotation.eulerAngles.z.ToString("0"), new int[] { blocks[i] });
            }
        }
    }
}
