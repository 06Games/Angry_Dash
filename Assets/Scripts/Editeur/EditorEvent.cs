using Editor.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Editor.Event
{
    public class EditorEvent : MonoBehaviour
    {
        public Editeur editor;
        public enum ProgType { visual, textual }
        public ProgType type = ProgType.visual;

        void OnDisable() { editor.bloqueSelect = false; }
        void OnEnable()
        {
            editor.bloqueSelect = true;

            if (type == ProgType.visual) VisualInitialization();
            else Debug.LogError("Unsupported");
        }

        void VisualInitialization()
        {
            Transform visual = transform.GetChild(0);
            Transform elements = visual.GetChild(1);

            string[] ids = new string[] {
                "collision", //trigger
                //"color", //action
                "if", "else" //condition
            };
            foreach (string id in ids)
            {
                GameObject config = Resources.Load<GameObject>($"Events/{id}");
                if (config != null)
                {
                    GameObject Slot = Instantiate(elements.GetChild(0).gameObject, elements);
                    EditorEventItem Item = Instantiate(config, Slot.transform).GetComponent<EditorEventItem>();

                    Slot.name = Item.id = id;
                    Slot.GetComponent<GridLayoutGroup>().cellSize =
                        Slot.GetComponent<RectTransform>().sizeDelta =
                        Item.referenceSize =
                        Item.GetComponent<RectTransform>().sizeDelta;
                    Slot.GetComponent<UImage_Reader>().baseID = Item.GetComponent<UImage_Reader>().baseID;
                    Slot.SetActive(true);
                }
#if UNITY_EDITOR
                else Debug.LogWarning($"<b>{id}</b> has no prefab");
#endif
            }
        }
    }
}
