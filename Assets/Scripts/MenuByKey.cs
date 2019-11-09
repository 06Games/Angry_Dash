using UnityEngine;

[RequireComponent(typeof(MenuManager))]
public class MenuByKey : MonoBehaviour
{

    public int[] menu;
    public string[] keys;

    void Update()
    {
        for (int i = 0; i < menu.Length; i++)
        {
            string[] k = keys[i].Split(new string[] { " | " }, System.StringSplitOptions.None);
            for (int v = 0; v < k.Length; v++)
            {
                KeyCode key = (KeyCode)System.Enum.Parse(typeof(KeyCode), k[v]);
                if (Input.GetKeyDown(key))
                    GetComponent<MenuManager>().array = menu[i];
            }
        }
    }
}
