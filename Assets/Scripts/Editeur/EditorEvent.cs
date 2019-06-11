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
                "color", //action
                "if", "else" //condition
            };
            foreach (string id in ids)
            {
                Object config = Resources.Load($"Events/{id}");
                if (config != null)
                {
                    EventInfos eventInfos = FileFormat.XML.Utils.XMLtoClass<EventInfos>(config.ToString());
                    eventInfos.id = id;

                    GameObject go = Instantiate(elements.GetChild(0).gameObject, elements);
                    go.name = id;
                    go.GetComponent<UImage_Reader>().SetID($"native/GUI/editor/edit/event/{eventInfos.type}Background").Load();
                    float multiplier = 400F / eventInfos.referenceSize.x;
                    if (multiplier == 0 | float.IsNaN(multiplier)) multiplier = 0.5F;
                    go.GetComponent<GridLayoutGroup>().cellSize =
                        go.GetComponent<RectTransform>().sizeDelta =
                        go.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta =
                        eventInfos.referenceSize * multiplier;

                    EditorEventItem eventItem = go.transform.GetChild(0).GetComponent<EditorEventItem>();
                    eventItem.infos = eventInfos;
                    eventItem.GetComponent<UImage_Reader>().SetID($"native/GUI/editor/edit/event/{eventInfos.type}Background").Load();
                    eventItem.Initialize();

                    go.SetActive(true);
                }
#if UNITY_EDITOR
                else Debug.LogWarning($"<b>{id}</b> has no config file");
#endif
            }
        }
    }
}
