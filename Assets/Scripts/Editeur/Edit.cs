using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edit : MonoBehaviour {

    public Editeur editeur;

    public void EnterToEdit()
    {
        editeur.NoBlocSelectedPanel.SetActive(editeur.SelectedBlock == -1);
    }
}
