using UnityEngine;
using UnityEngine.UI;

public class EditorOther : MonoBehaviour
{
    public Editeur editor;

    private void Start()
    {
        if (string.IsNullOrEmpty(editor.file)) gameObject.SetActive(false);
    }

    public void Actualise()
    {
        if (string.IsNullOrEmpty(editor.file)) { transform.parent.GetComponent<MenuManager>().array = 0; return; }

        //Ressource Pack
        transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<InputField>().text = editor.level.rpURL;
        transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<Toggle>().isOn = editor.level.rpRequired;
    }

    public void ChangeRPurl(InputField input) { editor.level.rpURL = (input.text.StartsWith("http") ? "" : "http://") + input.text; Actualise(); }
    public void ChangRPneed(Toggle toggle) { editor.level.rpRequired = toggle.isOn; Actualise(); }
}
