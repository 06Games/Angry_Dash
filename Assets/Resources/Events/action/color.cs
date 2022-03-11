using UnityEngine;
using UnityEngine.UI;

namespace AngryDash.Editor.Event.Components
{
    public class color : MonoBehaviour
    {
        public InputField R;
        public InputField G;
        public InputField B;

        public ColorPicker picker;
        private GameObject pickedDisablePanel;

        private void Start() { pickedDisablePanel = picker.transform.Find("Disabled").gameObject; }
        private void Update() { pickedDisablePanel.SetActive(!R.interactable); }

        public void rgbChanged()
        {
            var color = new Color32();
            byte.TryParse(R.text, out color.r);
            byte.TryParse(G.text, out color.g);
            byte.TryParse(B.text, out color.b);
            picker.CurrentColor = color;
        }
        public void pickerChanged()
        {
            Color32 color = picker.CurrentColor;
            R.text = color.r.ToString();
            G.text = color.g.ToString();
            B.text = color.b.ToString();
        }
    }
}
