using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject[] GO;
    public UnityEngine.UI.Selectable[] Buttons;
    public int array;

    void Update()
    {
        for (int i = 0; i < GO.Length; i++)
        {
            if (GO[i] != null)
            {
                GO[i].SetActive(i == array);
                if (Buttons != null)
                {
                    if (Buttons.Length > i)
                        if (Buttons[i] != null) Buttons[i].interactable = !(i == array);
                }
            }
        }
    }

    public void ChangArray(int a)
    {
        array = a;
        transform.GetChild(0).gameObject.SetActive(false);
    }
    public void Array(int a) { array = a; }

    public GameObject selectedObject
    {
        get { return GO[array]; }
        set
        {
            for (int i = 0; i < GO.Length; i++)
            {
                if (GO[i] == value) { array = i; i = GO.Length; }
            }
        }
    }
}
