using UnityEngine;
using UnityEngine.UI;

public class ControllerManager : MonoBehaviour
{
    public bool Controller = false;
    UnityEngine.EventSystems.EventSystem eventSystem;
    Selectable baseSelectable;

    void Start()
    {
        eventSystem = GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem.firstSelectedGameObject != null) baseSelectable = eventSystem.firstSelectedGameObject.GetComponent<Selectable>();
    }

    public void SelectButton(Selectable obj)
    {
        if (Controller) obj.Select();
        baseSelectable = obj;
    }

    void Update()
    {
        if (eventSystem == null)
        {
            eventSystem = GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>();
            baseSelectable = eventSystem.firstSelectedGameObject.GetComponent<Selectable>();
        }
        bool newController = Input.GetJoystickNames().Length > 0;
        if (newController) newController = !(Input.GetJoystickNames().Length == 1 & string.IsNullOrEmpty(Input.GetJoystickNames()[0]));
        if (newController == true & Controller == false & baseSelectable != null) baseSelectable.Select();
        if (Controller & eventSystem.currentSelectedGameObject == null & baseSelectable != null) baseSelectable.Select();
        if (Controller & !newController) eventSystem.SetSelectedGameObject(null);
        Controller = newController;

    }
}
