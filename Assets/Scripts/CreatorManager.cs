using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatorManager : MonoBehaviour
{
    public GameObject[] GO;
    public UnityEngine.UI.Selectable[] Buttons;
    public int array;

    void Update()
    {
        for (int i = 0; i < GO.Length; i++)
        {
            GO[i].SetActive(i == array);
            if (Buttons != null)
            {
                if (Buttons.Length > i)
                    Buttons[i].interactable = !(i == array);
            }
        }
    }

    public void ChangArray(int a)
    {
        array = a;
        transform.GetChild(0).gameObject.SetActive(false);
    }
    public void Array(int a) { array = a; }
}
