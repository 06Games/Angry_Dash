using UnityEngine;
using UnityEngine.EventSystems;
using Tools;

namespace AngryDah.Editor.Event
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

            if (startParent.IsChildOf(visualPanel.GetChild(0).GetComponent<UnityEngine.UI.ScrollRect>().content)) //The item has just been spawned
            {
                if (visualPanel.GetChild(1).GetComponent<RectTransform>().IsOver((RectTransform)transform)) //The item is over the code panel
                {
                    if (!transform.parent.GetComponent<RectTransform>().IsOver(transform.position)) transform.SetParent(visualPanel.GetChild(1));
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
            else if (!visualPanel.GetChild(1).GetComponent<RectTransform>().IsOver(transform.position)) //The item isn't hover the code panel
            {
                if (transform.parent.IsChildOf(visualPanel.GetChild(1)) & visualPanel.GetChild(1) != transform.parent) //The item was in the code
                {
                    transform.SetParent(visualPanel.GetChild(1)); //Remove the item from its parent
                    startParent.GetComponentInParent<EditorEventItem>().UpdateSize(); //Refresh the parent
                }
                Destroy(gameObject); //Destroy the item
            }
            else if (startParent == transform.parent & !RectTransformExtensions.IsOver((RectTransform)startParent, transform.position)) //The item has exit its parent
            {
                transform.SetParent(visualPanel.GetChild(1));
                startParent.GetComponentInParent<EditorEventItem>().UpdateSize();
            }
        }
    }
}
