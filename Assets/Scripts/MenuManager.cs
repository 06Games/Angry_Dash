using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject[] GO;
    public Selectable[] Buttons;
    public int array;

    private void Update()
    {
        for (var i = 0; i < GO.Length; i++)
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
        get
        {
            if (array >= 0 & array < GO.Length) return GO[array];
            return null;
        }
        set
        {
            for (var i = 0; i < GO.Length; i++)
            {
                if (GO[i] == value) { array = i; i = GO.Length; }
            }
        }
    }
}
