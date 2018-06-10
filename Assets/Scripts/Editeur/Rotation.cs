using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Rotation : MonoBehaviour {

    public InputField IF;
    public Editeur editeur;
    int Bloc;
    public Button[] Button;

    void Update()
    {
        if (Bloc != editeur.SelectedBlock)
        {
            Bloc = editeur.SelectedBlock;
            IF.text = editeur.GetBlocStatus(2F).Replace(")", "");
            if (IF.text == "")
                IF.text = "0";
        }
        else
        {
            editeur.ChangBlocStatus(2, IF.text);

            if (IF.text == "180")
                Button[1].interactable = false;
            else Button[1].interactable = true;

            if (IF.text == "-180")
                Button[0].interactable = false;
            else Button[0].interactable = true;

            if (int.Parse(IF.text) > 180)
                IF.text = "180";
            else if (int.Parse(IF.text) < -180)
                IF.text = "-180";

            if (IF.text == "")
                IF.text = "0";
        }
    }

    public void ChangIFValue(int positive)
    {
        IF.text = (int.Parse(IF.text) + (positive*90)).ToString();
    }
}
