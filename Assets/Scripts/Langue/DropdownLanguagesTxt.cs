using UnityEngine;
using UnityEngine.UI;

public class DropdownLanguagesTxt : MonoBehaviour
{
    public string category = "native";
    public string[] id;
    public bool keepIfNotExist = true;
    void Start()
    {
        for (int i = 0; i < GetComponent<Dropdown>().options.Capacity & i < id.Length; i++) {
            string txt = LangueAPI.String(category, id[i]);
            
            if (txt == null & !keepIfNotExist)
                txt = "<color=red>Language File Error</color>";
            
            GetComponent<Dropdown>().options[i].text = txt;
        }
    }
}
