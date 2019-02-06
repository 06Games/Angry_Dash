using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorSetting : MonoBehaviour
{
    void Start() { NewStart(); }
    public void NewStart()
    {
        //Default Values
        if (!ConfigAPI.Exists("editor.showCoordinates")) ConfigAPI.SetBool("editor.showCoordinates", true);
        if (!ConfigAPI.Exists("editor.autoSave")) ConfigAPI.SetInt("editor.autoSave", 2);
        
        //Set States
        transform.GetChild(0).GetComponent<Toggle>().isOn = ConfigAPI.GetBool("editor.showCoordinates");
        transform.GetChild(1).GetComponent<Dropdown>().value = ConfigAPI.GetInt("editor.autoSave");
    }
    
    public void ShowCoordinates(Toggle toggle) { ConfigAPI.SetBool("editor.showCoordinates", toggle.isOn); }
    public void AutoSave(Dropdown dropdown) { ConfigAPI.SetInt("editor.autoSave", dropdown.value); }
}
