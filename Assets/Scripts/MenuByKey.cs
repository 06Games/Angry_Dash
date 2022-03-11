using System;
using UnityEngine;

[RequireComponent(typeof(MenuManager))]
public class MenuByKey : MonoBehaviour
{

    public int[] menu;
    public string[] keys;

    private void Update()
    {
        for (var i = 0; i < menu.Length; i++)
        {
            var k = keys[i].Split(new[] { " | " }, StringSplitOptions.None);
            for (var v = 0; v < k.Length; v++)
            {
                var key = (KeyCode)Enum.Parse(typeof(KeyCode), k[v]);
                if (Input.GetKeyDown(key))
                    GetComponent<MenuManager>().array = menu[i];
            }
        }
    }
}
