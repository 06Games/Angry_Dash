using UnityEngine;

public class RespawnMode : MonoBehaviour
{
    public Editeur editor;

    private void Start()
    {
        if (string.IsNullOrEmpty(editor.file)) gameObject.SetActive(false);
    }

    public void Actualise()
    {
        if (string.IsNullOrEmpty(editor.file)) { transform.parent.GetComponent<CreatorManager>().array = 0; return; }

        for (int i = 1; i < transform.childCount; i++)
            transform.GetChild(i).GetComponent<UnityEngine.UI.Button>().interactable = (i - 1) != editor.level.respawnMode;
    }

    public void Chang(int btn)
    {
        editor.level.respawnMode = btn;
        for (int i = 1; i < transform.childCount; i++)
            transform.GetChild(i).GetComponent<UnityEngine.UI.Button>().interactable = (i - 1) != btn;
    }
}
