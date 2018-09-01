using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlocColor : MonoBehaviour {

    public Editeur editeur;
    public int[] Bloc;
    ColorPicker CP;
    public ColorPicker cpExpend;

    private void Start()
    {
        CP = transform.GetChild(0).GetComponent<ColorPicker>();
        if (string.IsNullOrEmpty(editeur.file)) gameObject.SetActive(false);
    }

    private void Update()
    {
        if (editeur.SelectedBlock.Length == 0) { transform.parent.GetComponent<Edit>().EnterToEdit(); return; }

        if (Bloc != editeur.SelectedBlock)
        {
            Bloc = editeur.SelectedBlock;
            Expend(false);
        }
        else
        {
            if (transform.GetChild(0).gameObject.activeInHierarchy)
                editeur.ChangBlocStatus(3, Editeur.ColorToHex(CP.CurrentColor), Bloc);
            else editeur.ChangBlocStatus(3, Editeur.ColorToHex(cpExpend.CurrentColor), Bloc);
        }
    }

    public void Expend(bool expend)
    {
        if (gameObject.activeInHierarchy)
        {
            transform.GetChild(0).gameObject.SetActive(!expend);
            cpExpend.gameObject.SetActive(expend);
            CP.transform.GetChild(0).GetChild(1).GetComponent<HexColorField>().displayAlpha = true;
            CP.transform.GetChild(3).GetChild(3).gameObject.SetActive(true);
            cpExpend.transform.GetChild(2).gameObject.SetActive(false);

            if (Bloc.Length == 0) { transform.parent.GetComponent<Edit>().EnterToEdit(); return; }
            CP.CurrentColor = Editeur.HexToColor(editeur.GetBlocStatus(3, Bloc[0]));
            cpExpend.CurrentColor = Editeur.HexToColor(editeur.GetBlocStatus(3, Bloc[0]));
            editeur.SelectBlocking = false;
            editeur.bloqueSelect = expend;
            editeur.SelectedBlock = Bloc;
        }
        else
        {
            cpExpend.gameObject.SetActive(expend);
            cpExpend.transform.GetChild(2).gameObject.SetActive(false);
            editeur.bloqueSelect = false;
        }
    }
}
