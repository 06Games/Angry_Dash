using UnityEngine;
using UnityEngine.UI;

public class LangueTxt : MonoBehaviour {

    public string id;
    public string[] arg = new string[0];
	void Start () {
        if(arg.Length > 0)
            GetComponent<Text>().text = LangueAPI.StringWithArgument(id, arg);
        else GetComponent<Text>().text = LangueAPI.String(id);
    }
}
