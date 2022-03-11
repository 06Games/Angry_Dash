using UnityEngine;
using UnityEngine.UI;

public class EditorMovements : MonoBehaviour
{
    public Editeur Editor;
    private Transform Content;

    private void Start()
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
        var blocks = Editor.SelectedBlock;
        for (var i = 0; i < blocks.Length; i++)
        {
            var go = GameObject.Find("Objet n° " + blocks[i]);
            if (go != null)
            {
                go.transform.Translate(mvt * 50F);
                Editor.level.blocks[blocks[i]].position = (Vector2)(go.transform.position - new Vector3(25, 25)) / 50F;
            }
        }
    }

    public void RotateZ(float mvt)
    {
        var blocks = Editor.SelectedBlock;
        for (var i = 0; i < blocks.Length; i++)
        {
            var go = GameObject.Find("Objet n° " + blocks[i]);
            if (go != null)
            {
                go.transform.Rotate(new Vector3(0, 0, mvt));
                Editor.ChangBlocStatus("Rotate", go.transform.rotation.eulerAngles.z.ToString("0"), new[] { blocks[i] });
            }
        }
    }

    public void Layer(int mvt)
    {
        foreach (var i in Editor.SelectedBlock)
        {
            var go = GameObject.Find("Objet n° " + i);
            if (go != null)
            {
                var layer = (int)Editor.level.blocks[i].position.z + mvt;
                if (layer > -2 & layer < 1000)
                {
                    go.GetComponent<SpriteRenderer>().sortingOrder = layer;
                    Editor.level.blocks[i].position.z = layer;
                }
            }
        }

        if (Editor.SelectedBlock.Length > 0)
        {
            var layer = (int)Editor.level.blocks[Editor.SelectedBlock[0]].position.z;
            if (layer != Editor.selectedLayer & Editor.selectedLayer != -2)
                Editor.ChangeDisplayedLayer(layer - Editor.selectedLayer);
        }
    }
}
