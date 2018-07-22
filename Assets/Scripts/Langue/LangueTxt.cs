using UnityEngine;
using UnityEngine.UI;

public class LangueTxt : MonoBehaviour {

    public string id;
    public bool keepIfNotExist = true;
    public string[] arg = new string[0];
	void Start () {
        string txt = null;
        if(arg.Length > 0)
            txt = LangueAPI.StringWithArgument(id, arg);
        else txt = LangueAPI.String(id);

        if (txt != null)
            GetComponent<Text>().text = txt;
        else if (!keepIfNotExist)
            GetComponent<Text>().text = "<color=red>Language File Error</color>";
    }
}
