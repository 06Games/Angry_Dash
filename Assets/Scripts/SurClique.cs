using UnityEngine;
using System;
using UnityEngine.Events;

public class SurClique : MonoBehaviour {
    
    [SerializeField] private OnCompleteEvent OnCompleteMethods = new OnCompleteEvent();
    public enum QuelClique
    {
        Gauche,
        Droit
    }
    bool clic;
    public bool WantKeyUp = false;

    public QuelClique SurQuelClique;
	void Update () {
#if UNITY_STANDALONE || UNITY_EDITOR
        if (SurQuelClique == QuelClique.Gauche & !WantKeyUp)
            clic = Input.GetKey(KeyCode.Mouse0);
        else if(SurQuelClique == QuelClique.Gauche)
            clic = Input.GetKeyDown(KeyCode.Mouse0);
        else if(!WantKeyUp)
            clic = Input.GetKey(KeyCode.Mouse1);
        else clic = Input.GetKeyDown(KeyCode.Mouse1);

        if (clic)
            OnCompleteMethods.Invoke();
#elif UNITY_ANDROID || UNITY_IOS
        if (SurQuelClique == QuelClique.Gauche)
            SimpleGesture.OnShortTap(Tap);
        else SimpleGesture.OnLongTap(Tap);
#endif
    }

    public void Tap() { OnCompleteMethods.Invoke(); }

    [Serializable]public class OnCompleteEvent : UnityEvent { }
}
