using AngryDash.Image.Reader;
using System.Collections;
using System.Collections.Generic;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace AngryDah.Editor.Event
{
    [System.Serializable]
    public class EventField
    {
        public Type accepted;
        public string id;
        public RectTransform transform;
        [HideInInspector] public Vector2 referenceSize;

        public bool CanDrop(Type objectType) { return accepted == objectType | (accepted == Type.action & objectType == Type.conditional); }
    }

    [System.Serializable]
    public class EventParameter
    {
        public string id;
        public RectTransform transform;
    }

    public enum Type
    {
        ///<summary> Triggers actions when validating a defined event </summary>
        trigger,
        /// <summary> Do something in the level </summary>
        action,
        /// <summary> Check the state of something </summary>
        conditional,
        /// <summary> Check operator </summary>
        logicalOperator
    }

    public class EditorEventItem : MonoBehaviour
    {
        public Type type;
        [HideInInspector] public string id;
        [HideInInspector] public Vector2 referenceSize;

        public EventField[] fields;
        Vector2 initialFieldSize = Vector2.zero;
        public EventParameter[] parameters;

        void Start()
        {
            foreach (EventField field in fields)
            {
                if (field.transform == null) Debug.LogError($"[{id}] The field with id {field.id} is null");
                else
                {
                    LayoutElement layout = field.transform.parent.GetComponent<LayoutElement>();
                    field.referenceSize = new Vector2(layout.preferredWidth, layout.preferredHeight);
                    if (field.referenceSize.x > initialFieldSize.x) initialFieldSize.x = field.referenceSize.x;
                    initialFieldSize.y += field.referenceSize.y;
                }
            }
        }

        bool lastEnable = true;
        void Update()
        {
            var elements = transform.FindParent("Elements");
            if (elements != null)
            {
                if (type == Type.trigger)
                {
                    bool enable = elements.parent.GetChild(1).Find($"{id}(Clone)(Clone)") == null;
                    if (enable != lastEnable)
                    {
                        GetComponent<UImage_Reader>().StartAnimating(3, enable ? -1 : 1);
                        foreach (UImage_Reader img in GetComponentsInChildren<UImage_Reader>()) img.StartAnimating(3, enable ? -1 : 1);
                        GetComponent<EditorEventDragHandler>().enabled = enable;
                    }
                    lastEnable = enable;
                }
            }
            else
            {
                EditorEventItem item = EditorEventDragHandler.itemBeingDragged;
                if (item == null) return;
                else if (item.gameObject == gameObject) return;
                else if (transform.IsChildOf(item.transform)) return;
                else if (!GetComponent<RectTransform>().IsOver((RectTransform)item.transform)) return;

                foreach (EventField field in fields)
                {
                    if (field.CanDrop(item.type))
                    {
                        if (field.transform.IsOver(item.transform.position))
                        {
                            item.transform.SetParent(field.transform);
                            UpdateSize();
                        }
                    }
                }
            }
        }

        public void UpdateSize()
        {
            Vector2 totalSize = Vector2.zero;
            foreach (EventField field in fields)
            {
                Vector2 size = Vector2.zero;
                RectTransform fieldParent = (RectTransform)field.transform.parent;
                if (field.transform.childCount > 0)
                {
                    foreach (RectTransform rectTransform in fieldParent)
                    {
                        if (rectTransform == field.transform)
                        {
                            foreach (RectTransform fieldTransform in field.transform)
                            {
                                if (fieldTransform.sizeDelta.x > size.x) size.x = fieldTransform.sizeDelta.x;
                                size.y += fieldTransform.sizeDelta.y;
                            }
                        }
                        else
                        {
                            if (rectTransform.sizeDelta.x > size.x) size.x = rectTransform.sizeDelta.x;
                            size.y += rectTransform.sizeDelta.y;
                        }
                    }

                }
                else size = field.referenceSize;

                if (size.x > totalSize.x) totalSize.x = size.x;
                totalSize.y += size.y;
                fieldParent.sizeDelta = size;
            }

            GetComponent<RectTransform>().sizeDelta = referenceSize - initialFieldSize + totalSize;
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
            foreach (EventField field in fields) LayoutRebuilder.ForceRebuildLayoutImmediate(field.transform);
        }
    }
}
