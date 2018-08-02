using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CreatorManager))]
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
                    GetComponent<CreatorManager>().array = menu[i];
            }
        }
    }
}
