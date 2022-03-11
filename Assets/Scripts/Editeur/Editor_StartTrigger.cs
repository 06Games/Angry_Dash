using AngryDash.Language;
using UnityEngine;
using UnityEngine.UI;

public class Editor_StartTrigger : MonoBehaviour
{

    public Editeur editor;
    private int[] SB;

    private string All => LangueAPI.Get("native", "editor.edit.genericEvent.player.infinity", "All");

    public int[] Players = new int[2];

    private void Update()
    {
        if (editor.SelectedBlock.Length == 0) { transform.parent.GetComponent<Edit>().EnterToEdit(); return; }

        if (SB != editor.SelectedBlock)
        {
            if (float.Parse(editor.GetBlocStatus("ID", editor.SelectedBlock[0])) == 0.1F)
            {
                SB = editor.SelectedBlock;
                var temp = editor.GetBlocStatus("Players", SB[0]);
                try
                {
                    var tempArray = temp.Substring(1, temp.Length - 2).Split(',');
                    for (var i = 0; i < tempArray.Length; i++)
                        Players[i] = int.Parse(tempArray[i]);
                }
                catch { }
                Actualise();
            }
            else { transform.parent.GetComponent<Edit>().EnterToEdit();
            }
        }
    }

    private void Actualise()
    {
        transform.GetChild(1).GetChild(1).GetChild(2).GetComponent<InputField>().text = Players[0].ToString();
        transform.GetChild(1).GetChild(2).GetChild(2).GetComponent<InputField>().text = Players[1].ToString();
    }


    public void PlayerChanged(InputField inputField)
    {
        var actualValue = -1;
        if (inputField.text == "") actualValue = -1;
        else if (inputField.text == All) actualValue = 0;
        else if (!int.TryParse(inputField.text, out actualValue)) { actualValue = 0; inputField.text = All; }
        else if (actualValue < 0) actualValue = 0;

        if (actualValue == 0) inputField.text = All;

        if (actualValue == -1) actualValue = 0;
        Players[inputField.transform.parent.GetSiblingIndex() - 1] = actualValue;

        var param = "(" + Players[0];
        for (var i = 1; i < Players.Length; i++)
            param = param + "," + Players[i];
        param = param + ")";
        editor.ChangBlocStatus("Players", param, SB);
    }
    public void PlayerAdd(InputField inputField) { PlayerModif(inputField, 1); }
    public void PlayerRemove(InputField inputField) { PlayerModif(inputField, -1); }
    public void PlayerModif(InputField inputField, int value)
    {
        var actualValue = -1;
        if (inputField.text == "") actualValue = 0;
        else if (!int.TryParse(inputField.text, out actualValue)) actualValue = 0;

        if ((actualValue > 0 & value < 0) | value > 0)
            actualValue = actualValue + value;

        if (actualValue == 0) inputField.text = All;
        else inputField.text = actualValue.ToString();
    }
}
