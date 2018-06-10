using UnityEngine;
using UnityEngine.UI;

public class LangueTxt : MonoBehaviour {

    public float id;
	void Update () {
        GetComponent<Text>().text = LangueAPI.String(id);
    }
}
