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

#if UNITY_STANDALONE || UNITY_EDITOR
        MultiSelectBtn.SetActive(false);
#else
        MultiSelectBtn.SetActive(true);
#endif
    }
}
