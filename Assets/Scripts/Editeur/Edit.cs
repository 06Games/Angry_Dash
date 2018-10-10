﻿using System.Collections;
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
    public GameObject MultiSelectBtn;
    public EditMenus[] menus;

    public void EnterToEdit()
    {
        editeur.NoBlocSelectedPanel.SetActive(editeur.SelectedBlock.Length == 0);
        try { transform.GetChild(GetComponent<CreatorManager>().array).GetComponent<CreatorManager>().Array(0); } catch { EnterToEdit(); return; }

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
            float blocID = -1;
            try { blocID = float.Parse(editeur.GetBlocStatus("ID", editeur.SelectedBlock[0])); } catch { }

            bool find = false;
            for (int i = 0; i < menus.Length & !find; i++)
            {
                for (int b = 0; b < menus[i].BlockID.Length & !find; b++)
                {
                    float bID = menus[i].BlockID[b];
                    if ((bID < 0 & bID*-1 <= blocID) | (bID == blocID & bID >= 0))
                    {
                        GetComponent<CreatorManager>().Array(menus[i].Object);
                        find = true;
                    }
                }
            }
            if (!find) GetComponent<CreatorManager>().Array(0);
        }
    }
}
