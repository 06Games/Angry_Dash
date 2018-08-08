using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Behavior : MonoBehaviour {

    public Editeur editor;
    int SB;

	void Update ()
    {
        Toggle Boost = transform.GetChild(1).GetComponent<Toggle>();
        InputField t = Boost.transform.GetChild(2).GetComponent<InputField>();

        if (SB != editor.SelectedBlock & editor.SelectedBlock != -1)
        {
            float id = 0;
            try { id = float.Parse(editor.GetBlocStatus(4)); }
            catch { Debug.LogWarning("The block at the line " + editor.SelectedBlock + " as an invalid behavior id"); transform.parent.GetComponent<CreatorManager>().ChangArray(0); return; }
            transform.GetChild(0).GetChild((int)id).GetComponent<Toggle>().isOn = true;

            Boost.isOn = id != (int)id;
            if (Boost.isOn)
                t.text = id.ToString().Split(new string[1] { "." }, System.StringSplitOptions.None)[1];
            else t.text = "";
            SB = editor.SelectedBlock;
        }

        string col = GetComponent<ToggleGroup>().ActiveToggles().FirstOrDefault().name;
        
        if (col == "2" | col == "3")
        {
            Boost.interactable = true;
            t.interactable = true;
        }
        else
        {
            Boost.interactable = false;
            t.interactable = false;
        }

        int b = -1;
        try
        {
            
            b = int.Parse(t.text);

            if (b.ToString() != t.text)
                t.text = b.ToString();
        }
        catch {
            if (!string.IsNullOrEmpty(t.text) & b == 1)
                t.text = "";
        }

        if (Boost.isOn & b != -1)
            col = col + "."+ b;
        editor.ChangBlocStatus(4, col);
    }
}
