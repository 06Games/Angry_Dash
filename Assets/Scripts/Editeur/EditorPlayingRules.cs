using AngryDash.Language;
using UnityEngine;
using UnityEngine.UI;

public class EditorPlayingRules : MonoBehaviour
{
    public Editeur editor;

    private void Start()
    {
        if (string.IsNullOrEmpty(editor.file)) gameObject.SetActive(false);
    }

    public void Actualise()
    {
        if (string.IsNullOrEmpty(editor.file)) { transform.parent.GetComponent<MenuManager>().array = 0; return; }

        //Respawn Mode
        for (int i = 1; i < transform.GetChild(0).childCount; i++)
            transform.GetChild(0).GetChild(i).GetComponent<Button>().interactable = (i - 1) != editor.level.player.respawnMode;

        //Player Settings
        transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<InputField>().text = editor.level.player.distance.ToString();

        //Victory Conditions
        string maxThrow = editor.level.victoryConditions.maxThrow.ToString();
        if (editor.level.victoryConditions.maxThrow <= 0)
            maxThrow = LangueAPI.Get("native", "editor.options.playingRules.victoryConditions.maxThrows.unlimited", "Unlimited");
        transform.GetChild(2).GetChild(1).GetChild(0).GetComponent<InputField>().text = maxThrow;
    }

    public void ChangRespawnMode(int btn) { editor.level.player.respawnMode = btn; Actualise(); }
    public void ChangPlayerDistance(InputField input) { float.TryParse(input.text, out editor.level.player.distance); Actualise(); }
    public void ChangVictoryThrow(InputField input) { int.TryParse(input.text, out editor.level.victoryConditions.maxThrow); Actualise(); }
}
