using AngryDash.Image.Reader;
using UnityEngine;

public class DeleteBloc : MonoBehaviour
{

    public Editeur editor;

    private void Update()
    {
        if (editor.file != "") editor.DeleteSelectedBloc(true);
    }

    private int oldMenu;
    public void Switch(UImage_Reader reader)
    {
        var manager = transform.parent.GetComponent<MenuManager>();
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
