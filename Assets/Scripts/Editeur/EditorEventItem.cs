using System.Collections;
using System.Collections.Generic;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Editor.Event
{
    [System.Serializable]
    public class EventAccepted
    {
        public EventInfos.Type type;
        public Rect position;
    }

    [System.Serializable]
    public class EventText
    {
        public string category;
        public string id;
        public string dontExists;

        public Rect position;
        public TextAnchor anchor;
    }

    [System.Serializable]
    public class EventInfos
    {
        public enum Type
        {
            trigger,
            action,
            conditional,
            logicalOperator
        }
        public Type type;
        public string id;
        public Vector2 referenceSize;
        public EventAccepted[] accepted;

        public EventText[] text;
    }

    public class EditorEventItem : MonoBehaviour
    {
        public EventInfos infos;

        Vector2 element { get { return (Vector2)transform.position - (GetComponent<RectTransform>().sizeDelta / 2); } }
        float multiplier { get { return 400F / infos.referenceSize.x; } }

        public void Initialize()
        {
            foreach (EventText text in infos.text)
            {
                Rect textRect = text.position.Multiply(multiplier);
                GameObject go = Instantiate(transform.GetChild(0).GetChild(0).gameObject, transform.GetChild(0));
                go.GetComponent<RectTransform>().SetRect(textRect);
                go.GetComponent<Text>().text = LangueAPI.Get(text.category, text.id, text.dontExists);
                go.GetComponent<Text>().alignment = text.anchor;
                go.SetActive(true);
            }
            foreach(EventAccepted accepted in infos.accepted)
            {
                GameObject go = Instantiate(transform.GetChild(1).GetChild(0).gameObject, transform.GetChild(1));
                go.GetComponent<RectTransform>().SetStretchSize(accepted.position.Multiply(multiplier));
                go.name = $"{accepted.type} Field";
                go.GetComponent<UImage_Reader>().SetID($"native/GUI/editor/edit/event/{infos.type}Field").Load();
                go.SetActive(true);
            }
        }

        void Update()
        {
            if (transform.FindParent("Elements") != null) return;

            EditorEventItem item = EditorEventDragHandler.itemBeingDragged;
            if (item == null) return;
            else if (item.gameObject == gameObject) return;
            foreach (EventAccepted accepted in infos.accepted)
            {
                if (accepted.type == item.infos.type | (accepted.type == EventInfos.Type.action & item.infos.type == EventInfos.Type.conditional))
                {
                    Rect acceptedRect = accepted.position.Multiply(multiplier);
                    Rect rect = new Rect(acceptedRect.x + element.x, acceptedRect.y + element.y, acceptedRect.width, acceptedRect.height);
                    if (rect.Contains(item.transform.position))
                    {
                        RectTransform parent = (RectTransform)transform.GetChild(1).Find($"{accepted.type} Field");
                        item.transform.SetParent(parent);
                        LayoutRebuilder.ForceRebuildLayoutImmediate(parent);
                        GetComponent<RectTransform>().sizeDelta = itemSize(parent, accepted);
                    }
                    else if (item.transform.IsChildOf(transform))
                    {
                        RectTransform parent = (RectTransform)transform.GetChild(1).Find($"{accepted.type} Field");
                        item.transform.SetParent(item.GetComponent<EditorEventDragHandler>().visualPanel.GetChild(2));
                        if (parent.childCount > 0)
                        {
                            GetComponent<RectTransform>().sizeDelta = itemSize(parent, accepted);
                            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
                        }
                        else GetComponent<RectTransform>().sizeDelta = infos.referenceSize * multiplier;
                    }
                }
            }
        }

        Vector2 itemSize(RectTransform field, EventAccepted accepted)
        {
            Vector2 size = Vector2.zero;
            foreach (RectTransform rectTransform in field)
            {
                if (rectTransform.sizeDelta.x > size.x) size.x = rectTransform.sizeDelta.x;
                size.y += rectTransform.sizeDelta.y;
            }
            return infos.referenceSize * multiplier - accepted.position.size * multiplier + size;
        }
    }
}
