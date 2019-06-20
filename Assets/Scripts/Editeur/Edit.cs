using System.Linq;
using UnityEngine;

[System.Serializable]
public class EditMenus
{
    public Level.Block.Type Type;
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
        try
        {
            GameObject selected = GetComponent<MenuManager>().selectedObject;
            if(selected != null) selected.GetComponent<MenuManager>().Array(0);
        }
        catch (System.Exception e) { Debug.LogError(e); return; }

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
            Level.Block block = new Level.Block();
            if (editeur.SelectedBlock[0] < editeur.level.blocks.Length) block = editeur.level.blocks[editeur.SelectedBlock[0]];

            EditMenus menu = menus.Where(f => f.Type == block.type).FirstOrDefault();
            if (menu != null) GetComponent<MenuManager>().Array(menu.Object);
            else if (block.id == 0.4F) GetComponent<MenuManager>().Array(2); //Compatibility
            else transform.parent.GetComponent<MenuManager>().Array(3);
        }
        else
        {
            GetComponent<MenuManager>().Array(999);
            editeur.NoBlocSelectedPanel.SetActive(true);
        }
    }
}
