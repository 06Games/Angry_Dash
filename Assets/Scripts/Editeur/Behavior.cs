using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Behavior : MonoBehaviour {

    public Editeur editor;

	void Update ()
    {
        editor.ChangBlocStatus(4, GetComponent<ToggleGroup>().ActiveToggles().FirstOrDefault().name);
    }
}
