using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteBloc : MonoBehaviour {

    public Editeur editor;

	void Update () {
        if (editor.file != "")
        {
            editor.DeleteSelectedBloc(true);
            editor.SelectModeChang(true);
        }
	}
}
