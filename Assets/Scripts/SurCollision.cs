using System;
using UnityEngine;
using UnityEngine.Events;

public class SurCollision : MonoBehaviour
{
    public bool Collision;

    private void OnTriggerExit(Collider other)
    {
        Collision = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Collision = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Collision = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Collision = true;
    }

    private void Update()
    {
        if (Collision)
            OnCompleteMethods.Invoke();
    }



    [SerializeField] private OnCompleteEvent OnCompleteMethods = new OnCompleteEvent();
    [Serializable] public class OnCompleteEvent : UnityEvent { }

    public void Detruire(GameObject go = null)
    {
        if (go == null)
            go = gameObject;

        Destroy(go);
    }

    public void Scene(string Scène)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(Scène); //alors charger la scene
    }
}
