using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlocColor : MonoBehaviour {

    public Editeur editeur;
    public int Bloc = -1;
    ColorPicker CP;

    private void Start()
    {
        CP = transform.GetChild(0).GetComponent<ColorPicker>();
        if (string.IsNullOrEmpty(editeur.file)) gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Bloc != editeur.SelectedBlock)
        {
            Bloc = editeur.SelectedBlock;
            CP.CurrentColor = Editeur.HexToColor(editeur.GetBlocStatus(3, Bloc));
        }
        else editeur.ChangBlocStatus(3, Editeur.ColorToHex(CP.CurrentColor), Bloc);
    }
}
