using UnityEngine;
using UnityEngine.EventSystems;
using Tools;

namespace Editor.Event
{
    public class EditorEventDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public static EditorEventItem itemBeingDragged;
        [HideInInspector] public Transform startParent;
        public Transform visualPanel;

        void Start()
        {
            if (startParent == null) startParent = transform.parent;
            if (visualPanel == null) visualPanel = transform.FindParent("Visual");

            if (startParent.childCount == 1 & transform.IsChildOf(visualPanel.GetChild(0)))
            {
                gameObject.SetActive(false);
                Instantiate(startParent.GetChild(0).gameObject, startParent).SetActive(true);
            }
        }

        //When the user grab the item
        public void OnBeginDrag(PointerEventData eventData)
        {
            startParent = transform.parent;
            itemBeingDragged = GetComponent<EditorEventItem>();
            GetComponent<CanvasGroup>().blocksRaycasts = false;
        }

        //During the traget
        public void OnDrag(PointerEventData eventData) { transform.position = eventData.position; /*Follow the mouse*/ }

        //When the user release the item
        public void OnEndDrag(PointerEventData eventData)
        {
            itemBeingDragged = null;
            GetComponent<CanvasGroup>().blocksRaycasts = true;

            if (startParent.IsChildOf(visualPanel.GetChild(0).GetComponent<UnityEngine.UI.ScrollRect>().content))
            {
                if (visualPanel.GetChild(1).GetComponent<RectTransform>().IsHover((RectTransform)transform))
                {
                    if (!transform.parent.GetComponent<RectTransform>().IsHover(transform.position)) transform.SetParent(visualPanel.GetChild(1));
                    Instantiate(startParent.GetChild(0).gameObject, startParent).SetActive(true);
                }
                else
                {
                    Instantiate(startParent.GetChild(0).gameObject, startParent).SetActive(true);
                    if (startParent.childCount > 2 & transform.parent != startParent) Destroy(gameObject);
                }

                if (startParent.childCount > 2)
                {
                    for (int i = 2; i < startParent.childCount; i++)
                        Destroy(startParent.GetChild(2).gameObject);
                }
            }
            else if (!visualPanel.GetChild(1).GetComponent<RectTransform>().IsHover(transform.position)) Destroy(gameObject);
            else if (startParent == transform.parent & !RectTransformExtensions.IsHover((RectTransform)startParent, transform.position))
            {
                transform.SetParent(visualPanel.GetChild(1));
                startParent.GetComponentInParent<EditorEventItem>().UpdateSize();
            }
        }
    }
}
