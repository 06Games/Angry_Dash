using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetVersion : MonoBehaviour {

    void Start() {
        GetComponent<Text>().text = LangueAPI.StringWithArgument("gameVersion", new string[1]{Base.GetVersion()});
	}
}
