using System;
using System.Collections.Generic;
using AngryDash.Image.Reader;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace AngryDash.Editor.Event
{
    [Serializable]
    public class EventField
    {
        public Type accepted;
        public string id;
        public RectTransform transform;
        [HideInInspector] public Vector2 referenceSize;

        public bool CanDrop(Type objectType) { return accepted == objectType | (accepted == Type.action & objectType == Type.conditional); }
    }

    [Serializable]
    public class EventParameter
    {
        public string id;
        public Selectable selectable;
        public enum Type { Text, Number, Bool }
        public KeyValuePair<Type, string> Value
        {
            get
            {
                if (selectable == null) return new KeyValuePair<Type, string>();
                if (selectable.GetComponent<InputField>() != null)
                {
                    var IF = (InputField)selectable;
                    var type = IF.contentType.In(InputField.ContentType.IntegerNumber, InputField.ContentType.DecimalNumber, InputField.ContentType.Pin, InputField.ContentType.Custom) ? Type.Number : Type.Text;
                    return new KeyValuePair<Type, string>(type, IF.text);
                }

                if (selectable.GetComponent<Toggle>() != null) return new KeyValuePair<Type, string>(Type.Bool, ((Toggle)selectable).isOn.ToString());
                return new KeyValuePair<Type, string>();
            }
            set
            {
                if (selectable == null) return;
                if (selectable.GetComponent<InputField>() != null) ((InputField)selectable).text = value.Value;
                else if (selectable.GetComponent<Toggle>() != null) { bool.TryParse(value.Value, out var on); ((Toggle)selectable).isOn = on; }
            }
        }
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
        public string methodName;
        [HideInInspector] public Vector2 referenceSize;

        public EventField[] fields;
        private Vector2 initialFieldSize = Vector2.zero;
        public EventParameter[] parameters;

        private void Start()
        {
            foreach (var field in fields)
            {
                if (field.transform == null) Debug.LogError($"[{id}] The field with id {field.id} is null");
                else
                {
                    var layout = field.transform.parent.GetComponent<LayoutElement>();
                    field.referenceSize = new Vector2(layout.preferredWidth, layout.preferredHeight);
                    if (field.referenceSize.x > initialFieldSize.x) initialFieldSize.x = field.referenceSize.x;
                    initialFieldSize.y += field.referenceSize.y;
                }
            }
        }

        private bool lastEnable = true;

        private void Update()
        {
            var elements = transform.FindParent("Elements");
            foreach (var parameter in parameters) parameter.selectable.interactable = elements == null;
            if (elements != null)
            {
                if (type == Type.trigger)
                {
                    var enable = elements.parent.parent.GetChild(1).Find($"{id}(Clone)(Clone)") == null;
                    if (enable != lastEnable)
                    {
                        GetComponent<UImage_Reader>().StartAnimating(3, enable ? -1 : 1);
                        foreach (var img in GetComponentsInChildren<UImage_Reader>()) img.StartAnimating(3, enable ? -1 : 1);
                        GetComponent<EditorEventDragHandler>().enabled = enable;
                    }
                    lastEnable = enable;
                }
            }
            else
            {
                var item = EditorEventDragHandler.itemBeingDragged;
                if (item == null) return;
                if (item.gameObject == gameObject) return;
                if (transform.IsChildOf(item.transform)) return;
                if (!GetComponent<RectTransform>().IsOver((RectTransform)item.transform)) return;

                foreach (var field in fields)
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
            var totalSize = Vector2.zero;
            foreach (var field in fields)
            {
                var size = Vector2.zero;
                var fieldParent = (RectTransform)field.transform.parent;
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
            foreach (var field in fields) LayoutRebuilder.ForceRebuildLayoutImmediate(field.transform);
        }
    }
}
