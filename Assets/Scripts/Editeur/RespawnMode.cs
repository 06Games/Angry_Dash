using UnityEngine;

public class RespawnMode : MonoBehaviour {

    public Editeur editor;

    private void Start()
    {
        if (string.IsNullOrEmpty(editor.file)) gameObject.SetActive(false);
    }

    public void Actualise()
    {
        if (string.IsNullOrEmpty(editor.file))
        {
            transform.parent.GetComponent<CreatorManager>().array = 0;
            return;
        }

        int respawnModeLine = -1;
        for (int x = 0; x < editor.component.Length; x++)
        {
            if (editor.component[x].Contains("respawnMode = ") & respawnModeLine == -1)
                respawnModeLine = x;
        }
        int respawnMode = 0;
        if (respawnModeLine != -1)
            respawnMode = int.Parse(editor.component[respawnModeLine].Replace("respawnMode = ", ""));

        for (int i = 1; i < transform.childCount; i++)
            transform.GetChild(i).GetComponent<UnityEngine.UI.Button>().interactable = (i - 1) != respawnMode;
    }

    public void Chang(int btn)
    {
        int respawnModeLine = -1;
        for (int x = 0; x < editor.component.Length; x++)
        {
            if (editor.component[x].Contains("respawnMode = ") & respawnModeLine == -1)
                respawnModeLine = x;
        }
        if (respawnModeLine != -1)
            editor.component[respawnModeLine] = "respawnMode = " + btn;

        for (int i = 1; i < transform.childCount; i++)
            transform.GetChild(i).GetComponent<UnityEngine.UI.Button>().interactable = (i - 1) != btn;
    }
}
