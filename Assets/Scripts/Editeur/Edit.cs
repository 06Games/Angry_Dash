using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EditMenus
{
    public float[] BlockID;
    public int Object;
}

public class Edit : MonoBehaviour
{
    public Editeur editeur;
    public GameObject MobileUtilities;
    public EditMenus[] menus;

    public void EnterToEdit()
    {
        editeur.NoBlocSelectedPanel.SetActive(editeur.SelectedBlock.Length == 0);
        try { GetComponent<MenuManager>().selectedObject.GetComponent<MenuManager>().Array(0); } catch { EnterToEdit(); return; }

#if UNITY_STANDALONE || UNITY_EDITOR
        MobileUtilities.SetActive(false);
#else
        MobileUtilities.SetActive(true);
#endif
    }

    private void Update()
    {
        if (editeur.SelectedBlock.Length > 0)
        {
            float blocID = -1;
            if (editeur.SelectedBlock[0] < editeur.level.blocks.Length) blocID = editeur.level.blocks[editeur.SelectedBlock[0]].id;

            bool find = false;
            for (int i = 0; i < menus.Length & !find; i++)
            {
                for (int b = 0; b < menus[i].BlockID.Length & !find; b++)
                {
                    float bID = menus[i].BlockID[b];
                    if ((bID < 0 & bID * -1 <= blocID) | (bID == blocID & bID >= 0))
                    {
                        GetComponent<MenuManager>().Array(menus[i].Object);
                        find = true;
                    }
                }
            }
            if (!find) GetComponent<MenuManager>().Array(0);
        }
    }
}
