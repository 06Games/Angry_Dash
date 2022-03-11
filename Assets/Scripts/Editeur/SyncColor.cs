using UnityEngine;
using UnityEngine.UI;

public class SyncColor : MonoBehaviour
{

    public enum SyncColorType { Button, Dropdown, Image }
    public SyncColorType Type = SyncColorType.Image;

    public Image[] imgChilds;
    public Text[] textChilds;
    public Color32 actualColor = new Color32(255, 255, 255, 255);

    private void Update()
    {
        if (Type == SyncColorType.Image)
        {
            var main = GetComponent<Image>();
            if (main.color != actualColor)
            {
                actualColor = main.color;
            }
        }
        else if (Type == SyncColorType.Button)
        {
            var main = GetComponent<Button>();
            if (main.interactable)
                actualColor = main.colors.normalColor;
            else actualColor = main.colors.disabledColor;
        }
        else if (Type == SyncColorType.Dropdown)
        {
            var main = GetComponent<Dropdown>();
            if (main.interactable)
                actualColor = main.colors.normalColor;
            else actualColor = main.colors.disabledColor;
        }


        for (var i = 0; i < imgChilds.Length; i++)
            imgChilds[i].color = actualColor;
        for (var i = 0; i < textChilds.Length; i++)
            textChilds[i].color = actualColor;
    }
}
