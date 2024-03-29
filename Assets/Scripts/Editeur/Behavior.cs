﻿using System.Linq;
using Tools;
using UnityEngine;
using UnityEngine.UI;

public class Behavior : MonoBehaviour
{

    public Editeur editor;
    private int[] SB;

    private void Update()
    {
        if (editor.SelectedBlock.Length == 0) { transform.parent.parent.GetComponent<Edit>().EnterToEdit(); return; }

        var Boost = transform.GetChild(1).GetChild(0).GetComponent<InputField>();

        if (SB != editor.SelectedBlock)
        {
            if (editor.SelectedBlock.Length > 0)
            {
                SB = editor.SelectedBlock;
                float.TryParse(editor.GetBlocStatus("Behavior", SB[0]), out var id);
                transform.GetChild(0).GetChild((int)id).GetComponent<Toggle>().isOn = true;

                var ID = id.ToString().Split(".");
                if (ID.Length == 2) Boost.text = ID[1];
                else Boost.text = "0";
            }
            else { transform.parent.parent.GetComponent<Edit>().EnterToEdit(); return; }
        }

        var col = GetComponent<ToggleGroup>().ActiveToggles().FirstOrDefault().name;
        if (col == "2" | col == "3") Boost.interactable = true;
        else Boost.interactable = false;

        var b = -1;
        if (int.TryParse(Boost.text, out b))
        {
            if (b.ToString() != Boost.text) Boost.text = b.ToString();
        }
        else if (!string.IsNullOrEmpty(Boost.text)) Boost.text = "0";

        if (b != -1) col = col + "." + b;
        editor.ChangBlocStatus("Behavior", col, SB);

    }
}
