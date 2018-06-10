using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Layer : MonoBehaviour {

    public InputField IF;
    public Editeur editeur;
    int Bloc;
    public Button[] Button;

	void Update () {
        if (Bloc != editeur.SelectedBlock)
        {
            Bloc = editeur.SelectedBlock;
            IF.text = editeur.GetBlocStatus(1.2F).Replace(")", "");
        }
        else {
            try
            {
                IF.text = IF.text.Replace(".0", "");
                int.Parse(IF.text);

                editeur.ChangBlocStatus(1.2F, float.Parse(IF.text).ToString());

                if (IF.text == "999")
                    Button[1].interactable = false;
                else Button[1].interactable = true;

                if (IF.text == "-999")
                    Button[0].interactable = false;
                else Button[0].interactable = true;

                if (int.Parse(IF.text) > 999)
                    IF.text = "999";
                else if (IF.text.Length > 3 & int.Parse(IF.text) >= 0)
                    IF.text = int.Parse(IF.text).ToString();
            }
            catch { Debug.LogError("The input was not a int"); }
        }
    }

    public void ChangIFValue(int positive)
    {
        IF.text = (int.Parse(IF.text) + positive).ToString();
    }
}
