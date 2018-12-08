using UnityEngine;
using UnityEngine.UI;

public class LangueTxt : MonoBehaviour {

    public string category = "native";
    public string id;
    public bool keepIfNotExist = true;
    public string[] arg = new string[0];
	void Start () {
        string txt = null;
        if(arg.Length > 0)
            txt = LangueAPI.StringWithArgument(category, id, arg);
        else txt = LangueAPI.String(category, id);

        if (txt != null)
            GetComponent<Text>().text = txt;
        else if (!keepIfNotExist)
            GetComponent<Text>().text = "<color=red>Language File Error</color>";
    }
}
