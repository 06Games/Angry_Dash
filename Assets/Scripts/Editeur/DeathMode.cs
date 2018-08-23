using UnityEngine;

public class DeathMode : MonoBehaviour {

    public Editeur editor;

	public void Actualise()
    {
        int deathModeLine = -1;
        for (int x = 0; x < editor.component.Length; x++)
        {
            if (editor.component[x].Contains("deathMode = ") & deathModeLine == -1)
                deathModeLine = x;
        }
        int deathMode = 0;
        if (deathModeLine != -1)
            deathMode = int.Parse(editor.component[deathModeLine].Replace("deathMode = ", ""));

        for (int i = 1; i < transform.childCount; i++)
            transform.GetChild(i).GetComponent<UnityEngine.UI.Button>().interactable = (i - 1) != deathMode;
    }

    public void Chang(int btn)
    {
        int deathModeLine = -1;
        for (int x = 0; x < editor.component.Length; x++)
        {
            if (editor.component[x].Contains("deathMode = ") & deathModeLine == -1)
                deathModeLine = x;
        }
        if (deathModeLine != -1)
            editor.component[deathModeLine] = "deathMode = " + btn;

        for (int i = 1; i < transform.childCount; i++)
            transform.GetChild(i).GetComponent<UnityEngine.UI.Button>().interactable = (i - 1) != btn;
    }
}
