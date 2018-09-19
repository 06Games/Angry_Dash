using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edit : MonoBehaviour
{

    public Editeur editeur;
    public GameObject MultiSelectBtn;

    public void EnterToEdit()
    {
        editeur.NoBlocSelectedPanel.SetActive(editeur.SelectedBlock.Length == 0);
        transform.GetChild(GetComponent<CreatorManager>().array).GetComponent<CreatorManager>().Array(0);

#if UNITY_STANDALONE || UNITY_EDITOR
        MultiSelectBtn.SetActive(false);
#else
        MultiSelectBtn.SetActive(true);
#endif
    }

    private void Update()
    {
        if (editeur.SelectedBlock.Length > 0)
        {
            float blocID = float.Parse(editeur.component[editeur.SelectedBlock[0]].Split(new string[] { "; " }, System.StringSplitOptions.None)[0]);
            if (blocID < 1)
            {
                float triggerID = blocID;
                while (triggerID != (int)triggerID)
                    triggerID = triggerID * 10;
                GetComponent<CreatorManager>().Array((int)triggerID);
            }
            else GetComponent<CreatorManager>().Array(0);
        }
    }
}
