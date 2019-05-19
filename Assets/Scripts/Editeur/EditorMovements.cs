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
    public void Translate(Vector2 mvt)
    {
        int[] blocks = Editor.SelectedBlock;
        for (int i = 0; i < blocks.Length; i++)
        {
            GameObject go = GameObject.Find("Objet n° " + blocks[i]);
            if (go != null)
            {
                go.transform.Translate(mvt * 50F);
                Editor.level.blocks[blocks[i]].position = (Vector2)(go.transform.position - new Vector3(25, 25)) / 50F;
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

    public void Layer(int mvt)
    {
        foreach(int i in Editor.SelectedBlock)
        {
            GameObject go = GameObject.Find("Objet n° " + i);
            if (go != null)
            {
                int layer = (int)Editor.level.blocks[i].position.z + mvt;
                if (layer > -2 & layer < 1000)
                {
                    go.GetComponent<SpriteRenderer>().sortingOrder = layer;
                    Editor.level.blocks[i].position.z = layer;
                }
            }
        }

        if (Editor.SelectedBlock.Length > 0)
        {
            int layer = (int)Editor.level.blocks[Editor.SelectedBlock[0]].position.z;
            if (layer != Editor.selectedLayer & Editor.selectedLayer != -2)
                Editor.ChangeDisplayedLayer(layer - Editor.selectedLayer);
        }
    }
}
