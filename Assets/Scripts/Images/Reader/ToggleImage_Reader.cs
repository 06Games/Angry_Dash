using UnityEngine;
using UnityEngine.UI;

namespace AngryDash.Image.Reader
{
    public class ToggleImage_Reader : MonoBehaviour
    {
        public string onID;
        public string offID;

        private void OnEnable()
        {
            var toggle = GetComponent<Toggle>();
            toggle.toggleTransition = Toggle.ToggleTransition.None;
            toggle.transition = Selectable.Transition.None;
            if (toggle.graphic != null) Destroy(toggle.graphic.gameObject);

            if (toggle.targetGraphic == null) toggle.targetGraphic = gameObject.GetComponentInChildren<Graphic>();
            var reader = toggle.targetGraphic.GetComponent<UImage_Reader>();
            if (reader == null) reader = toggle.targetGraphic.gameObject.AddComponent<UImage_Reader>();
            reader.enabled = reader.autoChange = false;
            reader.animationChanged = null;

            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener((on) =>
            {
                reader.animationChanged = anChange;
                if (!reader.StartAnimating(2)) anChange(0);
            });

            anChange(reader.animationIndex);
            void anChange(int animation)
            {
                if (animation == 0)
                {
                    reader.animationChanged = null;
                    reader.SetID(toggle.isOn ? onID : offID).LoadAsync();
                }
            }
        }
    }
}
