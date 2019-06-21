using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteBloc : MonoBehaviour
{

    public Editeur editor;

    void Update()
    {
        if (editor.file != "")
        {
            editor.DeleteSelectedBloc(true);
            editor.SelectModeChang(true);
        }
    }

    int oldMenu;
    public void Switch(UImage_Reader reader)
    {
        MenuManager manager = transform.parent.GetComponent<MenuManager>();
        if (manager.array == 4)
        {
            manager.array = oldMenu;
            reader.SetID("native/GUI/editor/deleteOff").Load();
        }
        else
        {
            oldMenu = manager.array;
            manager.Array(4);
            reader.SetID("native/GUI/editor/deleteOn").Load();
        }
    }
}
