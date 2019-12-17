using AngryDash.Image.Reader;
using UnityEngine;

public class DeleteBloc : MonoBehaviour
{

    public Editeur editor;

    void Update()
    {
        if (editor.file != "") editor.DeleteSelectedBloc(true);
    }

    int oldMenu;
    public void Switch(UImage_Reader reader)
    {
        MenuManager manager = transform.parent.GetComponent<MenuManager>();
        if (manager.array == 4)
        {
            manager.array = oldMenu;
            editor.SelectModeChang(false);
            reader.SetID("native/GUI/editor/deleteOff").LoadAsync();
        }
        else
        {
            oldMenu = manager.array;
            manager.Array(4);
            editor.SelectModeChang(true);
            reader.SetID("native/GUI/editor/deleteOn").LoadAsync();
        }
    }
}
