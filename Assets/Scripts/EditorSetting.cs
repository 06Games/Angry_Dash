using UnityEngine;
using UnityEngine.UI;

public class EditorSetting : MonoBehaviour
{
    private void Start() { NewStart(); }
    public void NewStart()
    {
        //Default Values
        if (!ConfigAPI.Exists("editor.showCoordinates")) ConfigAPI.SetBool("editor.showCoordinates", true);
        if (!ConfigAPI.Exists("editor.autoSave")) ConfigAPI.SetInt("editor.autoSave", 2);
        if (!ConfigAPI.Exists("editor.hideToolbox")) ConfigAPI.SetBool("editor.hideToolbox", SystemInfo.deviceType == DeviceType.Desktop);

        //Set States
        transform.GetChild(0).GetComponent<Toggle>().isOn = ConfigAPI.GetBool("editor.showCoordinates");
        transform.GetChild(1).GetChild(1).GetComponent<Dropdown>().value = ConfigAPI.GetInt("editor.autoSave");
        transform.GetChild(2).GetComponent<Toggle>().isOn = ConfigAPI.GetBool("editor.hideToolbox");
    }

    public void ShowCoordinates(Toggle toggle) { ConfigAPI.SetBool("editor.showCoordinates", toggle.isOn); }
    public void AutoSave(Dropdown dropdown) { ConfigAPI.SetInt("editor.autoSave", dropdown.value); }
    public void HideToolbox(Toggle toggle) { ConfigAPI.SetBool("editor.hideToolbox", toggle.isOn); }
}
