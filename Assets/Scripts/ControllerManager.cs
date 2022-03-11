using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ControllerManager : MonoBehaviour
{
    public bool Controller;
    private EventSystem eventSystem;
    private Selectable baseSelectable;

    private void Start()
    {
        eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        if (eventSystem.firstSelectedGameObject != null) baseSelectable = eventSystem.firstSelectedGameObject.GetComponent<Selectable>();
    }

    public void SelectButton(Selectable obj)
    {
        if (Controller) obj.Select();
        baseSelectable = obj;
    }

    private void Update()
    {
        if (eventSystem == null)
        {
            eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
            baseSelectable = eventSystem.firstSelectedGameObject?.GetComponent<Selectable>();
        }
        var newController = Input.GetJoystickNames().Length > 0;
        if (newController) newController = !(Input.GetJoystickNames().Length == 1 & string.IsNullOrEmpty(Input.GetJoystickNames()[0]));
        if (newController & Controller == false & baseSelectable != null) baseSelectable.Select();
        if (Controller & eventSystem.currentSelectedGameObject == null & baseSelectable != null) baseSelectable.Select();
        if (Controller & !newController) eventSystem.SetSelectedGameObject(null);
        Controller = newController;

    }
}
