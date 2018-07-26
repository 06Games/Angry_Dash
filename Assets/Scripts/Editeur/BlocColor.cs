using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlocColor : MonoBehaviour {

    public Editeur editeur;
    public int Bloc = -1;
    ColorPicker CP;
    public ColorPicker cpExpend;

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
            Expend(false);
        }
        else
        {
            if(transform.GetChild(0).gameObject.activeInHierarchy)
                editeur.ChangBlocStatus(3, Editeur.ColorToHex(CP.CurrentColor), Bloc);
            else editeur.ChangBlocStatus(3, Editeur.ColorToHex(cpExpend.CurrentColor), Bloc);
        }
    }

    public void Expend(bool expend)
    {
        transform.GetChild(0).gameObject.SetActive(!expend);
        cpExpend.gameObject.SetActive(expend);

        CP.CurrentColor = Editeur.HexToColor(editeur.GetBlocStatus(3, Bloc));
        cpExpend.CurrentColor = Editeur.HexToColor(editeur.GetBlocStatus(3, Bloc));
        editeur.SelectBlocking = false;
        editeur.bloqueSelect = expend;
        editeur.SelectedBlock = Bloc;
    }
}
